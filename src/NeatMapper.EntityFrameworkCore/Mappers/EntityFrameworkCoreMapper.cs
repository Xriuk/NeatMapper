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
	/// When mapping keys to entities, may be searched locally in the <see cref="DbContext"/> first,
	/// otherwise a query to the db will be made, depending on <see cref="EntityFrameworkCoreOptions"/>
	/// (and <see cref="EntityFrameworkCoreMappingOptions"/>).
	/// </summary>
	public sealed class EntityFrameworkCoreMapper : EntityFrameworkCoreBaseMapper, IMapper, IMapperCanMap {
		private static readonly MethodInfo EntityFrameworkQueryableExtensions_Load = typeof(EntityFrameworkQueryableExtensions).GetMethod(nameof(EntityFrameworkQueryableExtensions.Load))
			?? throw new InvalidOperationException("Could not find EntityFrameworkQueryableExtensions.Load<T>()");
		private static readonly MethodInfo Enumerable_ToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))
			?? throw new InvalidOperationException("Could not find Enumerable.ToArray<T>()");
		private static readonly MethodInfo Queryable_FirstOrDefault = typeof(Queryable).GetMethods().First(m => m.Name == nameof(Queryable.FirstOrDefault) && m.GetParameters().Length == 1);


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
			mergeCollectionsOptions = null) :
				base(model, dbContextType, serviceProvider, entityFrameworkCoreOptions, elementsMatcher, mergeCollectionsOptions) {}


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
					if (types.To.IsTuple())
						valueTupleToTuple = GetOrCreateValueTupleToTupleMap(types.From, types.To);
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

					// Retrieve the db context from the services
					var db = RetrieveDbContext(mappingOptions);

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
											AttachEntity(types.To, db, ref localEntity, keyValues, key);
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
								AttachEntity(types.To, db, ref result, keyValues, key);
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

			var efCoreOptions = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>();
			var entitiesRetrievalMode = efCoreOptions?.EntitiesRetrievalMode
				?? _entityFrameworkCoreOptions.EntitiesRetrievalMode;

			// Adjust LocalOrAttach options to prevent attaching (we'll do it here)
			MappingOptions destinationMappingOptions;
			if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
				destinationMappingOptions = (mappingOptions ?? MappingOptions.Empty)
					.Replace<EntityFrameworkCoreMappingOptions>(o => new EntityFrameworkCoreMappingOptions(EntitiesRetrievalMode.Local, o.DbContextInstance, o.ThrowOnDuplicateEntity));
			}
			else
				destinationMappingOptions = mappingOptions;

			if (!CanMapMerge(sourceType, destinationType, destinationMappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			(Type From, Type To)? collectionElementTypes = destinationType.IsCollection() && !destinationType.IsArray ?
				((Type From, Type To)?)(sourceType.GetEnumerableElementType(), destinationType.GetEnumerableElementType()) :
				null;

			(Type From, Type To) types = collectionElementTypes ?? (sourceType, destinationType);

			var throwOnDuplicateEntity = efCoreOptions?.ThrowOnDuplicateEntity
				?? _entityFrameworkCoreOptions.ThrowOnDuplicateEntity;

			// Retrieve the db context from the services
			DbContext db = null;
			IKey key = null;
			if(entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) { 
				db = RetrieveDbContext(mappingOptions);
				key = _model.FindEntityType(types.To).FindPrimaryKey();
			}

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
							// Check if the collection is not readonly
							try {
								if (!CanMapMerge(sourceType, destinationType, destination as IEnumerable, destinationMappingOptions))
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

							var sourceEntitiesEnumerable = Map(source, sourceType, destinationType, destinationMappingOptions) as IEnumerable
								?? throw new InvalidOperationException("Invalid result"); // Should not happen

							MergeCollection(destinationEnumerable, sourceEnumerable, sourceEntitiesEnumerable,
								types, db, key, entitiesRetrievalMode, destinationMappingOptions, throwOnDuplicateEntity);

							return destinationEnumerable;
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
				try { 
					if (source == null || TypeUtils.IsDefaultValue(sourceType.UnwrapNullable(), source)) {
						if(destination != null && throwOnDuplicateEntity) 
							throw new DuplicateEntityException($"A non-null entity of type {types.To?.FullName ?? types.To.Name} was provided for the default key. When merging objects make sure that they match");

						return null;
					}

					// Forward the retrieval to NewMap, since we have to retrieve/create a new entity
					var result = Map(source, sourceType, destinationType, destinationMappingOptions);

					if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach && result == null) {
						if (destination != null) {
							db.Attach(destination);
							result = destination;
						}
						else 
							AttachEntity(types.To, db, ref result, GetKeyValues(types.To, types.From, source), key);
					}
					else if((result == null || destination != null) && destination != result && throwOnDuplicateEntity) {
						if (result != null)
							throw new DuplicateEntityException($"A duplicate entity of type {types.To?.FullName ?? types.To.Name} was found for the key {string.Join(", ", GetKeyValues(types.To, types.From, source))}");
						else
							throw new DuplicateEntityException($"A non-null entity of type {types.To?.FullName ?? types.To.Name} was provided for a not found entity. When merging objects make sure that they match");
					}

					return result;
				}
				catch (MappingException) {
					throw;
				}
				catch (MapNotFoundException) {
					throw;
				}
				catch (Exception e) {
					throw new MappingException(e, (sourceType, destinationType));
				}
			}


			object[] GetKeyValues(Type entityType, Type keyType, object keyEntity) {
				var keyToValuesMap = GetOrCreateKeyToValuesMap(entityType, keyType);
				if (keyType.IsTuple())
					keyEntity = GetOrCreateTupleToValueTupleMap(entityType, keyType).DynamicInvoke(keyEntity);
				return keyToValuesMap.DynamicInvoke(keyEntity) as object[]
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

		override protected bool CheckCollectionMapperNestedContextRecursive(MappingOptions mappingOptions) {
			return CheckCollectionMapperNestedContextRecursive(mappingOptions?.GetOptions<NestedMappingContext>());
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
