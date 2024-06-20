using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// <see cref="IMapper"/> which retrieves entities from their keys (even composite keys
	/// as <see cref="Tuple"/> or <see cref="ValueTuple"/>, and shadow keys) from a <see cref="DbContext"/>.<br/>
	/// Supports new and merge maps, also supports collections (not nested).<br/>
	/// Entities may be searched locally in the <see cref="DbContext"/> first,
	/// otherwise a query to the db will be made, depending on
	/// <see cref="EntityFrameworkCoreOptions.EntitiesRetrievalMode"/>
	/// (and <see cref="EntityFrameworkCoreMappingOptions.EntitiesRetrievalMode"/>).
	/// </summary>
	/// <remarks>
	/// Since a single <see cref="DbContext"/> instance cannot be used concurrently and it is not thread-safe
	/// on its own, every access to the provided <see cref="DbContext"/> instance and all its members
	/// (local and remote) for each map is protected by a semaphore.<br/>
	/// This makes this class thread-safe and concurrently usable, though not necessarily efficient to do so.<br/>
	/// Any external concurrent use of the <see cref="DbContext"/> instance is not monitored and could throw exceptions,
	/// so you should not be accessing the context externally while mapping.
	/// </remarks>
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
				base(model, dbContextType, serviceProvider,
					entityFrameworkCoreOptions != null ? new EntityFrameworkCoreOptions(entityFrameworkCoreOptions) : null,
					elementsMatcher,
					mergeCollectionsOptions != null ? new MergeCollectionsOptions(mergeCollectionsOptions) : null) {}


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

			using(var factory = MapNewFactory(sourceType, destinationType, mappingOptions)) { 
				return factory.Invoke(source);
			}
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

			using (var factory = MapMergeFactory(sourceType, destinationType, mappingOptions)) {
				return factory.Invoke(source, destination);
			}
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
		public INewMapFactory MapNewFactory(
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
			var dbContext = RetrieveDbContext(mappingOptions);

			var dbContextSemaphore = EfCoreUtils.GetOrCreateSemaphoreForDbContext(dbContext);

			var retrievalMode = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>()?.EntitiesRetrievalMode
				?? _entityFrameworkCoreOptions.EntitiesRetrievalMode;

			var key = _model.FindEntityType(types.To).FindPrimaryKey();
			object dbSet;
			IEnumerable localView;
			dbContextSemaphore.Wait();
			try { 
				dbSet = dbContext.GetType().GetMethods().FirstOrDefault(m => m.IsGenericMethod && m.Name == nameof(DbContext.Set)).MakeGenericMethod(types.To).Invoke(dbContext, null)
					?? throw new InvalidOperationException("Cannot retrieve DbSet<T>");
				localView = dbSet.GetType().GetProperty(nameof(DbSet<object>.Local)).GetValue(dbSet) as IEnumerable
					?? throw new InvalidOperationException("Cannot retrieve DbSet<T>.Local");
			}
			finally {
				dbContextSemaphore.Release();
			}

			var tupleToValueTupleDelegate = types.From.IsTuple() ? EfCoreUtils.GetOrCreateTupleToValueTupleDelegate(types.From) : null;
			var keyValuesDelegate = GetOrCreateKeyToValuesDelegate(types.From);

			// Create the matcher used to retrieve local elements (it will never throw because of SafeMatcher/EmptyMatcher), won't contain semaphore
			var normalizedElementsMatcherFactory = GetNormalizedMatchFactory(types, mappingOptions
				.ReplaceOrAdd<NestedSemaphoreContext>(c => c ?? NestedSemaphoreContext.Instance));
			try { 
				// Check if we are mapping a collection or just a single entity
				if (collectionElementTypes != null) {
					// Retrieve the factory which we will use to create instances of the collection and the actual type
					// which will be used, eg: to create an array we create a List<T> first, which will be later
					// converted to the desired array
					Func<object> collectionFactory;
					Type actualCollectionType;
					try {
						collectionFactory = ObjectFactory.CreateCollectionFactory(destinationType, out actualCollectionType);
					}
					catch (ObjectCreationException) {
						throw new MapNotFoundException(types);
					}

					// Since the method above is not 100% accurate in checking if the type is an actual collection
					// we check again here, if we do not get back a method to add elements then it is not a collection
					Action<object, object> addDelegate;
					try {
						addDelegate = ObjectFactory.GetCollectionCustomAddDelegate(actualCollectionType);
					}
					catch (InvalidOperationException) {
						throw new MapNotFoundException(types);
					}

					var collectionConversionDelegate = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType,destinationType);

					var localsAndPredicatesFactory = RetrieveLocalAndPredicateFactory(types.From, types.To, key, retrievalMode, localView, dbContext, normalizedElementsMatcherFactory);

					return new DisposableNewMapFactory(
						sourceType, destinationType,
						source => {
							NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));

							if (source == null || TypeUtils.IsDefaultValue(sourceType.UnwrapNullable(), source))
								return null;

							if (source is IEnumerable sourceEnumerable) {
								object result;
								try {
									// Retrieve tracked local entities (or attach them if needed) and create expressions for missing
									var localsAndPredicates = sourceEnumerable
										.Cast<object>()
										.Select(localsAndPredicatesFactory)
										.ToArray();

									// Query db for missing entities if needed
									if (retrievalMode == EntitiesRetrievalMode.LocalOrRemote || retrievalMode == EntitiesRetrievalMode.Remote) {
										var missingEntities = localsAndPredicates.Where(lp => lp.LocalEntity == null && lp.Key != null);
										if (missingEntities.Any()) {
											var filterExpression = GetEntitiesPredicate(types.From, types.To, key,
												missingEntities
												.Select(e => keyValuesDelegate.Invoke(e.Key))
												.ToArray());
											// Locking shouldn't be needed here because Queryable.Where creates just an Expression.Call
											var query = Queryable_Where.MakeGenericMethod(types.To).Invoke(null, new object[] { dbSet, filterExpression }) as IQueryable;

											dbContextSemaphore.Wait();
											try {
												if (retrievalMode == EntitiesRetrievalMode.LocalOrRemote) {
													EntityFrameworkQueryableExtensions_Load.MakeGenericMethod(types.To).Invoke(null, new object[] { query });
													// Not using Where() because the collection changes during iteration
													foreach (var localAndPredicate in localsAndPredicates) {
														if (localAndPredicate.LocalEntity != null || localAndPredicate.Key == null)
															continue;

														localAndPredicate.LocalEntity = localView
															.Cast<object>()
															.FirstOrDefault(e => normalizedElementsMatcherFactory.Invoke(localAndPredicate.Key, e));
													}
												}
												else {
													var entities = Enumerable_ToArray.MakeGenericMethod(types.To).Invoke(null, new object[] { query }) as IEnumerable
														?? throw new InvalidOperationException("Invalid result returned");
													foreach (var localAndPredicate in localsAndPredicates.Where(lp => lp.Key != null)) {
														localAndPredicate.LocalEntity = entities
															.Cast<object>()
															.FirstOrDefault(e => normalizedElementsMatcherFactory.Invoke(localAndPredicate.Key, e));
													}
												}
											}
											finally {
												dbContextSemaphore.Release();
											}
										}
									}

									// Create collection and populate it
									var destination = collectionFactory.Invoke();

									foreach (var localAndPredicate in localsAndPredicates) {
										addDelegate.Invoke(destination, localAndPredicate.LocalEntity);
									}

									result = collectionConversionDelegate.Invoke(destination);
								}
								catch (OperationCanceledException) {
									throw;
								}
								catch (Exception e) {
									throw new MappingException(e, (sourceType, destinationType));
								}

								// Should not happen
								NeatMapper.TypeUtils.CheckObjectType(result, destinationType);

								return result;
							}
							else
								throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
						},
						normalizedElementsMatcherFactory);
				}
				else {
					var attachEntityDelegate = GetOrCreateAttachEntityDelegate(types.To, key);

					return new DisposableNewMapFactory(
						sourceType, destinationType,
						source => {
							NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));

							if (source == null || TypeUtils.IsDefaultValue(sourceType.UnwrapNullable(), source))
								return null;

							object result;
							try {
								var keyValues = keyValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
									tupleToValueTupleDelegate.Invoke(source) :
									source);

								// Check how we need to retrieve the entity
								switch (retrievalMode) {
								case EntitiesRetrievalMode.Local:
								case EntitiesRetrievalMode.LocalOrAttach: {
									if (tupleToValueTupleDelegate != null)
										source = tupleToValueTupleDelegate.Invoke(source);

									dbContextSemaphore.Wait();
									try { 
										result = localView
											.Cast<object>()
											.FirstOrDefault(e => normalizedElementsMatcherFactory.Invoke(source, e));
									}
									finally {
										dbContextSemaphore.Release();
									}

									// Attach a new entity to the context if not found, and mark it as unchanged
									if (retrievalMode == EntitiesRetrievalMode.LocalOrAttach && result == null) 
										attachEntityDelegate.Invoke(ref result, keyValues, dbContextSemaphore, dbContext);
									break;
								}
								case EntitiesRetrievalMode.LocalOrRemote:
									dbContextSemaphore.Wait();
									try {
										result = dbContext.Find(types.To, keyValues);
									}
									finally {
										dbContextSemaphore.Release();
									}
									break;
								case EntitiesRetrievalMode.Remote: { 
									var expr = GetEntitiesPredicate(types.From, types.To, key, new object[][] { keyValues });

									dbContextSemaphore.Wait();
									try {
										result = Queryable_FirstOrDefault.MakeGenericMethod(types.To).Invoke(null, new object[] {
											Queryable_Where.MakeGenericMethod(types.To).Invoke(null, new object[] { dbSet, expr })
										});
									}
									finally {
										dbContextSemaphore.Release();
									}
									break;
								}
								default:
									throw new InvalidOperationException("Unknown retrieval mode");
								}
							}
							catch (OperationCanceledException) {
								throw;
							}
							catch (Exception e) {
								throw new MappingException(e, (sourceType, destinationType));
							}

							// Should not happen
							NeatMapper.TypeUtils.CheckObjectType(result, destinationType);

							return result;
						},
						normalizedElementsMatcherFactory);
				}
			}
			catch {
				normalizedElementsMatcherFactory.Dispose();
				throw;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public IMergeMapFactory MapMergeFactory(
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

			try { 
				(Type From, Type To)? collectionElementTypes = destinationType.IsCollection() && !destinationType.IsArray ?
					((Type From, Type To)?)(sourceType.GetEnumerableElementType(), destinationType.GetEnumerableElementType()) :
					null;

				(Type From, Type To) types = collectionElementTypes ?? (sourceType, destinationType);

				var throwOnDuplicateEntity = efCoreOptions?.ThrowOnDuplicateEntity
					?? _entityFrameworkCoreOptions.ThrowOnDuplicateEntity;

				// Retrieve the db context from the services if we need to attach entities
				DbContext dbContext;
				IKey key;
				if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
					dbContext = RetrieveDbContext(mappingOptions);
					key = _model.FindEntityType(types.To).FindPrimaryKey();
				}
				else {
					dbContext = null;
					key = null;
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

					var newFactory = MapNewFactory(sourceType, destinationType, mappingOptions);
					try { 
						var mergeFactory = MergeCollection(types, dbContext, key, entitiesRetrievalMode, destinationMappingOptions, throwOnDuplicateEntity);

						return new DisposableMergeMapFactory(
							sourceType, destinationType,
							(source, destination) => {
								try {
									NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));
									NeatMapper.TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

									if (source is IEnumerable sourceEnumerable) {
										// If we have to create the destination collection we forward to NewMap
										// Otherwise we must check that the collection can be mapped to
										if (destination == null)
											return newFactory.Invoke(source);
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

											mergeFactory.Invoke(destinationEnumerable, sourceEnumerable, sourceEntitiesEnumerable);

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
								catch (OperationCanceledException) {
									throw;
								}
								catch (MapNotFoundException) {
									throw;
								}
								catch (Exception e) {
									throw new MappingException(e, (sourceType, destinationType));
								}
							},
							destinationFactory, newFactory, mergeFactory);
					}
					catch {
						newFactory.Dispose();
						throw;
					}
				}
				else {
					var tupleToValueTupleDelegate = types.From.IsTuple() ? EfCoreUtils.GetOrCreateTupleToValueTupleDelegate(types.From) : null;
					var keyValuesDelegate = GetOrCreateKeyToValuesDelegate(types.From);

					var attachEntityDelegate = GetOrCreateAttachEntityDelegate(types.To, key);

					var dbContextSemaphore = dbContext != null ? EfCoreUtils.GetOrCreateSemaphoreForDbContext(dbContext) : null;

					return new DisposableMergeMapFactory(
						sourceType, destinationType,
						(source, destination) => {
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
										dbContextSemaphore.Wait();
										try{ 
											dbContext.Attach(destination);
										}
										finally {
											dbContextSemaphore.Release();
										}
										result = destination;
									}
									else {
										var keyValues = keyValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
											tupleToValueTupleDelegate.Invoke(source) :
											source);

										attachEntityDelegate.Invoke(ref result, keyValues, dbContextSemaphore, dbContext);
									}
								}
								else if ((result == null || destination != null) && destination != result && throwOnDuplicateEntity) {
									if (result != null) {
										var keyValues = keyValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
											tupleToValueTupleDelegate.Invoke(source) :
											source);
										throw new DuplicateEntityException($"A duplicate entity of type {types.To?.FullName ?? types.To.Name} was found for the key {string.Join(", ", keyValues)}");
									}
									else
										throw new DuplicateEntityException($"A non-null entity of type {types.To?.FullName ?? types.To.Name} was provided for a not found entity. When merging objects make sure that they match");
								}
							}
							catch (MappingException) {
								throw;
							}
							catch (OperationCanceledException) {
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
						},
						destinationFactory);
				}
			}
			catch {
				destinationFactory.Dispose();
				throw;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private DbContext RetrieveDbContext(MappingOptions mappingOptions) {
			var dbContext = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>()?.DbContextInstance;
			if (dbContext != null && dbContext.GetType() != _dbContextType)
				dbContext = null;

			if (dbContext == null) {
				try {
					dbContext = (mappingOptions?.GetOptions<MapperOverrideMappingOptions>()?.ServiceProvider ?? _serviceProvider)
						.GetRequiredService(_dbContextType) as DbContext;
				}
				catch { }
			}

			if (dbContext == null)
				throw new InvalidOperationException($"Could not retrieve a DbContext of type {_dbContextType.FullName ?? _dbContextType.Name}");

			return dbContext;
		}

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
