using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
	/// <para>
	/// <see cref="IMapper"/> which retrieves entities from their keys (even composite keys
	/// as <see cref="Tuple"/> or <see cref="ValueTuple"/>, and shadow keys) from a <see cref="DbContext"/>.<br/>
	/// Supports new and merge maps, also supports collections (same as <see cref="CollectionMapper"/> but not nested).<br/>
	/// Entities may be searched locally in the <see cref="DbContext"/> first,
	/// otherwise a query to the db will be made, depending on
	/// <see cref="EntityFrameworkCoreOptions.EntitiesRetrievalMode"/> (and overrides).
	/// </para>
	/// <para>
	/// Also supports mapping keys (even composite keys as <see cref="Tuple"/> or <see cref="ValueTuple"/>,
	/// and shadow keys, and collections) to the corresponding typed <see cref="Expression{TDelegate}"/>
	/// (<see cref="Func{T, TResult}"/>) for querying (only new maps).
	/// </para>
	/// </summary>
	/// <remarks>
	/// Since a single <see cref="DbContext"/> instance cannot be used concurrently and it is not thread-safe
	/// on its own, every access to the provided <see cref="DbContext"/> instance and all its members
	/// (local and remote) for each map is protected by a semaphore.<br/>
	/// This makes this class thread-safe and concurrently usable, though not necessarily efficient to do so.<br/>
	/// Any external concurrent use of the <see cref="DbContext"/> instance is not monitored and could throw exceptions,
	/// so you should not be accessing the context externally while mapping.
	/// </remarks>
	public sealed class EntityFrameworkCoreMapper : EntityFrameworkCoreBaseMapper, IMapper, IMapperFactory {
		/// <summary>
		/// <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/>
		/// </summary>
		private static readonly MethodInfo Enumerable_ToArray = NeatMapper.TypeUtils.GetMethod(() => default(IEnumerable<object>)!.ToArray());
		private static readonly MethodCacheFunc<Type, IEnumerable, IEnumerable> EnumerableToArray =
			new MethodCacheFunc<Type, IEnumerable, IEnumerable>(
				e => e.GetType().GetEnumerableElementType(),
				t => Enumerable_ToArray.MakeGenericMethod(t),
				"enumerable");


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
			EntityFrameworkCoreOptions? entityFrameworkCoreOptions = null,
			IMatcher? elementsMatcher = null,
			MergeCollectionsOptions? mergeCollectionsOptions = null) :
				base(model, dbContextType, serviceProvider,
					entityFrameworkCoreOptions != null ? new EntityFrameworkCoreOptions(entityFrameworkCoreOptions) : null,
					elementsMatcher,
					mergeCollectionsOptions != null ? new MergeCollectionsOptions(mergeCollectionsOptions) : null) {}


		#region IMapper methods
		public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapNewInternal(sourceType, destinationType, mappingOptions, out _, out _);
		}

		public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapMergeInternal(sourceType, destinationType, mappingOptions, out _, out _);
		}

		public object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapNewInternal(sourceType, destinationType, mappingOptions, out var isCollection, out var elementTypes))
				throw new MapNotFoundException(types);

			NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			if(source == null)
				return null;

			var key = _model.FindEntityType(elementTypes.Entity)!.FindPrimaryKey()!;

			var tupleToValueTupleDelegate = EfCoreUtils.GetOrCreateTupleToValueTupleDelegate(elementTypes.Key);
			var keyToValuesDelegate = GetOrCreateKeyToValuesDelegate(elementTypes.Key);

			// Check if we are mapping keys to lambda expressions or keys to entities
			if (destinationType.IsGenericType && destinationType.GetGenericTypeDefinition() == typeof(Expression<>)) {
				if (isCollection) {
					if (source is IEnumerable sourceEnumerable) {
						var keysValues = sourceEnumerable
							.Cast<object>()
							.Where(sourceElement => sourceElement != null)
							.Select(sourceElement => {
								if (tupleToValueTupleDelegate != null)
									sourceElement = tupleToValueTupleDelegate.Invoke(sourceElement);
								return keyToValuesDelegate(sourceElement);
							})
							.ToList();
						try {
							if (!keysValues.Any(v => v.All(k => k != null)))
								return null;

							return GetEntitiesPredicate(
								elementTypes.Key,
								elementTypes.Entity,
								key,
								keysValues.Where(sourceElement => sourceElement.All(k => k != null)));
						}
						finally {
							foreach(var keysValue in keysValues) {
								ArrayPool.Return(keysValue);
							}
						}
					}
					else
						throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
				}
				else {
					var keyValues = keyToValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
						tupleToValueTupleDelegate.Invoke(source) :
						source);

					try { 
						if(keyValues.Any(k => k == null))
							return null;

						return GetEntitiesPredicate(elementTypes.Key, elementTypes.Entity, key, [keyValues]);
					}
					finally {
						ArrayPool.Return(keyValues);
					}
				}
			}
			else { 
				// Retrieve the db context from the services
				var dbContext = RetrieveDbContext(mappingOptions);

				var dbContextSemaphore = EfCoreUtils.GetOrCreateSemaphoreForDbContext(dbContext);

				var efCoreMappingOptions = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>();
				var entitiesRetrievalMode = efCoreMappingOptions?.EntitiesRetrievalMode
					?? _entityFrameworkCoreOptions.EntitiesRetrievalMode;

				var attachEntityDelegate = GetOrCreateAttachEntityDelegate(elementTypes.Entity, key);

				var dbSet = RetrieveDbSet(dbContext, elementTypes.Entity);
				var localView = GetLocalFromDbSet(dbSet);

				// Create the matcher used to retrieve local elements (it will never throw because of SafeMatcher/EmptyMatcher), won't contain semaphore
				using(var normalizedElementsMatcherFactory = GetNormalizedMatchFactory(elementTypes, (mappingOptions ?? MappingOptions.Empty)
					.ReplaceOrAdd<NestedSemaphoreContext>(c => c ?? NestedSemaphoreContext.Instance))) { 

					// Check if we are mapping a collection or just a single entity
					if (isCollection) {
						// Create the collection and retrieve the actual type which will be used,
						// eg: to create an array we create a List<T> first, which will be later
						// converted to the desired array
						object destination;
						Type actualCollectionType;
						try {
							destination = ObjectFactory.CreateCollectionFactory(types.To, out actualCollectionType).Invoke();
						}
						catch (ObjectCreationException) {
							throw new MapNotFoundException(types);
						}

						// Since the method above is not 100% accurate in checking if the type is an actual collection
						// we check again here, if we do not get back a method to add elements then it is not a collection
						Action<object, object?> addDelegate;
						try {
							addDelegate = ObjectFactory.GetCollectionCustomAddDelegate(actualCollectionType);
						}
						catch (InvalidOperationException) {
							throw new MapNotFoundException(types);
						}

						if (source is IEnumerable sourceEnumerable) {
							object result;
							try {
								List<EntityMappingInfo> localsAndPredicates;

								dbContextSemaphore.Wait();
								try {
									// Retrieve tracked local entities and normalize keys
									localsAndPredicates = sourceEnumerable
										.Cast<object>()
										.Select(sourceElement => {
											if (sourceElement == null || TypeUtils.IsDefaultValue(elementTypes.Key.UnwrapNullable(), sourceElement))
												return new EntityMappingInfo();

											if (tupleToValueTupleDelegate != null)
												sourceElement = tupleToValueTupleDelegate.Invoke(sourceElement);

											return new EntityMappingInfo {
												Key = sourceElement,
												LocalEntity = localView
													.Cast<object>()
													.FirstOrDefault(e => normalizedElementsMatcherFactory.Invoke(sourceElement, e))
											};
										})
										.ToList();

									// Query db for missing entities if needed,
									// or attach missing elements
									if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrRemote) {
										var missingEntities = localsAndPredicates.Where(lp => lp.LocalEntity == null && lp.Key != null);
										if (missingEntities.Any()) {
											var missingKeys = ListsPool.Get();
											LambdaExpression filterExpression;
											try { 
												foreach(var missingEntity in missingEntities) {
													missingKeys.Add(keyToValuesDelegate.Invoke(missingEntity.Key));
												}

												filterExpression = GetEntitiesPredicate(elementTypes.Key, elementTypes.Entity, key, missingKeys);
											}
											finally {
												foreach (var missingKey in missingKeys) {
													ArrayPool.Return(missingKey);
												}
												ListsPool.Return(missingKeys);
											}

											var query = TypeUtils.QueryableWhere.Invoke(dbSet, filterExpression);

											var entities = EnumerableToArray.Invoke(query);

											// Not using Where() because the collection changes during iteration
											foreach (var localAndPredicate in localsAndPredicates) {
												if (localAndPredicate.LocalEntity != null || localAndPredicate.Key == null)
													continue;

												localAndPredicate.LocalEntity = entities
													.Cast<object>()
													.FirstOrDefault(e => normalizedElementsMatcherFactory.Invoke(localAndPredicate.Key, e));
											}
										}
									}
									else if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
										// Not using Where() because the collection changes during iteration
										foreach (var localAndPredicate in localsAndPredicates) {
											if (localAndPredicate.LocalEntity != null || localAndPredicate.Key == null)
												continue;

											var keyValues = keyToValuesDelegate.Invoke(localAndPredicate.Key);
											try { 
												localAndPredicate.LocalEntity = attachEntityDelegate.Invoke(keyValues, dbContext);
											}
											finally {
												ArrayPool.Return(keyValues);
											}
										}
									}
								}
								finally {
									dbContextSemaphore.Release();
								}

								var removeNullEntities = efCoreMappingOptions?.IgnoreNullEntities
									?? _entityFrameworkCoreOptions.IgnoreNullEntities;

								foreach (var localAndPredicate in localsAndPredicates) {
									if(!removeNullEntities || localAndPredicate.LocalEntity != null)
										addDelegate.Invoke(destination, localAndPredicate.LocalEntity);
								}

								result = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType, types.To).Invoke(destination);
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
					}
					else {
						if (TypeUtils.IsDefaultValue(sourceType.UnwrapNullable(), source))
							return null;

						object? result;
						try {
							var keyValues = keyToValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
								tupleToValueTupleDelegate.Invoke(source) :
								source);

							try { 
								dbContextSemaphore.Wait();
								try {
									// Retrieve the tracked local entity (only if we are not going to retrieve it remotely,
									// in which case we can use Find directly)
									if (entitiesRetrievalMode != EntitiesRetrievalMode.LocalOrRemote) { 
										result = localView
											.Cast<object>()
											.FirstOrDefault(e => normalizedElementsMatcherFactory.Invoke(source, e));
									}
									else
										result = null;

									if (result == null) {
										// Query db for missing entity if needed (this also performs a local search first),
										// or attach the missing entity
										if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrRemote)
											result = dbContext.Find(types.To, keyValues);
										else if(entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach)
											result = attachEntityDelegate.Invoke(keyValues, dbContext);
									}
								}
								finally {
									dbContextSemaphore.Release();
								}
							}
							finally {
								ArrayPool.Return(keyValues);
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
					}
				}
			}
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapMergeInternal(sourceType, destinationType, mappingOptions, out var isCollection, out var elementTypes))
				throw new MapNotFoundException(types);

			NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			NeatMapper.TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			var efCoreMappingOptions = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>();
			var entitiesRetrievalMode = efCoreMappingOptions?.EntitiesRetrievalMode
				?? _entityFrameworkCoreOptions.EntitiesRetrievalMode;
			var throwOnDuplicateEntity = efCoreMappingOptions?.ThrowOnDuplicateEntity
				?? _entityFrameworkCoreOptions.ThrowOnDuplicateEntity;

			// Retrieve the db context from the services if we need to attach entities
			DbContext? dbContext;
			IKey? key;
			if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
				dbContext = RetrieveDbContext(mappingOptions);
				key = _model.FindEntityType(elementTypes.Entity)!.FindPrimaryKey()!;
			}
			else {
				dbContext = null;
				key = null;
			}

			// Adjust LocalOrAttach options to prevent attaching inside NewMap, we'll do it here when merging instead,
			// because we might attach provided entities instead of creating new ones, also do not remove null entities yet
			var destinationMappingOptions = CreateDestinationMappingOptions(mappingOptions);

			// Check if we are mapping a collection or just a single entity
			if (isCollection) {
				try {
					if (source is IEnumerable sourceEnumerable) {
						// If we have to create the destination collection we forward to NewMap
						// Otherwise we must check that the collection can be mapped to
						if (destination == null)
							return Map(source, sourceType, destinationType, mappingOptions);
						else if (NeatMapper.TypeUtils.IsCollectionReadonly(destination)) {
							throw new InvalidOperationException("Cannot merge map to a readonly destination collection, destination type is: " +
								(destination.GetType().FullName ?? destination.GetType().Name));
						}

						if (destination is IEnumerable destinationEnumerable) {
							var sourceEntitiesEnumerable = Map(source, sourceType, destinationType, destinationMappingOptions) as IEnumerable
								?? throw new InvalidOperationException("Invalid result"); // Should not happen

							using(var mergeFactory = MergeCollection(elementTypes, dbContext, key, mappingOptions, throwOnDuplicateEntity)) { 
								mergeFactory.Invoke(destinationEnumerable, sourceEnumerable, sourceEntitiesEnumerable);
							}
						}
						else
							throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
					}
					else if (source == null)
						return destination;
					else
						throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
				}
				catch (MappingException) {
					throw;
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception e) {
					throw new MappingException(e, (sourceType, destinationType));
				}

				// Should not happen
				NeatMapper.TypeUtils.CheckObjectType(destination, types.To);

				return destination;
			}
			else {
				var tupleToValueTupleDelegate = EfCoreUtils.GetOrCreateTupleToValueTupleDelegate(elementTypes.Key);
				var keyToValuesDelegate = GetOrCreateKeyToValuesDelegate(elementTypes.Key);

				object? result;
				try {
					if (source == null || TypeUtils.IsDefaultValue(types.From.UnwrapNullable(), source)) {
						if (destination != null && throwOnDuplicateEntity)
							throw new DuplicateEntityException($"A non-null entity of type {types.To.FullName ?? types.To.Name} was provided for the default key. When merging objects make sure that they match");

						return null;
					}

					// Forward the retrieval to NewMap, since we have to retrieve/create a new entity
					result = Map(source, sourceType, destinationType, destinationMappingOptions);

					if (result == null && entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
						var dbContextSemaphore = EfCoreUtils.GetOrCreateSemaphoreForDbContext(dbContext!);
						dbContextSemaphore.Wait();
						try {
							if (destination != null) {
								dbContext!.Attach(destination);

								result = destination;
							}
							else {
								var keyValues = keyToValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
									tupleToValueTupleDelegate.Invoke(source) :
									source);

								try { 
									result = GetOrCreateAttachEntityDelegate(elementTypes.Entity, key!).Invoke(keyValues, dbContext!);
								}
								finally {
									ArrayPool.Return(keyValues);
								}
							}
						}
						finally {
							dbContextSemaphore.Release();
						}
					}
					else if ((result == null || destination != null) && destination != result && throwOnDuplicateEntity) {
						if (result != null) {
							var keyValues = keyToValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
								tupleToValueTupleDelegate.Invoke(source) :
								source);
							try { 
								throw new DuplicateEntityException($"A duplicate entity of type {types.To.FullName ?? types.To.Name} was found for the key {string.Join(", ", keyValues)}");
							}
							finally {
								ArrayPool.Return(keyValues);
							}
						}
						else
							throw new DuplicateEntityException($"A non-null entity of type {types.To.FullName ?? types.To.Name} was provided for a not found entity. When merging objects make sure that they match");
					}
				}
				catch (MappingException) {
					throw;
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
		}
		#endregion

		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapNewInternal(sourceType, destinationType, mappingOptions, out var isCollection, out var elementTypes))
				throw new MapNotFoundException(types);

			// Retrieve the db context from the services
			var dbContext = RetrieveDbContext(mappingOptions);
			var dbContextSemaphore = EfCoreUtils.GetOrCreateSemaphoreForDbContext(dbContext);

			var efCoreMappingOptions = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>();
			var retrievalMode = efCoreMappingOptions?.EntitiesRetrievalMode
				?? _entityFrameworkCoreOptions.EntitiesRetrievalMode;

			var key = _model.FindEntityType(elementTypes.Entity)!.FindPrimaryKey()!;
			IQueryable dbSet;
			IEnumerable localView;
			dbContextSemaphore.Wait();
			try {
				dbSet = RetrieveDbSet(dbContext, elementTypes.Entity);
				localView = GetLocalFromDbSet(dbSet);
			}
			finally {
				dbContextSemaphore.Release();
			}

			var tupleToValueTupleDelegate = EfCoreUtils.GetOrCreateTupleToValueTupleDelegate(elementTypes.Key);
			var keyToValuesDelegate = GetOrCreateKeyToValuesDelegate(elementTypes.Key);
			var attachEntityDelegate = GetOrCreateAttachEntityDelegate(elementTypes.Entity, key);

			// Create the matcher used to retrieve local elements (it will never throw because of SafeMatcher/EmptyMatcher), won't contain semaphore
			var normalizedElementsMatcherFactory = GetNormalizedMatchFactory(elementTypes, (mappingOptions ?? MappingOptions.Empty)
				.ReplaceOrAdd<NestedSemaphoreContext>(c => c ?? NestedSemaphoreContext.Instance));

			try { 
				// Check if we are mapping a collection or just a single entity
				if (isCollection) {
					// Retrieve the factory which we will use to create instances of the collection and the actual type
					// which will be used, eg: to create an array we create a List<T> first, which will be later
					// converted to the desired array
					Func<object> collectionFactory;
					Type actualCollectionType;
					try {
						collectionFactory = ObjectFactory.CreateCollectionFactory(types.To, out actualCollectionType);
					}
					catch (ObjectCreationException) {
						throw new MapNotFoundException(types);
					}

					// Since the method above is not 100% accurate in checking if the type is an actual collection
					// we check again here, if we do not get back a method to add elements then it is not a collection
					Action<object, object?> addDelegate;
					try {
						addDelegate = ObjectFactory.GetCollectionCustomAddDelegate(actualCollectionType);
					}
					catch (InvalidOperationException) {
						throw new MapNotFoundException(types);
					}

					var collectionConversionDelegate = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType, types.To);

					var removeNullEntities = efCoreMappingOptions?.IgnoreNullEntities
						?? _entityFrameworkCoreOptions.IgnoreNullEntities;

					return new DisposableNewMapFactory(
						sourceType, destinationType,
						source => {
							NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));

							if (source is IEnumerable sourceEnumerable) {
								object result;
								try {
									List<EntityMappingInfo> localsAndPredicates;

									dbContextSemaphore.Wait();
									try {
										// Retrieve tracked local entities and normalize keys
										localsAndPredicates = sourceEnumerable
											.Cast<object>()
											.Select(sourceElement => {
												if (sourceElement == null || TypeUtils.IsDefaultValue(elementTypes.Key.UnwrapNullable(), sourceElement))
													return new EntityMappingInfo();

												if (tupleToValueTupleDelegate != null)
													sourceElement = tupleToValueTupleDelegate.Invoke(sourceElement);

												return new EntityMappingInfo {
													Key = sourceElement,
													LocalEntity = localView
														.Cast<object>()
														.FirstOrDefault(e => normalizedElementsMatcherFactory.Invoke(sourceElement, e))
												};
											})
											.ToList();

										// Query db for missing entities if needed,
										// or attach missing elements
										if (retrievalMode == EntitiesRetrievalMode.LocalOrRemote) {
											var missingEntities = localsAndPredicates.Where(lp => lp.LocalEntity == null && lp.Key != null);
											if (missingEntities.Any()) {
												var missingKeys = ListsPool.Get();
												LambdaExpression filterExpression;
												try {
													foreach (var missingEntity in missingEntities) {
														missingKeys.Add(keyToValuesDelegate.Invoke(missingEntity.Key));
													}

													filterExpression = GetEntitiesPredicate(elementTypes.Key, elementTypes.Entity, key, missingKeys);
												}
												finally {
													foreach (var missingKey in missingKeys) {
														ArrayPool.Return(missingKey);
													}
													ListsPool.Return(missingKeys);
												}

												var query = TypeUtils.QueryableWhere.Invoke(dbSet, filterExpression);

												var entities = EnumerableToArray.Invoke(query);

												// Not using Where() because the collection changes during iteration
												foreach (var localAndPredicate in localsAndPredicates) {
													if (localAndPredicate.LocalEntity != null || localAndPredicate.Key == null)
														continue;

													localAndPredicate.LocalEntity = entities
														.Cast<object>()
														.FirstOrDefault(e => normalizedElementsMatcherFactory.Invoke(localAndPredicate.Key, e));
												}
											}
										}
										else if (retrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
											// Not using Where() because the collection changes during iteration
											foreach (var localAndPredicate in localsAndPredicates) {
												if (localAndPredicate.LocalEntity != null || localAndPredicate.Key == null)
													continue;

												var keyValues = keyToValuesDelegate.Invoke(localAndPredicate.Key);
												try { 
													localAndPredicate.LocalEntity = attachEntityDelegate.Invoke(keyValues, dbContext);
												}
												finally {
													ArrayPool.Return(keyValues);
												}
											}
										}
									}
									finally {
										dbContextSemaphore.Release();
									}

									// Create collection and populate it
									var destination = collectionFactory.Invoke();

									foreach (var localAndPredicate in localsAndPredicates) {
										if (!removeNullEntities || localAndPredicate.LocalEntity != null)
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
							else if(source == null)
								return null;
							else
								throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
						},
						normalizedElementsMatcherFactory);
				}
				else {
					return new DisposableNewMapFactory(
						sourceType, destinationType,
						source => {
							NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));

							if (source == null || TypeUtils.IsDefaultValue(sourceType.UnwrapNullable(), source))
								return null;

							object? result;
							try {
								var keyValues = keyToValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
									tupleToValueTupleDelegate.Invoke(source) :
									source);

								try { 
									dbContextSemaphore.Wait();
									try {
										// Retrieve the tracked local entity (only if we are not going to retrieve it remotely,
										// in which case we can use Find directly)
										if (retrievalMode != EntitiesRetrievalMode.LocalOrRemote) {
											result = localView
												.Cast<object>()
												.FirstOrDefault(e => normalizedElementsMatcherFactory.Invoke(source, e));
										}
										else
											result = null;

										if(result == null) {
											// Query db for missing entity if needed (this also performs a local search first),
											// or attach the missing entity
											if (retrievalMode == EntitiesRetrievalMode.LocalOrRemote) 
												result = dbContext.Find(types.To, keyValues);
											else if (retrievalMode == EntitiesRetrievalMode.LocalOrAttach)
												result = attachEntityDelegate.Invoke(keyValues, dbContext);
										}
									}
									finally {
										dbContextSemaphore.Release();
									}
								}
								finally {
									ArrayPool.Return(keyValues);
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
		}

		public IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapMergeInternal(sourceType, destinationType, mappingOptions, out var isCollection, out var elementTypes))
				throw new MapNotFoundException(types);

			var efCoreMappingOptions = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>();
			var entitiesRetrievalMode = efCoreMappingOptions?.EntitiesRetrievalMode
				?? _entityFrameworkCoreOptions.EntitiesRetrievalMode;

			// Adjust LocalOrAttach options to prevent attaching inside NewMap, we'll do it here when merging instead,
			// because we might attach provided entities instead of creating new ones, also do not remove null entities yet
			var destinationFactory = MapNewFactory(sourceType, destinationType, CreateDestinationMappingOptions(mappingOptions));

			try { 
				var throwOnDuplicateEntity = efCoreMappingOptions?.ThrowOnDuplicateEntity
					?? _entityFrameworkCoreOptions.ThrowOnDuplicateEntity;

				// Retrieve the db context from the services if we need to attach entities
				DbContext? dbContext;
				IKey? key;
				if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
					dbContext = RetrieveDbContext(mappingOptions);
					key = _model.FindEntityType(elementTypes.Entity)!.FindPrimaryKey()!;
				}
				else {
					dbContext = null;
					key = null;
				}

				// Check if we are mapping a collection or just a single entity
				if (isCollection) {
					var newFactory = MapNewFactory(sourceType, destinationType, mappingOptions);

					try { 
						var mergeFactory = MergeCollection(elementTypes, dbContext, key, mappingOptions, throwOnDuplicateEntity);

						try { 
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
											else if(NeatMapper.TypeUtils.IsCollectionReadonly(destination)) {
												throw new InvalidOperationException("Cannot merge map to a readonly destination collection, destination type is: " +
													(destination.GetType().FullName ?? destination.GetType().Name));
											}

											if (destination is IEnumerable destinationEnumerable) {
												var sourceEntitiesEnumerable = destinationFactory.Invoke(source) as IEnumerable
													?? throw new InvalidOperationException("Invalid result"); // Should not happen

												mergeFactory.Invoke(destinationEnumerable, sourceEnumerable, sourceEntitiesEnumerable);
											}
											else
												throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
										}
										else if (source == null)
											return destination;
										else
											throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
									}
									catch (MappingException) {
										throw;
									}
									catch (OperationCanceledException) {
										throw;
									}
									catch (Exception e) {
										throw new MappingException(e, (sourceType, destinationType));
									}

									// Should not happen
									NeatMapper.TypeUtils.CheckObjectType(destination, types.To);

									return destination;
								},
								destinationFactory, newFactory, mergeFactory);
						}
						catch {
							mergeFactory.Dispose();
							throw;
						}
					}
					catch {
						newFactory.Dispose();
						throw;
					}
				}
				else {
					var tupleToValueTupleDelegate = EfCoreUtils.GetOrCreateTupleToValueTupleDelegate(elementTypes.Key);
					var keyToValuesDelegate = GetOrCreateKeyToValuesDelegate(elementTypes.Key);

					var attachEntityDelegate = dbContext != null ? GetOrCreateAttachEntityDelegate(elementTypes.Entity, key!) : null;
					var dbContextSemaphore = dbContext != null ? EfCoreUtils.GetOrCreateSemaphoreForDbContext(dbContext) : null;

					return new DisposableMergeMapFactory(
						sourceType, destinationType,
						(source, destination) => {
							NeatMapper.TypeUtils.CheckObjectType(source, types.From, nameof(source));
							NeatMapper.TypeUtils.CheckObjectType(destination, types.To, nameof(destination));

							object? result;
							try {
								if (source == null || TypeUtils.IsDefaultValue(types.From.UnwrapNullable(), source)) {
									if (destination != null && throwOnDuplicateEntity)
										throw new DuplicateEntityException($"A non-null entity of type {types.To.FullName ?? types.To.Name} was provided for the default key. When merging objects make sure that they match");

									return null;
								}

								// Forward the retrieval to NewMap, since we have to retrieve/create a new entity
								result = destinationFactory.Invoke(source);

								if (result == null && entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
									dbContextSemaphore!.Wait();
									try {
										if (destination != null) {
											dbContext!.Attach(destination);
										
											result = destination;
										}
										else {
											var keyValues = keyToValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
												tupleToValueTupleDelegate.Invoke(source) :
												source);

											try { 
												result = attachEntityDelegate!.Invoke(keyValues, dbContext!);
											}
											finally {
												ArrayPool.Return(keyValues);
											}
										}
									}
									finally {
										dbContextSemaphore.Release();
									}
								}
								else if ((result == null || destination != null) && destination != result && throwOnDuplicateEntity) {
									if (result != null) {
										var keyValues = keyToValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
											tupleToValueTupleDelegate.Invoke(source) :
											source);
										try { 
											throw new DuplicateEntityException($"A duplicate entity of type {types.To.FullName ?? types.To.Name} was found for the key {string.Join(", ", keyValues)}");
										}
										finally {
											ArrayPool.Return(keyValues);
										}
									}
									else
										throw new DuplicateEntityException($"A non-null entity of type {types.To.FullName ?? types.To.Name} was provided for a not found entity. When merging objects make sure that they match");
								}
							}
							catch (MappingException) {
								throw;
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
						destinationFactory);
				}
			}
			catch {
				destinationFactory.Dispose();
				throw;
			}
		}
		#endregion


		private bool CanMapNewInternal(
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions,
			out bool isCollection,
			out (Type Key, Type Entity) elementTypes) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Prevent being used by a collection mapper
			if (CheckCollectionMapperNestedContextRecursive(mappingOptions)) {
				isCollection = false;
				elementTypes = default;

				return false;
			}

			// Check if we are mapping keys to lambda expressions or keys to entities
			if (destinationType.IsGenericType && destinationType.GetGenericTypeDefinition() == typeof(Expression<>)) {
				// Check if the lambda expression matches the delegate
				var delegateType = destinationType.GetGenericArguments()[0];
				if(!delegateType.IsGenericType || delegateType.GetGenericTypeDefinition() != typeof(Func<,>)) {
					isCollection = false;
					elementTypes = default;
					return false;
				}
				var delegateArguments = delegateType.GetGenericArguments();
				if (delegateArguments[1] != typeof(bool)) {
					isCollection = false;
					elementTypes = default;
					return false;
				}

				// We could also map collections of keys
				if (sourceType.IsEnumerable() && sourceType != typeof(string)) {
					isCollection = true;
					elementTypes = (sourceType.GetEnumerableElementType(), delegateArguments[0]);
				}
				else {
					isCollection = false;
					elementTypes = (sourceType, delegateArguments[0]);
				}
			}
			else { 
				// We could also map collections of keys/entities
				if (sourceType.IsEnumerable() && sourceType != typeof(string) &&
					destinationType.IsEnumerable() && destinationType != typeof(string)) {

					isCollection = true;
					elementTypes = (sourceType.GetEnumerableElementType(), destinationType.GetEnumerableElementType());

					if (!ObjectFactory.CanCreateCollection(destinationType))
						return false;
				}
				else {
					isCollection = false;
					elementTypes = (sourceType, destinationType);
				}
			}

			return CanMapTypesInternal(elementTypes);
		}

		private bool CanMapMergeInternal(
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions,
			out bool isCollection,
			out (Type Key, Type Entity) elementTypes) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Prevent being used by a collection mapper
			if (CheckCollectionMapperNestedContextRecursive(mappingOptions)) {
				isCollection = false;
				elementTypes = default;

				return false;
			}

			// We could also map collections of keys/entities
			if (sourceType.IsEnumerable() && sourceType != typeof(string) &&
				destinationType.IsCollection()) {

				isCollection = true;
				elementTypes = (sourceType.GetEnumerableElementType(), destinationType.GetEnumerableElementType());

				if (destinationType.IsArray || !ObjectFactory.CanCreateCollection(destinationType))
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
			}
			else {
				isCollection = false;
				elementTypes = (sourceType, destinationType);
			}

			return CanMapTypesInternal(elementTypes);
		}

		private DbContext RetrieveDbContext(MappingOptions? mappingOptions) {
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

		override protected bool CheckCollectionMapperNestedContextRecursive(MappingOptions? mappingOptions) {
			return mappingOptions
				?.GetOptions<NestedMappingContext>()
				?.CheckRecursive(c => c.ParentMapper is CollectionMapper) == true;
		}
	}
}
