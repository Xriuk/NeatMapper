using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// <see cref="IMapper"/> which converts entities to and from their keys, even composite keys
	/// as <see cref="Tuple"/> or <see cref="ValueTuple"/>.<br/>
	/// Also supports collections (not nested).<br/>
	/// When mapping keys to entities, entities will be searched locally in the <see cref="DbContext"/> first,
	/// otherwise a query to the db will be made
	/// </summary>
	public sealed class EntityFrameworkCoreMapper : IMapper, IMapperCanMap {
		private static readonly ConstructorInfo MappingException_constructor = typeof(MappingException).GetConstructor(new[] { typeof(Exception), typeof((Type, Type)) })
			?? throw new InvalidOperationException("Could not find MappingException()");
		private static readonly ConstructorInfo ValueTuple_Type_Type_constructor = TupleUtils.GetValueTupleConstructor(new[] { typeof(Type), typeof(Type) });
		private static readonly MethodInfo Queryable_Where = typeof(Queryable).GetMethods().First(m => {
			if(m.Name != nameof(Queryable.Where))
				return false;
			var parameters = m.GetParameters();
			if(parameters.Length == 2 && parameters[1].ParameterType.IsGenericType) {
				var delegateType = parameters[1].ParameterType.GetGenericArguments()[0];
				if(delegateType.IsGenericType && delegateType.GetGenericTypeDefinition() == typeof(Func<,>))
					return true;
			}

			return false;
		});
		private static readonly MethodInfo EntityFrameworkQueryableExtensions_Load = typeof(EntityFrameworkQueryableExtensions).GetMethod(nameof(EntityFrameworkQueryableExtensions.Load))
			?? throw new InvalidOperationException("Could not find EntityFrameworkQueryableExtensions.Load<T>()");
		private static readonly MethodInfo Queryable_FirstOrDefault = typeof(Queryable).GetMethods().First(m => m.Name == nameof(Queryable.FirstOrDefault) && m.GetParameters().Length == 1);
		private static readonly MethodInfo Enumerable_ToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))
			?? throw new InvalidOperationException("Could not find Enumerable.ToArray<T>()");


		private readonly IModel _model;
		private readonly Type _dbContextType;
		private readonly IServiceProvider _serviceProvider;
		private readonly EntityFrameworkCoreOptions _entityFrameworkCoreOptions;
		private readonly IDictionary<(Type, Type), Delegate> _entityToKeyCache = new Dictionary<(Type, Type), Delegate>();
		private readonly IDictionary<(Type, Type), Delegate> _keyToValuesCache = new Dictionary<(Type, Type), Delegate>();

		public EntityFrameworkCoreMapper(
			IModel model,
			Type dbContextType,
			IServiceProvider serviceProvider,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			EntityFrameworkCoreOptions?
#else
			EntityFrameworkCoreOptions
#endif
			entityFrameworkCoreOptions = null) {

			_model = model ?? throw new ArgumentNullException(nameof(model));
			_dbContextType = dbContextType ?? throw new ArgumentNullException(nameof(dbContextType));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_entityFrameworkCoreOptions = entityFrameworkCoreOptions ?? new EntityFrameworkCoreOptions();

#if !NET5_0 && !NETCOREAPP3_1
			if (_serviceProvider.GetService<IServiceProviderIsService>()?.IsService(dbContextType) == false)
				throw new ArgumentException($"The provided IServiceProvider does not support the DbContext of type {_dbContextType.FullName ?? _dbContextType.Name}");
#endif
		}


		#region IMapper methods
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			Map(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source != null && !sourceType.IsAssignableFrom(source.GetType()))
				throw new ArgumentException($"Object of type {source.GetType().FullName ?? source.GetType().Name} is not assignable to type {sourceType.FullName ?? sourceType.Name}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (!CanMapNew(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			(Type From, Type To)? collectionElementTypes = destinationType.IsEnumerable() && destinationType != typeof(string) ?
				((Type From, Type To)?)(sourceType.GetEnumerableElementType(), destinationType.GetEnumerableElementType()) :
				null;

			bool entityToKey;
			if(collectionElementTypes != null) 
				entityToKey = collectionElementTypes.Value.To.IsKeyType() || collectionElementTypes.Value.To.IsCompositeKeyType();
			else
				entityToKey = destinationType.IsKeyType() || destinationType.IsCompositeKeyType();

			(Type From, Type To) types = collectionElementTypes ?? (sourceType, destinationType);

			try {
				// Check if we are mapping an entity to a key or vice-versa
				object result;
				if(entityToKey) {
					// Create and cache the delegate if needed
					Delegate entityToKeyMap;
					lock (_entityToKeyCache) {
						if (!_entityToKeyCache.TryGetValue(types, out entityToKeyMap)) {
							var entityParam = Expression.Parameter(types.From, "entity");
							var modelEntity = _model.FindEntityType(types.From);
							var key = modelEntity.FindPrimaryKey();
							Expression body;
							if (key.Properties.Count == 1) {
								// entity.Id
								body = Expression.Property(entityParam, key.Properties[0].PropertyInfo);
							}
							else if (!types.To.IsTuple()) { // ValueTuple or nullable ValueTuple
								// new ValueTuple<...>(entity.Key1, ...)
								body = Expression.New(
									TupleUtils.GetValueTupleConstructor(!types.To.IsNullable() ? types.To.GetGenericArguments() : types.To.GetGenericArguments()[0].GetGenericArguments()),
									key.Properties.Select(p => Expression.Property(entityParam, p.PropertyInfo)));
							}
							else {
								// new Tuple<...>(entity.Key1, ...)
								body = Expression.New(
									TupleUtils.GetTupleConstructor(types.To.GetGenericArguments()),
									key.Properties.Select(p => Expression.Property(entityParam, p.PropertyInfo)));
							}

							// (KEY?)entity.Id or (KEY?)(entity.Key1, ...)
							if (types.To.IsNullable())
								body = Expression.Convert(body, typeof(Nullable<>).MakeGenericType(body.Type));

							// entity != null ? KEY : default(KEY)
							body = Expression.Condition(
								Expression.NotEqual(entityParam, Expression.Constant(null, entityParam.Type)),
								body,
								Expression.Default(body.Type));

							// Add try/catch for mapping errors
							var excParam = Expression.Parameter(typeof(Exception), "e");
							body = Expression.TryCatch(body, Expression.Catch(excParam, Expression.Throw(Expression.New(MappingException_constructor,
								excParam,
								Expression.New(ValueTuple_Type_Type_constructor, Expression.Constant(types.From), Expression.Constant(types.To))), body.Type)));
							entityToKeyMap = Expression.Lambda(typeof(Func<,>).MakeGenericType(types.From, body.Type), body, entityParam).Compile();
							_entityToKeyCache.Add(types, entityToKeyMap);
						}
					}

					// Check if we are mapping a collection or just a single entity
					if (collectionElementTypes != null) { 
						if(source is IEnumerable sourceEnumerable) { 
							var destination = ObjectFactory.CreateCollection(destinationType);
							var addMethod = ObjectFactory.GetCollectionAddMethod(destination);

							foreach (var element in sourceEnumerable) {
								addMethod.Invoke(destination, new object[] { entityToKeyMap.DynamicInvoke(element) });
							}

							result = ObjectFactory.ConvertCollectionToType(destination, destinationType);
						}
						else if(source == null)
							return null;
						else
							throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
					}
					else
						result = entityToKeyMap.DynamicInvoke(source);
				}
				else {
					// Retrieve the db context from the services
					DbContext db;
					var overrideOptions = mappingOptions?.GetOptions<MapperOverrideMappingOptions>();
					if(overrideOptions?.ServiceProvider != null) {
						try {
							db = overrideOptions?.ServiceProvider.GetRequiredService(_dbContextType) as DbContext;
						}
						catch {
							db = null;
						}
					}
					else
						db = null;
					if(db == null) {
						try {
							db = _serviceProvider.GetRequiredService(_dbContextType) as DbContext;
						}
						catch {
							db = null;
						}
						if (db == null)
							throw new InvalidOperationException($"Could not retrieve a DbContext of type {_dbContextType.FullName ?? _dbContextType.Name}");
					}

					// Create and cache the delegate if needed
					Delegate keyToValuesMap;
					lock (_keyToValuesCache) {
						if (!_keyToValuesCache.TryGetValue(types, out keyToValuesMap)) {
							var keyParam = Expression.Parameter(types.From, "key");
							var key = _model.FindEntityType(types.To).FindPrimaryKey();
							Expression body;
							if (key.Properties.Count == 1) {
								// new object[]{ (object)key }
								body = Expression.NewArrayInit(typeof(object), Expression.Convert(keyParam, typeof(object)));
							}
							else if (!types.From.IsNullable()) {
								// new object[]{ (object)key.Item1, ... }
								body = Expression.NewArrayInit(typeof(object),
									Enumerable.Range(1, key.Properties.Count)
										.Select(n => Expression.Convert(Expression.PropertyOrField(keyParam, "Item" + n), typeof(object))));
							}
							else {
								var nonNullableType = types.From.GetGenericArguments()[0];
								// new object[]{ (object)(SOURCE)key.Item1, ... }
								body = Expression.NewArrayInit(typeof(object),
									Enumerable.Range(1, key.Properties.Count)
										.Select(n => Expression.Convert(Expression.PropertyOrField(Expression.Convert(keyParam, nonNullableType), "Item" + n), typeof(object))));
							}

							// key != null ? KEY : default(KEY)
							if (!types.From.IsValueType) { 
								body = Expression.Condition(
									Expression.NotEqual(keyParam, Expression.Constant(null, keyParam.Type)),
									body,
									Expression.Default(body.Type));
							}

							keyToValuesMap = Expression.Lambda(typeof(Func<,>).MakeGenericType(types.From, body.Type), body, keyParam).Compile();
							_keyToValuesCache.Add(types, keyToValuesMap);
						}
					}

					var retrievalMode = mappingOptions.GetOptions<EntityFrameworkCoreMappingOptions>()?.EntitiesRetrievalMode
						?? _entityFrameworkCoreOptions.EntitiesRetrievalMode;

					// Check if we are mapping a collection or just a single entity
					if (collectionElementTypes != null) {
						if (source is IEnumerable sourceEnumerable) {
							var key = _model.FindEntityType(types.To).FindPrimaryKey();
							var dbSet = db.GetType().GetMethod(nameof(DbContext.Set)).Invoke(db, null)
								?? throw new InvalidOperationException("Cannot retrieve DbSet<T>");
							var local = dbSet.GetType().GetProperty(nameof(DbSet<object>.Local)).GetValue(dbSet) as IEnumerable
								?? throw new InvalidOperationException("Cannot retrieve DbSet<T>.Local");

							// Retrieve tracked local entities and create expressions for missing
							EntityMappingInfo[] localsAndPredicates = sourceEnumerable
								.Cast<object>()
								.Select(sourceElement => {
									if (sourceElement == null)
										return new EntityMappingInfo();

									var keyValues = keyToValuesMap.DynamicInvoke(sourceElement) as object[]
										?? throw new InvalidOperationException($"Invalid key(s) returned for entity of type {types.To.FullName ?? types.To.Name}");

									var expr = GetEntityExpression(keyValues, key);
									var deleg = expr.Compile();

									object localEntity;
									if(retrievalMode != EntitiesRetrievalMode.Remote) {
										localEntity = local
											.Cast<object>()
											.FirstOrDefault(e => (bool)deleg.DynamicInvoke(e));

										if(retrievalMode == EntitiesRetrievalMode.LocalOrAttach)
											AttachEntity(db, ref localEntity, keyValues, key);
									}
									else
										localEntity = null;

									return new EntityMappingInfo {
										LocalEntity = localEntity,
										Expression = localEntity != null ? null : expr,
										Delegate = deleg
									};
								})
								.ToArray();

							// Query db for missing entities if needed
							if(retrievalMode == EntitiesRetrievalMode.LocalOrRemote || retrievalMode == EntitiesRetrievalMode.Remote) { 
								var missingEntities = localsAndPredicates.Where(lp => lp.LocalEntity == null && lp.Expression != null);
								if(missingEntities.Any()) {
									var filterExpression = ExpressionUtils.Or(missingEntities.Select(m => m.Expression));
									var query = Queryable_Where.MakeGenericMethod(types.To).Invoke(null, new object[] { dbSet, filterExpression });

									if(retrievalMode == EntitiesRetrievalMode.LocalOrRemote) { 
										EntityFrameworkQueryableExtensions_Load.MakeGenericMethod(types.To).Invoke(null, new object[] { query });
										foreach (var localAndPredicate in localsAndPredicates) {
											if(localAndPredicate.LocalEntity != null || localAndPredicate.Delegate == null)
												continue;

											localAndPredicate.LocalEntity = local
												.Cast<object>()
												.FirstOrDefault(e => (bool)localAndPredicate.Delegate.DynamicInvoke(e));
										}
									}
									else {
										var entities = Enumerable_ToArray.MakeGenericMethod(types.To).Invoke(null, new object[] { query }) as IEnumerable
											?? throw new InvalidOperationException("Invalid result returned");
										foreach (var localAndPredicate in localsAndPredicates.Where(lp => lp.Delegate != null)) {
											localAndPredicate.LocalEntity = entities
												.Cast<object>()
												.FirstOrDefault(e => (bool)localAndPredicate.Delegate.DynamicInvoke(e));
										}
									}
								}
							}

							// Create collection and populate it
							var destination = ObjectFactory.CreateCollection(destinationType);
							var addMethod = ObjectFactory.GetCollectionAddMethod(destination);

							foreach(var localAndPredicate in localsAndPredicates) {
								addMethod.Invoke(destination, new object[] { localAndPredicate.LocalEntity });
							}

							result = ObjectFactory.ConvertCollectionToType(destination, destinationType);
						}
						else if (source == null)
							return null;
						else
							throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
					}
					else {
						if(source == null)
							return null;

						// Check how we need to retrieve the entity
						if(retrievalMode == EntitiesRetrievalMode.Local || retrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
							var key = _model.FindEntityType(types.To).FindPrimaryKey();
							var dbSet = db.GetType().GetMethod(nameof(DbContext.Set)).Invoke(db, null)
								?? throw new InvalidOperationException("Cannot retrieve DbSet<T>");
							var local = dbSet.GetType().GetProperty(nameof(DbSet<object>.Local)).GetValue(dbSet) as IEnumerable
								?? throw new InvalidOperationException("Cannot retrieve DbSet<T>.Local");

							var keyValues = keyToValuesMap.DynamicInvoke(source) as object[]
								?? throw new InvalidOperationException($"Invalid key(s) returned for entity of type {types.To.FullName ?? types.To.Name}");

							var expr = GetEntityExpression(keyValues, key);

							result = local
								.Cast<object>()
								.FirstOrDefault(e => (bool)expr.Compile().DynamicInvoke(e));

							// Attach a new entity to the context if not found, and mark it as unchanged
							if(retrievalMode == EntitiesRetrievalMode.LocalOrAttach && result == null)
								AttachEntity(db, ref result, keyValues, key);
						}
						else if(retrievalMode == EntitiesRetrievalMode.LocalOrRemote) { 
							if (!(keyToValuesMap.DynamicInvoke(source) is object[] keyValues))
								throw new InvalidOperationException($"Invalid key(s) returned for entity of type {types.To.FullName ?? types.To.Name}");

							result = db.Find(types.To, keyValues);
						}
						else if(retrievalMode == EntitiesRetrievalMode.Remote) {
							var key = _model.FindEntityType(types.To).FindPrimaryKey();
							var dbSet = db.GetType().GetMethod(nameof(DbContext.Set)).Invoke(db, null)
								?? throw new InvalidOperationException("Cannot retrieve DbSet<T>");

							var keyValues = keyToValuesMap.DynamicInvoke(source) as object[]
								?? throw new InvalidOperationException($"Invalid key(s) returned for entity of type {types.To.FullName ?? types.To.Name}");

							var expr = GetEntityExpression(keyValues, key);

							result = Queryable_FirstOrDefault.MakeGenericMethod(types.To).Invoke(null, new object[] {
								Queryable_Where.MakeGenericMethod(types.To).Invoke(null, new object[] { dbSet, expr })
							});
						}
						else
							throw new InvalidOperationException("Unknown retrieval mode");
					}
				}

				// Should not happen
				if (result != null && !destinationType.IsAssignableFrom(result.GetType()))
					throw new InvalidOperationException($"Object of type {result.GetType().FullName ?? result.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}");

				return result;
			}
			catch (Exception e) {
				throw new MappingException(e, (sourceType, destinationType));
			}

			throw new MapNotFoundException((sourceType, destinationType));


			LambdaExpression GetEntityExpression(object[] keyValues, IKey key) {
				var entityParam = Expression.Parameter(types.To, "entity");
				Expression body = key.Properties
					.Select((p, i) => Expression.Equal(Expression.Property(entityParam, p.PropertyInfo), Expression.Constant(keyValues[i])))
					.Aggregate(Expression.AndAlso);
				return Expression.Lambda(typeof(Predicate<>).MakeGenericType(types.To), body, entityParam);
			}

			void AttachEntity(DbContext db, ref object entity, object[] keyValues, IKey key) {
				entity = ObjectFactory.Create(types.To);
				for (int i = 0; i < keyValues.Length; i++) {
					key.Properties[i].PropertyInfo?.SetValue(entity, keyValues[i]);
				}
				db.Entry(entity).State = EntityState.Unchanged;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			Map(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			destination,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source != null && !sourceType.IsAssignableFrom(source.GetType()))
				throw new ArgumentException($"Object of type {source.GetType().FullName ?? source.GetType().Name} is not assignable to type {sourceType.FullName ?? sourceType.Name}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));
			if (destination != null && !destinationType.IsAssignableFrom(destination.GetType()))
				throw new ArgumentException($"Object of type {destination.GetType().FullName ?? destination.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}", nameof(destination));

			// Check if the types can be mapped, if it throws it means we need to check that the destination collection is not readonly
			try { 
				if (!CanMapMerge(sourceType, destinationType, mappingOptions))
					throw new MapNotFoundException((sourceType, destinationType));
			}
			catch (InvalidOperationException) {
				if(destination != null) {
					var destinationInstanceType = destination.GetType();
					if (destinationInstanceType.IsArray)
						throw new MapNotFoundException((sourceType, destinationType));

					var interfaceMap = destinationInstanceType.GetInterfaceMap(destinationInstanceType.GetInterfaces()
						.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;

					// If the collection is readonly we cannot map to it
					if ((bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly))).Invoke(destination, null))
						throw new MapNotFoundException((sourceType, destinationType));
				}
			}

			if(destination != null)
				return destination;

			// DEV: handle collections

			return Map(source, sourceType, destinationType, mappingOptions);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region IMapperCanMap methods
		public bool CanMapNew(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			// Prevent being used by a collection mapper
			if (CheckCollectionMapperNestedContextRecursive(mappingOptions?.GetOptions<NestedMappingContext>()))
				return false;

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// We could also map collections of keys/entities
			if(sourceType.IsEnumerable() && sourceType != typeof(string) && destinationType.IsEnumerable() && destinationType != typeof(string)) {
				if(!ObjectFactory.CanCreateCollection(destinationType))
					return false;

				sourceType = sourceType.GetEnumerableElementType();
				destinationType = destinationType.GetEnumerableElementType();
			}

			// Check which type is the key and which is the entity, since NewMap can be used both ways
			Type keyType;
			Type entityType;
			if (sourceType.IsKeyType() || sourceType.IsCompositeKeyType()) {
				keyType = sourceType;
				entityType = destinationType;
			}
			else if(destinationType.IsKeyType() || destinationType.IsCompositeKeyType()) {
				keyType = destinationType;
				entityType = sourceType;
			}
			else
				return false;

			return CanMap(entityType, keyType);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public bool CanMapMerge(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			// Prevent being used by a collection mapper
			if (CheckCollectionMapperNestedContextRecursive(mappingOptions?.GetOptions<NestedMappingContext>()))
				return false;

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			bool cannotDetermineMap = false;
			if (sourceType.IsEnumerable() && sourceType != typeof(string) && destinationType.IsCollection() && !destinationType.IsArray) {
				if (!ObjectFactory.CanCreateCollection(destinationType))
					return false;

				// If the destination type is not an interface, check if it is not readonly
				if (!destinationType.IsInterface && destinationType.IsGenericType) {
					var collectionDefinition = destinationType.GetGenericTypeDefinition();
					if (collectionDefinition == typeof(ReadOnlyCollection<>) ||
						collectionDefinition == typeof(ReadOnlyDictionary<,>) ||
						collectionDefinition == typeof(ReadOnlyObservableCollection<>)) {

						return false;
					}
				}
				else
					cannotDetermineMap = true;

				sourceType = sourceType.GetEnumerableElementType();
				destinationType = destinationType.GetCollectionElementType();
			}

			// MergeMap can only map from key to entity so we check source and destination types
			if (!sourceType.IsKeyType() && !sourceType.IsCompositeKeyType())
				return false;

			if(CanMap(destinationType, sourceType)) {
				if (!cannotDetermineMap)
					return true;
				else
					throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
			}
			else
				return false;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private bool CanMap(Type entityType, Type keyType) {
			if (!entityType.IsClass)
				return false;

			// Check if the entity is in the model
			var modelEntity = _model.FindEntityType(entityType);
			if (modelEntity == null || modelEntity.IsOwned())
				return false;

			// Check that the entity has a key and that it matches the key type
			var key = modelEntity.FindPrimaryKey();
			if (key == null || key.Properties.Count < 1 || !key.Properties.All(p => p.PropertyInfo != null))
				return false;
			if (keyType.IsCompositeKeyType()) {
				var keyTypes = !keyType.IsNullable() ? keyType.GetGenericArguments() : keyType.GetGenericArguments()[0].GetGenericArguments();
				if (key.Properties.Count != keyTypes.Length || !keyTypes.Zip(key.Properties, (k1, k2) => (k1, k2.ClrType)).All(keys => keys.Item1 == keys.Item2))
					return false;
			}
			else if (key.Properties.Count != 1 || (key.Properties[0].ClrType != keyType && !keyType.IsNullable(key.Properties[0].ClrType)))
				return false;

			return true;
		}

		private bool CheckCollectionMapperNestedContextRecursive(NestedMappingContext context) {
			if(context == null)
				return false;
			if(context.ParentMapper is CollectionMapper)
				return true;
			return CheckCollectionMapperNestedContextRecursive(context.ParentContext);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
