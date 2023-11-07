using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
		private readonly IMatcher _elementsMatcher;
		private readonly MergeCollectionsOptions _mergeCollectionOptions;
		// entity: (entity) => key
		private readonly IDictionary<Type, Delegate> _entityToKeyCache = new Dictionary<Type, Delegate>();
		private readonly IDictionary<Type, Delegate> _valueTupleToTupleCache = new Dictionary<Type, Delegate>();
		// entity: (key) => [...values]
		private readonly IDictionary<Type, Delegate> _keyToValuesCache = new Dictionary<Type, Delegate>();
		private readonly IDictionary<Type, Delegate> _tupleToValueTupleCache = new Dictionary<Type, Delegate>();

		public EntityFrameworkCoreMapper(
			IModel model,
			Type dbContextType,
			IServiceProvider serviceProvider,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			EntityFrameworkCoreOptions?
#else
			EntityFrameworkCoreOptions
#endif
			entityFrameworkCoreOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IMatcher?
#else
			IMatcher
#endif
			elementsMatcher = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MergeCollectionsOptions?
#else
			MergeCollectionsOptions
#endif
			mergeCollectionsOptions = null) {

			_model = model ?? throw new ArgumentNullException(nameof(model));
			_dbContextType = dbContextType ?? throw new ArgumentNullException(nameof(dbContextType));
			if(!typeof(DbContext).IsAssignableFrom(_dbContextType))
				throw new ArgumentException($"Type {_dbContextType.FullName ?? _dbContextType.Name} is not derived from DbContext", nameof(dbContextType));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_entityFrameworkCoreOptions = entityFrameworkCoreOptions ?? new EntityFrameworkCoreOptions();
			_elementsMatcher = elementsMatcher != null ? new SafeMatcher(elementsMatcher) : EmptyMatcher.Instance;
			_mergeCollectionOptions = mergeCollectionsOptions ?? new MergeCollectionsOptions();

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
			if (collectionElementTypes != null)
				entityToKey = collectionElementTypes.Value.To.IsKeyType() || collectionElementTypes.Value.To.IsCompositeKeyType();
			else
				entityToKey = destinationType.IsKeyType() || destinationType.IsCompositeKeyType();

			(Type From, Type To) types = collectionElementTypes ?? (sourceType, destinationType);

			try {
				// Check if we are mapping an entity to a key or vice-versa
				object result;
				if(entityToKey) {
					if(source == null && (!types.To.IsValueType || types.To.IsNullable()))
						return null;

					// Create and cache the delegate if needed
					var entityToKeyMap = GetOrCreateEntityToKeyMap(types.From, types.To);

					Delegate valueTupleToTuple;
					if (types.To.IsTuple()) { 
						lock (_valueTupleToTupleCache) {
							if (!_valueTupleToTupleCache.TryGetValue(types.From, out valueTupleToTuple)) {
								var keyParam = Expression.Parameter(TupleUtils.GetValueTupleConstructor(types.To.UnwrapNullable().GetGenericArguments()).DeclaringType
									?? throw new InvalidOperationException(), "key");
								// new Tuple<...>(key.Item1, ...)
								Expression body = Expression.New(
									TupleUtils.GetTupleConstructor(keyParam.Type.GetGenericArguments()),
									Enumerable.Range(1, keyParam.Type.GetGenericArguments().Count()).Select(n => Expression.Field(keyParam, "Item" + n)));
								// key == default(KEY) ? null : KEY
								body = Expression.Condition(
									Expression.Call(keyParam, keyParam.Type.GetMethod(nameof(ValueTuple.Equals), new [] { keyParam.Type }), Expression.Default(keyParam.Type)),
									Expression.Constant(null, body.Type),
									body);
								valueTupleToTuple = Expression.Lambda(typeof(Func<,>).MakeGenericType(keyParam.Type, body.Type), body, keyParam).Compile();
								_valueTupleToTupleCache.Add(types.From, valueTupleToTuple);
							}
						}
					}
					else
						valueTupleToTuple = null;

					// Check if we are mapping a collection or just a single entity
					if (collectionElementTypes != null) { 
						if(source is IEnumerable sourceEnumerable) { 
							var destination = ObjectFactory.CreateCollection(destinationType);
							var addMethod = ObjectFactory.GetCollectionAddMethod(destination);

							foreach (var element in sourceEnumerable) {
								var key = entityToKeyMap.DynamicInvoke(element);

								// Convert to Tuple if needed
								if (types.To.IsTuple())
									key = valueTupleToTuple.DynamicInvoke(key);
								else if(types.To.IsNullable() && TypeUtils.IsDefaultValue(types.To.GetGenericArguments()[0], key))
									key = null;

								addMethod.Invoke(destination, new object[] { key });
							}

							result = ObjectFactory.ConvertCollectionToType(destination, destinationType);
						}
						else if(source == null)
							return null;
						else
							throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
					}
					else {
						result = entityToKeyMap.DynamicInvoke(source);

						// Convert to Tuple if needed
						if (types.To.IsTuple()) 
							result = valueTupleToTuple.DynamicInvoke(result);
						else if (types.To.IsNullable() && TypeUtils.IsDefaultValue(types.To.GetGenericArguments()[0], result))
							result = null;
					}
				}
				else {
					if (source == null || TypeUtils.IsDefaultValue(sourceType.UnwrapNullable(), source))
						return null;

					var efCoreOptions = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>();

					// Retrieve the db context from the services
					DbContext db = efCoreOptions?.DbContextInstance;
					if(db != null && db.GetType() != _dbContextType)
						db = null;
					if (db == null) {
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
					}

					// Create and cache the delegate if needed
					var keyToValuesMap = GetOrCreateKeyToValuesMap(types.To, types.From);

					Delegate tupleToValueTuple;
					if (types.From.IsTuple())
						tupleToValueTuple = GetOrCreateTupleToValueTupleMap(types.To, types.From);
					else
						tupleToValueTuple = null;

					var retrievalMode = mappingOptions.GetOptions<EntityFrameworkCoreMappingOptions>()?.EntitiesRetrievalMode
						?? _entityFrameworkCoreOptions.EntitiesRetrievalMode;

					// Check if we are mapping a collection or just a single entity
					if (collectionElementTypes != null) {
						if (source is IEnumerable sourceEnumerable) {
							var key = _model.FindEntityType(types.To).FindPrimaryKey();
							var dbSet = db.GetType().GetMethods().FirstOrDefault(m => m.Name == nameof(DbContext.Set)).MakeGenericMethod(types.To).Invoke(db, null)
								?? throw new InvalidOperationException("Cannot retrieve DbSet<T>");
							var local = dbSet.GetType().GetProperty(nameof(DbSet<object>.Local)).GetValue(dbSet) as IEnumerable
								?? throw new InvalidOperationException("Cannot retrieve DbSet<T>.Local");

							// Retrieve tracked local entities and create expressions for missing
							EntityMappingInfo[] localsAndPredicates = sourceEnumerable
								.Cast<object>()
								.Select(sourceElement => {
									if (sourceElement == null || TypeUtils.IsDefaultValue(types.From.UnwrapNullable(), sourceElement))
										return new EntityMappingInfo();

									if(types.From.IsTuple())
										sourceElement = tupleToValueTuple.DynamicInvoke(sourceElement);

									var keyValues = keyToValuesMap.DynamicInvoke(sourceElement) as object[]
										?? throw new InvalidOperationException($"Invalid key(s) returned for entity of type {types.To.FullName ?? types.To.Name}");

									var expr = GetEntityExpression(keyValues, key);
									var deleg = expr.Compile();

									object localEntity;
									if(retrievalMode != EntitiesRetrievalMode.Remote) {
										localEntity = local
											.Cast<object>()
											.FirstOrDefault(e => (bool)deleg.DynamicInvoke(e));

										if(localEntity == null && retrievalMode == EntitiesRetrievalMode.LocalOrAttach)
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
						if (types.From.IsTuple())
							source = tupleToValueTuple.DynamicInvoke(source);

						var keyValues = keyToValuesMap.DynamicInvoke(source) as object[]
							?? throw new InvalidOperationException($"Invalid key(s) returned for entity of type {types.To.FullName ?? types.To.Name}");

						// Check how we need to retrieve the entity
						if (retrievalMode == EntitiesRetrievalMode.Local || retrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
							var key = _model.FindEntityType(types.To).FindPrimaryKey();
							var dbSet = db.GetType().GetMethods().FirstOrDefault(m => m.Name == nameof(DbContext.Set)).MakeGenericMethod(types.To).Invoke(db, null)
								?? throw new InvalidOperationException("Cannot retrieve DbSet<T>");
							var local = dbSet.GetType().GetProperty(nameof(DbSet<object>.Local)).GetValue(dbSet) as IEnumerable
								?? throw new InvalidOperationException("Cannot retrieve DbSet<T>.Local");

							var expr = GetEntityExpression(keyValues, key);

							result = local
								.Cast<object>()
								.FirstOrDefault(e => (bool)expr.Compile().DynamicInvoke(e));

							// Attach a new entity to the context if not found, and mark it as unchanged
							if(retrievalMode == EntitiesRetrievalMode.LocalOrAttach && result == null)
								AttachEntity(db, ref result, keyValues, key);
						}
						else if(retrievalMode == EntitiesRetrievalMode.LocalOrRemote)
							result = db.Find(types.To, keyValues);
						else if(retrievalMode == EntitiesRetrievalMode.Remote) {
							var key = _model.FindEntityType(types.To).FindPrimaryKey();
							var dbSet = db.GetType().GetMethods().FirstOrDefault(m => m.Name == nameof(DbContext.Set)).MakeGenericMethod(types.To).Invoke(db, null)
								?? throw new InvalidOperationException("Cannot retrieve DbSet<T>");

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


			LambdaExpression GetEntityExpression(object[] keyValues, IKey key) {
				var entityParam = Expression.Parameter(types.To, "entity");
				Expression body = key.Properties
					.Select((p, i) => Expression.Equal(Expression.Property(entityParam, p.PropertyInfo), Expression.Constant(keyValues[i])))
					.Aggregate(Expression.AndAlso);
				return Expression.Lambda(typeof(Func<,>).MakeGenericType(types.To, typeof(bool)), body, entityParam);
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

			if (!CanMapMerge(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			(Type From, Type To)? collectionElementTypes = destinationType.IsCollection() && !destinationType.IsArray ?
				((Type From, Type To)?)(sourceType.GetEnumerableElementType(), destinationType.GetEnumerableElementType()) :
				null;

			(Type From, Type To) types = collectionElementTypes ?? (sourceType, destinationType);

			var throwOnDuplicateEntity = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>()?.ThrowOnDuplicateEntity
				?? _entityFrameworkCoreOptions.ThrowOnDuplicateEntity;

			// Check if we are mapping a collection or just a single entity
			if (collectionElementTypes != null) {
				// If the destination type is not an interface, check if it is not readonly
				if (!destinationType.IsInterface && destinationType.IsGenericType) {
					var collectionDefinition = destinationType.GetGenericTypeDefinition();
					if (collectionDefinition == typeof(ReadOnlyCollection<>) ||
						collectionDefinition == typeof(ReadOnlyDictionary<,>) ||
						collectionDefinition == typeof(ReadOnlyObservableCollection<>)) {

						throw new MapNotFoundException(types);
					}
				}

				try {
					if (source is IEnumerable sourceEnumerable) {
						// If we have to create the destination collection we know that we forward to NewMap
						// Otherwise we must check that the collection can be mapped to
						if (destination == null) 
							return Map(source, sourceType, destinationType, mappingOptions);
						else {
							// Check if the collection is not readonly recursively
							try {
								if (!CanMapMerge(sourceType, destinationType, destination as IEnumerable, mappingOptions))
									throw new MapNotFoundException((sourceType, destinationType));
							}
							catch (MapNotFoundException) {
								throw;
							}
							catch { }
						}

						if (destination is IEnumerable destinationEnumerable) {
							var destinationInstanceType = destination.GetType();
							if (destinationInstanceType.IsArray)
								throw new MapNotFoundException((sourceType, destinationType));

							var interfaceMap = destinationInstanceType.GetInterfaceMap(destinationInstanceType.GetInterfaces()
								.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;

							// If the collection is readonly we cannot map to it
							if ((bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly))).Invoke(destination, null))
								throw new MapNotFoundException(types);

							var sourceEntitiesEnumerable = Map(source, sourceType, destinationType, mappingOptions) as IEnumerable
								?? throw new InvalidOperationException("Invalid result"); // Should not happen

							var addMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Add)));
							var removeMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Remove)));

							var elementsToRemove = new List<object>();
							var elementsToAdd = new List<object>();

							var mergeMappingOptions = mappingOptions?.GetOptions<MergeCollectionsMappingOptions>();

							// Create the matcher
							IMatcher elementMatcher;
							if (mergeMappingOptions?.Matcher != null)
								elementMatcher = new SafeMatcher(new DelegateMatcher(mergeMappingOptions.Matcher, _elementsMatcher, _serviceProvider));
							else
								elementMatcher = _elementsMatcher;

							// Deleted elements
							if (mergeMappingOptions?.RemoveNotMatchedDestinationElements
								?? _mergeCollectionOptions.RemoveNotMatchedDestinationElements) {
								foreach (var destinationElement in destinationEnumerable) {
									bool found = false;
									foreach (var sourceElement in sourceEntitiesEnumerable) {
										if (elementMatcher.Match(sourceElement, types.To, destinationElement, types.To, mappingOptions)) {
											found = true;
											break;
										}
									}

									if (!found)
										elementsToRemove.Add(destinationElement);
								}
							}

							// Added/updated elements
							foreach (var sourceEntityElement in sourceEntitiesEnumerable) {
								bool found = false;
								object matchingDestinationElement = null;
								foreach (var destinationElement in destinationEnumerable) {
									if (elementMatcher.Match(sourceEntityElement, types.To, destinationElement, types.To, mappingOptions) &&
										!elementsToRemove.Contains(destinationElement)) {

										matchingDestinationElement = destinationElement;
										found = true;
										break;
									}
								}

								if (found) {
									if (matchingDestinationElement != sourceEntityElement) {
										if (throwOnDuplicateEntity) {
											if (sourceEntityElement != null) 
												throw new DuplicateEntityException($"A duplicate entity of type {types.To?.FullName ?? types.To.Name} was found for the key {string.Join(", ", GetKeyValues(types.To, types.From, GetOrCreateEntityToKeyMap(types.To, types.From).DynamicInvoke(sourceEntityElement)))}");
											else
												throw new DuplicateEntityException($"A non-null entity of type {types.To?.FullName ?? types.To.Name} was provided for a not found entity. When merging objects make sure that they match");
										}
										else{
											elementsToRemove.Add(matchingDestinationElement);
											elementsToAdd.Add(sourceEntityElement);
										}
									}
								}
								else 
									elementsToAdd.Add(sourceEntityElement);
							}

							foreach (var element in elementsToRemove) {
								if (!(bool)removeMethod.Invoke(destination, new object[] { element }))
									throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
							}
							foreach (var element in elementsToAdd) {
								addMethod.Invoke(destination, new object[] { element });
							}

							return destination;
						}
						else
							throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
					}
					else if (source == null)
						return null;
					else
						throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
				}
				catch (MapNotFoundException) {
					throw;
				}
				catch (Exception e) {
					throw new MappingException(e, (sourceType, destinationType));
				}
			}
			else { 
				if (source == null || TypeUtils.IsDefaultValue(sourceType.UnwrapNullable(), source)) {
					if(destination != null && throwOnDuplicateEntity) 
						throw new DuplicateEntityException($"A non-null entity of type {types.To?.FullName ?? types.To.Name} was provided for the default key. When merging objects make sure that they match");

					return null;
				}

				// Forward the retrieval to NewMap, since we have to retrieve/create a new entity
				var result = Map(source, sourceType, destinationType, mappingOptions);

				if((result == null || destination != null) && destination != result && throwOnDuplicateEntity) {
					if (result != null)
						throw new DuplicateEntityException($"A duplicate entity of type {types.To?.FullName ?? types.To.Name} was found for the key {string.Join(", ", GetKeyValues(types.To, types.From, source))}");
					else
						throw new DuplicateEntityException($"A non-null entity of type {types.To?.FullName ?? types.To.Name} was provided for a not found entity. When merging objects make sure that they match");
				}

				return result;
			}


			object[] GetKeyValues(Type entityType, Type keyType, object key) {
				var keyToValuesMap = GetOrCreateKeyToValuesMap(entityType, keyType);
				if (keyType.IsTuple())
					key = GetOrCreateTupleToValueTupleMap(entityType, keyType).DynamicInvoke(key);
				return keyToValuesMap.DynamicInvoke(key) as object[]
					?? throw new InvalidOperationException($"Invalid key(s) returned for entity of type {entityType.FullName ?? entityType.Name}");
			}

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

			return CanMapMerge(sourceType, destinationType, null, mappingOptions);
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		bool CanMapMerge(
			Type sourceType,
			Type destinationType,
			IEnumerable destination = null,
			MappingOptions mappingOptions = null) {

			// Prevent being used by a collection mapper
			if (CheckCollectionMapperNestedContextRecursive(mappingOptions?.GetOptions<NestedMappingContext>()))
				return false;

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) elementTypes;
			if (sourceType.IsEnumerable() && sourceType != typeof(string) && destinationType.IsCollection() && !destinationType.IsArray) {
				if (!ObjectFactory.CanCreateCollection(destinationType))
					return false;

				// If the destination type is not an interface, check if it is not readonly
				// If the destination type is not an interface, check if it is not readonly
				// Otherwise check the destination if provided
				if (!destinationType.IsInterface && destinationType.IsGenericType) {
					var collectionDefinition = destinationType.GetGenericTypeDefinition();
					if (collectionDefinition == typeof(ReadOnlyCollection<>) ||
						collectionDefinition == typeof(ReadOnlyDictionary<,>) ||
						collectionDefinition == typeof(ReadOnlyObservableCollection<>)) {

						return false;
					}
				}
				else if (destination != null) {
					var destinationInstanceType = destination.GetType();
					if (destinationInstanceType.IsArray)
						return false;

					var interfaceMap = destinationInstanceType.GetInterfaceMap(destinationInstanceType.GetInterfaces()
						.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;

					// If the collection is readonly we cannot map to it
					if ((bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly))).Invoke(destination, null))
						return false;
				}

				elementTypes = (sourceType.GetEnumerableElementType(), destinationType.GetCollectionElementType());
			}
			else
				elementTypes = (sourceType, destinationType);

			// MergeMap can only map from key to entity so we check source and destination types
			if (!elementTypes.From.IsKeyType() && !elementTypes.From.IsCompositeKeyType())
				return false;

			if (CanMap(elementTypes.To, elementTypes.From)) {
				if(sourceType.IsEnumerable() && sourceType != typeof(string) && destinationType.IsCollection() && !destinationType.IsArray) { 
					if (!destinationType.IsInterface || destination != null)
						return true;

					throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
				}
				else
					return true;
			}
			else
				return false;
		}

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
				var keyTypes = keyType.UnwrapNullable().GetGenericArguments();
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

		private Delegate GetOrCreateEntityToKeyMap(Type entityType, Type keyType) {
			lock (_entityToKeyCache) {
				if (!_entityToKeyCache.TryGetValue(entityType, out var entityToKeyMap)) {
					var entityParam = Expression.Parameter(entityType, "entity");
					var modelEntity = _model.FindEntityType(entityType);
					var key = modelEntity.FindPrimaryKey();
					Expression body;
					if (key.Properties.Count == 1) {
						// entity.Id
						body = Expression.Property(entityParam, key.Properties[0].PropertyInfo);
					}
					else {
						// new ValueTuple<...>(entity.Key1, ...)
						body = Expression.New(
							TupleUtils.GetValueTupleConstructor(keyType.UnwrapNullable().GetGenericArguments()),
							key.Properties.Select(p => Expression.Property(entityParam, p.PropertyInfo)));
					}

					// entity != null ? KEY : default(KEY)
					body = Expression.Condition(
						Expression.NotEqual(entityParam, Expression.Constant(null, entityParam.Type)),
						body,
						Expression.Default(body.Type));
					entityToKeyMap = Expression.Lambda(typeof(Func<,>).MakeGenericType(entityParam.Type, body.Type), body, entityParam).Compile();
					_entityToKeyCache.Add(entityType, entityToKeyMap);
				}

				return entityToKeyMap;
			}
		}

		private Delegate GetOrCreateKeyToValuesMap(Type entityType, Type keyType) {
			lock (_keyToValuesCache) {
				if (!_keyToValuesCache.TryGetValue(entityType, out var keyToValuesMap)) {
					var keyParam = Expression.Parameter(keyType.IsCompositeKeyType() ? TupleUtils.GetValueTupleConstructor(keyType.UnwrapNullable().GetGenericArguments()).DeclaringType : keyType.UnwrapNullable(), "key");
					var key = _model.FindEntityType(entityType).FindPrimaryKey();
					Expression body;
					if (key.Properties.Count == 1) {
						// new object[]{ (object)key }
						body = Expression.NewArrayInit(typeof(object), Expression.Convert(keyParam, typeof(object)));
					}
					else {
						// new object[]{ (object)key.Item1, ... }
						body = Expression.NewArrayInit(typeof(object),
							Enumerable.Range(1, key.Properties.Count)
								.Select(n => Expression.Convert(Expression.PropertyOrField(keyParam, "Item" + n), typeof(object))));
					}

					keyToValuesMap = Expression.Lambda(typeof(Func<,>).MakeGenericType(keyParam.Type, body.Type), body, keyParam).Compile();
					_keyToValuesCache.Add(entityType, keyToValuesMap);
				}

				return keyToValuesMap;
			}
		}

		private Delegate GetOrCreateTupleToValueTupleMap(Type entityType, Type tupleType) {
			if(!tupleType.IsTuple())
				throw new ArgumentException("Type is not a Tuple", nameof(tupleType));

			lock (_tupleToValueTupleCache) {
				if (!_tupleToValueTupleCache.TryGetValue(entityType, out var tupleToValueTuple)) {
					var keyParam = Expression.Parameter(tupleType, "key");
					// new ValueTuple<...>(key.Item1, ...)
					Expression body = Expression.New(
						TupleUtils.GetValueTupleConstructor(keyParam.Type.GetGenericArguments()),
						Enumerable.Range(1, keyParam.Type.GetGenericArguments().Count()).Select(n => Expression.Property(keyParam, "Item" + n)));
					tupleToValueTuple = Expression.Lambda(typeof(Func<,>).MakeGenericType(keyParam.Type, body.Type), body, keyParam).Compile();
					_tupleToValueTupleCache.Add(entityType, tupleToValueTuple);
				}

				return tupleToValueTuple;
			}
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
