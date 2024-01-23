using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// <see cref="IMapper"/> which retrieves entities from their keys, even composite keys
	/// as <see cref="Tuple"/> or <see cref="ValueTuple"/>.<br/>
	/// Supports new and merge maps, also supports collections (not nested).<br/>
	/// Entities may be searched locally in the <see cref="DbContext"/> first,
	/// otherwise a query to the db will be made, depending on <see cref="EntityFrameworkCoreOptions"/>
	/// (and <see cref="EntityFrameworkCoreMappingOptions"/>).
	/// </summary>
	public sealed class EntityFrameworkCoreMapper : EntityFrameworkCoreBaseMapper, IMapper, IMapperCanMap, IMapperFactory {
		/// <summary>
		/// <see cref="EntityFrameworkQueryableExtensions.Load{TSource}(IQueryable{TSource})"/>
		/// </summary>
		private static readonly MethodInfo EntityFrameworkQueryableExtensions_Load = typeof(EntityFrameworkQueryableExtensions)
			.GetMethod(nameof(EntityFrameworkQueryableExtensions.Load))
				?? throw new InvalidOperationException("Could not find EntityFrameworkQueryableExtensions.Load<T>()");
		/// <summary>
		/// <see cref="Enumerable.ToArray{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>
		/// </summary>
		private static readonly MethodInfo Enumerable_ToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))
			?? throw new InvalidOperationException("Could not find Enumerable.ToArray<T>()");
		/// <summary>
		/// <see cref="Queryable.FirstOrDefault{TSource}(IQueryable{TSource})"/>
		/// </summary>
		private static readonly MethodInfo Queryable_FirstOrDefault = typeof(Queryable).GetMethods()
			.First(m => m.Name == nameof(Queryable.FirstOrDefault) && m.GetParameters().Length == 1);


		/// <summary>
		/// Creates a new instance of <see cref="EntityFrameworkCoreMapper"/>.
		/// </summary>
		/// <param name="model">Model to use to retrieve keys of entities.</param>
		/// <param name="dbContextType">
		/// Type of the database context to use, must derive from <see cref="DbContext"/>.
		/// </param>
		/// <param name="serviceProvider">
		/// Service provider used to retrieve instances of <paramref name="dbContextType"/> context.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions.ServiceProvider"/>.
		/// </param>
		/// <param name="entityFrameworkCoreOptions">
		/// Additional options which allow to specify how entities should be retrieved and how to merge them.<br/>
		/// Can be overridden during mapping with <see cref="EntityFrameworkCoreMappingOptions"/>.
		/// </param>
		/// <param name="elementsMatcher">
		/// <see cref="IMatcher"/> used to match elements between collections to merge them,
		/// if null the elements won't be matched.<br/>
		/// Can be overridden during mapping with <see cref="MergeCollectionsMappingOptions.Matcher"/>.
		/// </param>
		/// <param name="mergeCollectionsOptions">
		/// Additional merging options to apply during mapping, null to use default.<br/>
		/// Can be overridden during mapping with <see cref="MergeCollectionsMappingOptions"/>.
		/// </param>
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

			return MapNewFactory(sourceType, destinationType, mappingOptions).Invoke(source);
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

			return MapMergeFactory(sourceType, destinationType, mappingOptions).Invoke(source, destination);
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
			if (CheckCollectionMapperNestedContextRecursive(mappingOptions))
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

			// We can only map from key to entity so we check source and destination types
			if (!sourceType.IsKeyType() && !sourceType.IsCompositeKeyType())
				return false;

			return CanMap(destinationType, sourceType);

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

		#region IMapperFactory methods
		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
		object?, object?
#else
		object, object
#endif
			> MapNewFactory(
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
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (!CanMapNew(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			(Type From, Type To)? collectionElementTypes = destinationType.IsEnumerable() && destinationType != typeof(string) ?
				((Type From, Type To)?)(sourceType.GetEnumerableElementType(), destinationType.GetEnumerableElementType()) :
				null;

			(Type From, Type To) types = collectionElementTypes ?? (sourceType, destinationType);

			// Retrieve the db context from the services
			var db = RetrieveDbContext(mappingOptions);

			var retrievalMode = mappingOptions.GetOptions<EntityFrameworkCoreMappingOptions>()?.EntitiesRetrievalMode
				?? _entityFrameworkCoreOptions.EntitiesRetrievalMode;

			var key = _model.FindEntityType(types.To).FindPrimaryKey();
			var dbSet = db.GetType().GetMethods().FirstOrDefault(m => m.Name == nameof(DbContext.Set)).MakeGenericMethod(types.To).Invoke(db, null)
				?? throw new InvalidOperationException("Cannot retrieve DbSet<T>");
			var local = dbSet.GetType().GetProperty(nameof(DbSet<object>.Local)).GetValue(dbSet) as IEnumerable
				?? throw new InvalidOperationException("Cannot retrieve DbSet<T>.Local");

			// Check if we are mapping a collection or just a single entity
			if (collectionElementTypes != null) {
				Func<object> collectionFactory;
				Type actualCollectionType;
				try {
					collectionFactory = ObjectFactory.CreateCollectionFactory(destinationType, out actualCollectionType);
				}
				catch (ObjectCreationException) {
					throw new MapNotFoundException(types);
				}
				var addMethod = ObjectFactory.GetCollectionAddMethod(actualCollectionType);
				var collectionConversion = ObjectFactory.CreateCollectionConversionFactory(destinationType);

				return source => {
					NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));

					if (source == null || TypeUtils.IsDefaultValue(sourceType.UnwrapNullable(), source))
						return null;

					if (source is IEnumerable sourceEnumerable) {
						object result;
						try {
							// Retrieve tracked local entities (or attach them if needed) and create expressions for missing
							var localsAndPredicates = RetrieveLocalsAndPredicates(sourceEnumerable, types.From, types.To, key, retrievalMode, local, db);

							// Query db for missing entities if needed
							if (retrievalMode == EntitiesRetrievalMode.LocalOrRemote || retrievalMode == EntitiesRetrievalMode.Remote) {
								var missingEntities = localsAndPredicates.Where(lp => lp.LocalEntity == null && lp.Expression != null);
								if (missingEntities.Any()) {
									var filterExpression = ExpressionUtils.Or(missingEntities.Select(m => m.Expression));
									var query = Queryable_Where.MakeGenericMethod(types.To).Invoke(null, new object[] { dbSet, filterExpression });

									if (retrievalMode == EntitiesRetrievalMode.LocalOrRemote) {
										EntityFrameworkQueryableExtensions_Load.MakeGenericMethod(types.To).Invoke(null, new object[] { query });
										foreach (var localAndPredicate in localsAndPredicates) {
											if (localAndPredicate.LocalEntity != null || localAndPredicate.Delegate == null)
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
							var destination = collectionFactory.Invoke();

							foreach (var localAndPredicate in localsAndPredicates) {
								addMethod.Invoke(destination, new object[] { localAndPredicate.LocalEntity });
							}

							result = ObjectFactory.ConvertCollectionToType(destination, destinationType);
						}
						catch (TaskCanceledException) {
							throw;
						}
						catch (Exception e) {
							throw new MappingException(e, (sourceType, destinationType));
						}

						// Should not happen
						NeatMapper.TypeUtils.CheckObjectType(result, destinationType);

						return result;
					}
					else if (source == null)
						return null;
					else
						throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
				};
			}
			else {
				var tupleToValueTupleMap = types.From.IsTuple() ? GetOrCreateTupleToValueTupleMap(types.To, types.From) : null;
				var keyValuesMap = GetOrCreateKeyToValuesMap(types.To, types.From);

				return source => {
					NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));

					if (source == null || TypeUtils.IsDefaultValue(sourceType.UnwrapNullable(), source))
						return null;

					object result;
					try {
						if (types.From.IsTuple())
							source = tupleToValueTupleMap.DynamicInvoke(source);

						var keyValues = keyValuesMap.DynamicInvoke(source) as object[]
							?? throw new InvalidOperationException($"Invalid key(s) returned for entity of type {types.To.FullName ?? types.To.Name}");

						// Check how we need to retrieve the entity
						switch (retrievalMode) {
						case EntitiesRetrievalMode.Local:
						case EntitiesRetrievalMode.LocalOrAttach: { 
							var expr = GetEntityPredicate(types.To, keyValues, key);

							result = local
								.Cast<object>()
								.FirstOrDefault(e => (bool)expr.Compile().DynamicInvoke(e));

							// Attach a new entity to the context if not found, and mark it as unchanged
							if (retrievalMode == EntitiesRetrievalMode.LocalOrAttach && result == null)
								AttachEntity(types.To, db, ref result, keyValues, key);
							break;
						}
						case EntitiesRetrievalMode.LocalOrRemote:
							result = db.Find(types.To, keyValues);
							break;
						case EntitiesRetrievalMode.Remote: { 
							var expr = GetEntityPredicate(types.To, keyValues, key);

							result = Queryable_FirstOrDefault.MakeGenericMethod(types.To).Invoke(null, new object[] {
								Queryable_Where.MakeGenericMethod(types.To).Invoke(null, new object[] { dbSet, expr })
							});
							break;
						}
						default:
							throw new InvalidOperationException("Unknown retrieval mode");
						}
					}
					catch (TaskCanceledException) {
						throw;
					}
					catch (Exception e) {
						throw new MappingException(e, (sourceType, destinationType));
					}

					// Should not happen
					NeatMapper.TypeUtils.CheckObjectType(result, destinationType);

					return result;
				};
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
		object?, object?, object?
#else
		object, object, object
#endif
			> MapMergeFactory(
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
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (!CanMapMerge(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			var efCoreOptions = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>();
			var entitiesRetrievalMode = efCoreOptions?.EntitiesRetrievalMode
				?? _entityFrameworkCoreOptions.EntitiesRetrievalMode;

			// Adjust LocalOrAttach options to prevent attaching (we'll do it here)
			MappingOptions destinationMappingOptions;
			if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
				destinationMappingOptions = (mappingOptions ?? MappingOptions.Empty)
					.ReplaceOrAdd<EntityFrameworkCoreMappingOptions>(o => new EntityFrameworkCoreMappingOptions(EntitiesRetrievalMode.Local, o?.DbContextInstance, o?.ThrowOnDuplicateEntity));
			}
			else
				destinationMappingOptions = mappingOptions;
			var destinationFactory = MapNewFactory(sourceType, destinationType, destinationMappingOptions);

			(Type From, Type To)? collectionElementTypes = destinationType.IsCollection() && !destinationType.IsArray ?
				((Type From, Type To)?)(sourceType.GetEnumerableElementType(), destinationType.GetEnumerableElementType()) :
				null;

			(Type From, Type To) types = collectionElementTypes ?? (sourceType, destinationType);

			var throwOnDuplicateEntity = efCoreOptions?.ThrowOnDuplicateEntity
				?? _entityFrameworkCoreOptions.ThrowOnDuplicateEntity;

			// Retrieve the db context from the services
			DbContext db = null;
			IKey key = null;
			if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
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

				var factory = MapNewFactory(sourceType, destinationType, mappingOptions);

				return (source, destination) => {
					try {
						NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));
						NeatMapper.TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

						if (source is IEnumerable sourceEnumerable) {
							// If we have to create the destination collection we know that we forward to NewMap
							// Otherwise we must check that the collection can be mapped to
							if (destination == null)
								return factory.Invoke(source);
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

								var sourceEntitiesEnumerable = destinationFactory.Invoke(source) as IEnumerable
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
					catch (MappingException) {
						throw;
					}
					catch (TaskCanceledException) {
						throw;
					}
					catch (MapNotFoundException) {
						throw;
					}
					catch (Exception e) {
						throw new MappingException(e, (sourceType, destinationType));
					}
				};
			}
			else {
				return (source, destination) => {
					NeatMapper.TypeUtils.CheckObjectType(source, types.From, nameof(source));
					NeatMapper.TypeUtils.CheckObjectType(destination, types.To, nameof(destination));

					object result;
					try {
						if (source == null || TypeUtils.IsDefaultValue(types.From.UnwrapNullable(), source)) {
							if (destination != null && throwOnDuplicateEntity)
								throw new DuplicateEntityException($"A non-null entity of type {types.To?.FullName ?? types.To.Name} was provided for the default key. When merging objects make sure that they match");

							return null;
						}

						// Forward the retrieval to NewMap, since we have to retrieve/create a new entity
						result = destinationFactory.Invoke(source);

						if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach && result == null) {
							if (destination != null) {
								db.Attach(destination);
								result = destination;
							}
							else
								AttachEntity(types.To, db, ref result, GetKeyValues(types.To, types.From, source), key);
						}
						else if ((result == null || destination != null) && destination != result && throwOnDuplicateEntity) {
							if (result != null)
								throw new DuplicateEntityException($"A duplicate entity of type {types.To?.FullName ?? types.To.Name} was found for the key {string.Join(", ", GetKeyValues(types.To, types.From, source))}");
							else
								throw new DuplicateEntityException($"A non-null entity of type {types.To?.FullName ?? types.To.Name} was provided for a not found entity. When merging objects make sure that they match");
						}
					}
					catch (MappingException) {
						throw;
					}
					catch (TaskCanceledException) {
						throw;
					}
					catch (MapNotFoundException) {
						throw;
					}
					catch (Exception e) {
						throw new MappingException(e, (sourceType, destinationType));
					}

					// Should not happen
					NeatMapper.TypeUtils.CheckObjectType(result, destinationType);

					return result;
				};
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
