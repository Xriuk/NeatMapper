using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which retrieves asynchronously entities from their keys,
	/// even composite keys as <see cref="Tuple"/> or <see cref="ValueTuple"/>.<br/>
	/// Supports new and merge maps, also supports collections (not nested).<br/>
	/// Entities may be searched locally in the <see cref="DbContext"/> first,
	/// otherwise a query to the db will be made, depending on <see cref="EntityFrameworkCoreOptions"/>
	/// (and <see cref="EntityFrameworkCoreMappingOptions"/>).
	/// </summary>
	public sealed class AsyncEntityFrameworkCoreMapper : EntityFrameworkCoreBaseMapper, IAsyncMapper, IAsyncMapperCanMap, IAsyncMapperFactory {
		/// <summary>
		/// <see cref="EntityFrameworkQueryableExtensions.LoadAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		/// </summary>
		private static readonly MethodInfo EntityFrameworkQueryableExtensions_LoadAsync = typeof(EntityFrameworkQueryableExtensions)
			.GetMethod(nameof(EntityFrameworkQueryableExtensions.LoadAsync))
				?? throw new InvalidOperationException("Could not find EntityFrameworkQueryableExtensions.LoadAsync<T>()");
		/// <summary>
		/// <see cref="EntityFrameworkQueryableExtensions.ToArrayAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		/// </summary>
		private static readonly MethodInfo EntityFrameworkQueryableExtensions_ToArrayAsync = typeof(EntityFrameworkQueryableExtensions)
			.GetMethod(nameof(EntityFrameworkQueryableExtensions.ToArrayAsync))
				?? throw new InvalidOperationException("Could not find EntityFrameworkQueryableExtensions.ToArrayAsync<T>()");
		/// <summary>
		/// <see cref="EntityFrameworkQueryableExtensions.FirstOrDefaultAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		/// </summary>
		private static readonly MethodInfo EntityFrameworkQueryableExtensions_FirstOrDefaultAsync =
			typeof(EntityFrameworkQueryableExtensions).GetMethods()
			.First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync) && m.GetParameters().Length == 2);


		/// <summary>
		/// Creates a new instance of <see cref="AsyncEntityFrameworkCoreMapper"/>.
		/// </summary>
		/// <param name="model">Model to use to retrieve keys of entities.</param>
		/// <param name="dbContextType">
		/// Type of the database context to use, must derive from <see cref="DbContext"/>.
		/// </param>
		/// <param name="serviceProvider">
		/// Service provider used to retrieve instances of <paramref name="dbContextType"/> context.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions"/>.
		/// </param>
		/// <param name="entityFrameworkCoreOptions">
		/// Additional options which allow to specify how entities should be retrieved and how to merge them.<br/>
		/// Can be overridden during mapping with <see cref="EntityFrameworkCoreMappingOptions"/>.
		/// </param>
		/// <param name="elementsMatcher">
		/// <see cref="IMatcher"/> used to match elements between collections to merge them,
		/// if null the elements won't be matched.<br/>
		/// Can be overridden during mapping with <see cref="MergeCollectionsMappingOptions"/>.
		/// </param>
		/// <param name="mergeCollectionsOptions">
		/// Additional merging options to apply during mapping, null to use default.<br/>
		/// Can be overridden during mapping with <see cref="MergeCollectionsMappingOptions"/>.
		/// </param>
		public AsyncEntityFrameworkCoreMapper(
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


		#region IAsyncMapper methods
		public Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> MapAsync(
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
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return MapAsyncNewFactory(sourceType, destinationType, mappingOptions, cancellationToken).Invoke(source);
		}

		public Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> MapAsync(
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
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return MapAsyncMergeFactory(sourceType, destinationType, mappingOptions, cancellationToken).Invoke(source, destination);
		}
		#endregion

		#region IAsyncMapperCanMap methods
		public Task<bool> CanMapAsyncNew(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			// Prevent being used by a collection mapper
			if (CheckAsyncCollectionMapperNestedContextRecursive(mappingOptions?.GetOptions<AsyncNestedMappingContext>()))
				return Task.FromResult(false);

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// We could also map collections of keys/entities
			if (sourceType.IsEnumerable() && sourceType != typeof(string) && destinationType.IsEnumerable() && destinationType != typeof(string)) {
				if (!ObjectFactory.CanCreateCollection(destinationType))
					return Task.FromResult(false);

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
			else if (destinationType.IsKeyType() || destinationType.IsCompositeKeyType()) {
				keyType = destinationType;
				entityType = sourceType;
			}
			else
				return Task.FromResult(false);

			return Task.FromResult(CanMap(entityType, keyType));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public Task<bool> CanMapAsyncMerge(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return Task.FromResult(CanMapMerge(sourceType, destinationType, null, mappingOptions));
		}
		#endregion

		#region IAsyncMapperFactory methods
		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
		object?, Task<object?>
#else
		object, Task<object>
#endif
			> MapAsyncNewFactory(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (!CanMapAsyncNew(sourceType, destinationType, mappingOptions, cancellationToken).Result)
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

				return async source => {
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
										await (Task)EntityFrameworkQueryableExtensions_LoadAsync.MakeGenericMethod(types.To).Invoke(null, new object[] { query, cancellationToken });
										foreach (var localAndPredicate in localsAndPredicates) {
											if (localAndPredicate.LocalEntity != null || localAndPredicate.Delegate == null)
												continue;

											localAndPredicate.LocalEntity = local
												.Cast<object>()
												.FirstOrDefault(e => (bool)localAndPredicate.Delegate.DynamicInvoke(e));
										}
									}
									else {
										var entities = await TaskUtils.AwaitTask<IEnumerable>((Task)EntityFrameworkQueryableExtensions_ToArrayAsync.MakeGenericMethod(types.To).Invoke(null, new object[] { query, cancellationToken }));
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

				return async source => {
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
							result = await db.FindAsync(types.To, keyValues, cancellationToken);
							break;
						case EntitiesRetrievalMode.Remote: {
							var expr = GetEntityPredicate(types.To, keyValues, key);

							result = await TaskUtils.AwaitTask<object>((Task)EntityFrameworkQueryableExtensions_FirstOrDefaultAsync.MakeGenericMethod(types.To).Invoke(null, new object[] {
								Queryable_Where.MakeGenericMethod(types.To).Invoke(null, new object[] { dbSet, expr }),
								cancellationToken
							}));
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
		object?, object?, Task<object?>
#else
		object, object, Task<object>
#endif
			> MapAsyncMergeFactory(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (!CanMapAsyncMerge(sourceType, destinationType, mappingOptions, cancellationToken).Result)
				throw new MapNotFoundException((sourceType, destinationType));

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
			var destinationFactory = MapAsyncNewFactory(sourceType, destinationType, destinationMappingOptions, cancellationToken);

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

				var factory = MapAsyncNewFactory(sourceType, destinationType, mappingOptions, cancellationToken);

				return async (source, destination) => {
					try {
						NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));
						NeatMapper.TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

						if (source is IEnumerable sourceEnumerable) {
							// If we have to create the destination collection we know that we forward to NewMap
							// Otherwise we must check that the collection can be mapped to
							if (destination == null)
								return await factory.Invoke(source);
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

								var sourceEntitiesEnumerable = (await destinationFactory.Invoke(source)) as IEnumerable
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
				return async (source, destination) => {
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
						result = await destinationFactory.Invoke(source);

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
			return CheckAsyncCollectionMapperNestedContextRecursive(mappingOptions?.GetOptions<AsyncNestedMappingContext>());
		}
		private bool CheckAsyncCollectionMapperNestedContextRecursive(AsyncNestedMappingContext context) {
			if (context == null)
				return false;
			if (context.ParentMapper is AsyncCollectionMapper)
				return true;
			return CheckAsyncCollectionMapperNestedContextRecursive(context.ParentContext);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
