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
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which retrieves asynchronously entities from their keys (even composite keys
	/// as <see cref="Tuple"/> or <see cref="ValueTuple"/>, or shadow keys) from a <see cref="DbContext"/>.<br/>
	/// Supports new and merge maps, also supports collections (same as <see cref="AsyncCollectionMapper"/>
	/// but not nested).<br/>
	/// Entities may be searched locally in the <see cref="DbContext"/> first,
	/// otherwise an async query to the db will be made, depending on
	/// <see cref="EntityFrameworkCoreOptions.EntitiesRetrievalMode"/>
	/// (and <see cref="EntityFrameworkCoreMappingOptions.EntitiesRetrievalMode"/>).
	/// </summary>
	/// <inheritdoc cref="EntityFrameworkCoreMapper" path="/remarks"/>
	public sealed class AsyncEntityFrameworkCoreMapper : EntityFrameworkCoreBaseMapper, IAsyncMapper, IAsyncMapperFactory {
		/// <summary>
		/// <see cref="EntityFrameworkQueryableExtensions.ToArrayAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>
		/// </summary>
		private static readonly MethodInfo EntityFrameworkQueryableExtensions_ToArrayAsync = typeof(EntityFrameworkQueryableExtensions)
			.GetMethod(nameof(EntityFrameworkQueryableExtensions.ToArrayAsync))
				?? throw new InvalidOperationException("Could not find EntityFrameworkQueryableExtensions.ToArrayAsync<T>()");
		private static readonly MethodCacheFunc<Type, IQueryable, CancellationToken, Task<IEnumerable>> EntityFrameworkQueryableExtensionsToArrayAsync =
			new MethodCacheFunc<Type, IQueryable, CancellationToken, Task<IEnumerable>>(
				(q, _) => q.ElementType,
				t => EntityFrameworkQueryableExtensions_ToArrayAsync.MakeGenericMethod(t),
				"queryable", "cancellationToken");

		private static bool CheckAsyncCollectionMapperNestedContextRecursive(AsyncNestedMappingContext? context) {
			if (context == null)
				return false;
			if (context.ParentMapper is AsyncCollectionMapper)
				return true;
			return CheckAsyncCollectionMapperNestedContextRecursive(context.ParentContext);
		}


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
			EntityFrameworkCoreOptions? entityFrameworkCoreOptions = null,
			IMatcher? elementsMatcher = null,
			MergeCollectionsOptions? mergeCollectionsOptions = null) : 
				base(model, dbContextType, serviceProvider,
					entityFrameworkCoreOptions != null ? new EntityFrameworkCoreOptions(entityFrameworkCoreOptions) : null,
					elementsMatcher,
					mergeCollectionsOptions != null ? new MergeCollectionsOptions(mergeCollectionsOptions) : null) {}


		#region IAsyncMapper methods
		public bool CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapNewInternal(sourceType, destinationType, mappingOptions, out _, out _);
		}

		public bool CanMapAsyncMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapMergeInternal(sourceType, destinationType, mappingOptions, out _, out _);
		}

		public async Task<object?> MapAsync(
			object? source,
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapNewInternal(sourceType, destinationType, mappingOptions, out var isCollection, out var elementTypes))
				throw new MapNotFoundException(types);

			NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			if (source == null)
				return null;

			// Retrieve the db context from the services
			var dbContext = RetrieveDbContext(mappingOptions);
			var dbContextSemaphore = EfCoreUtils.GetOrCreateSemaphoreForDbContext(dbContext);

			var retrievalMode = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>()?.EntitiesRetrievalMode
				?? _entityFrameworkCoreOptions.EntitiesRetrievalMode;

			var key = _model.FindEntityType(elementTypes.Entity)!.FindPrimaryKey()!;

			var tupleToValueTupleDelegate = EfCoreUtils.GetOrCreateTupleToValueTupleDelegate(elementTypes.Key);
			var keyToValuesDelegate = GetOrCreateKeyToValuesDelegate(elementTypes.Key);
			var attachEntityDelegate = GetOrCreateAttachEntityDelegate(elementTypes.Entity, key);

			var dbSet = RetrieveDbSet(dbContext, elementTypes.Entity);
			var localView = GetLocalFromDbSet(dbSet);

			// Create the matcher used to retrieve local elements (it will never throw because of SafeMatcher/EmptyMatcher), won't contain semaphore
			using (var normalizedElementsMatcherFactory = GetNormalizedMatchFactory(elementTypes, (mappingOptions ?? MappingOptions.Empty)
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

					if(types.From.IsAsyncEnumerable()) {
						var moveNextAsyncDelegate = ObjectFactory.GetAsyncEnumeratorMoveNextAsync(elementTypes.Key);
						var currentDelegate = ObjectFactory.GetAsyncEnumeratorCurrent(elementTypes.Key);

						object result;
						try {
							var localsAndPredicates = new List<EntityMappingInfo>();

							await dbContextSemaphore.WaitAsync(cancellationToken);
							try {
								// Retrieve tracked local entities and normalize keys
								var asyncEnumerator = ObjectFactory.GetAsyncEnumerableGetAsyncEnumerator(elementTypes.Key)
									.Invoke(source, cancellationToken);
								try {
									while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
										var sourceElement = currentDelegate.Invoke(asyncEnumerator);
										if (sourceElement == null || TypeUtils.IsDefaultValue(elementTypes.Key.UnwrapNullable(), sourceElement)) {
											localsAndPredicates.Add(new EntityMappingInfo());
											continue;
										}

										if (tupleToValueTupleDelegate != null)
											sourceElement = tupleToValueTupleDelegate.Invoke(sourceElement);

										localsAndPredicates.Add(new EntityMappingInfo {
											Key = sourceElement,
											LocalEntity = localView
												.Cast<object>()
												.FirstOrDefault(e => normalizedElementsMatcherFactory.Invoke(sourceElement, e))
										});
									}
								}
								finally {
									await asyncEnumerator.DisposeAsync();
								}

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
										}

										var query = TypeUtils.QueryableWhere.Invoke(dbSet, filterExpression);

										var entities = await EntityFrameworkQueryableExtensionsToArrayAsync.Invoke(query, cancellationToken);

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

							foreach (var localAndPredicate in localsAndPredicates) {
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
					else { 
						if (source is IEnumerable sourceEnumerable) {
							object result;
							try {
								List<EntityMappingInfo> localsAndPredicates;
									
								await dbContextSemaphore.WaitAsync(cancellationToken);
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
											}

											var query = TypeUtils.QueryableWhere.Invoke(dbSet, filterExpression);

											var entities = await EntityFrameworkQueryableExtensionsToArrayAsync.Invoke(query, cancellationToken);

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

								foreach (var localAndPredicate in localsAndPredicates) {
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
							await dbContextSemaphore.WaitAsync(cancellationToken);
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

								if (result == null) {
									// Query db for missing entity if needed (this also performs a local search first),
									// or attach the missing entity
									if (retrievalMode == EntitiesRetrievalMode.LocalOrRemote)
										result = await dbContext.FindAsync(types.To, keyValues, cancellationToken);
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
				}
			}
		}

		public async Task<object?> MapAsync(
			object? source,
			Type sourceType,
			object? destination,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

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
			// because we might attach provided entities instead of creating new ones
			MappingOptions? destinationMappingOptions;
			if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
				destinationMappingOptions = (mappingOptions ?? MappingOptions.Empty)
					.ReplaceOrAdd<EntityFrameworkCoreMappingOptions>(
						o => new EntityFrameworkCoreMappingOptions(EntitiesRetrievalMode.LocalOnly, o?.DbContextInstance, o?.ThrowOnDuplicateEntity));
			}
			else
				destinationMappingOptions = mappingOptions;

			// Check if we are mapping a collection or just a single entity
			if (isCollection) {
				if (types.From.IsAsyncEnumerable()) {
					var moveNextAsyncDelegate = ObjectFactory.GetAsyncEnumeratorMoveNextAsync(elementTypes.Key);
					var currentDelegate = ObjectFactory.GetAsyncEnumeratorCurrent(elementTypes.Key);

					var newSourceType = typeof(IEnumerable<>).MakeGenericType(elementTypes.Key);

					// IAsyncEnumerable<T> would require enumerating all the values twice
					// in order to merge them, so we buffer them once and map them as IEnumerable<T>
					object sourceCollection;
					Type actualSourceCollectionType;
					try {
						sourceCollection = ObjectFactory.CreateCollectionFactory(newSourceType, out actualSourceCollectionType).Invoke();
					}
					catch (ObjectCreationException) {
						throw new MapNotFoundException(types);
					}

					// Since the method above is not 100% accurate in checking if the type is an actual collection
					// we check again here, if we do not get back a method to add elements then it is not a collection
					Action<object, object?> sourceAddDelegate;
					try {
						sourceAddDelegate = ObjectFactory.GetCollectionCustomAddDelegate(actualSourceCollectionType);
					}
					catch (InvalidOperationException) {
						throw new MapNotFoundException(types);
					}

					if (source == null)
						return destination;
					else {
						// If we have to create the destination collection we forward to NewMap
						// Otherwise we must check that the collection can be mapped to
						if (destination == null)
							return await MapAsync(source, sourceType, destinationType, mappingOptions, cancellationToken);
						else if (NeatMapper.TypeUtils.IsCollectionReadonly(destination)) {
							throw new InvalidOperationException("Cannot merge map to a readonly destination collection, destination type is: " +
								(destination.GetType().FullName ?? destination.GetType().Name));
						}

						// Retrieve all the values and buffer them
						var asyncEnumerator = ObjectFactory.GetAsyncEnumerableGetAsyncEnumerator(elementTypes.Key)
							.Invoke(source, cancellationToken);
						try {
							while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
								var sourceElement = currentDelegate.Invoke(asyncEnumerator);
								sourceAddDelegate.Invoke(sourceCollection, sourceElement);
							}
						}
						finally {
							await asyncEnumerator.DisposeAsync();
						}

						// Retrieve the elements and merge them
						if (destination is IEnumerable destinationEnumerable) {
							var sourceEntitiesEnumerable = await MapAsync(sourceCollection, newSourceType, destinationType, destinationMappingOptions, cancellationToken) as IEnumerable
								?? throw new InvalidOperationException("Invalid result"); // Should not happen

							using (var mergeFactory = MergeCollection(elementTypes, dbContext, key, entitiesRetrievalMode, destinationMappingOptions, throwOnDuplicateEntity)) {
								mergeFactory.Invoke(destinationEnumerable, (IEnumerable)sourceCollection, sourceEntitiesEnumerable);
							}
						}
						else
							throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
					}
				}
				else { 
					try {
						if (source is IEnumerable sourceEnumerable) {
							// If we have to create the destination collection we forward to NewMap
							// Otherwise we must check that the collection can be mapped to
							if (destination == null)
								return await MapAsync(source, sourceType, destinationType, mappingOptions, cancellationToken);
							else if (NeatMapper.TypeUtils.IsCollectionReadonly(destination)) {
								throw new InvalidOperationException("Cannot merge map to a readonly destination collection, destination type is: " +
									(destination.GetType().FullName ?? destination.GetType().Name));
							}

							if (destination is IEnumerable destinationEnumerable) {
								var sourceEntitiesEnumerable = await MapAsync(source, sourceType, destinationType, destinationMappingOptions, cancellationToken) as IEnumerable
									?? throw new InvalidOperationException("Invalid result"); // Should not happen

								using (var mergeFactory = MergeCollection(elementTypes, dbContext, key, entitiesRetrievalMode, destinationMappingOptions, throwOnDuplicateEntity)) {
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
					result = await MapAsync(source, sourceType, destinationType, destinationMappingOptions, cancellationToken);

					if (result == null && entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
						var dbContextSemaphore = EfCoreUtils.GetOrCreateSemaphoreForDbContext(dbContext!);
						await dbContextSemaphore.WaitAsync(cancellationToken);
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

		#region IAsyncMapperFactory methods
		public IAsyncNewMapFactory MapAsyncNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
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

			var retrievalMode = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>()?.EntitiesRetrievalMode
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

					if (types.From.IsAsyncEnumerable()) {
						var getAsyncEnumeratorDelegate = ObjectFactory.GetAsyncEnumerableGetAsyncEnumerator(elementTypes.Key);
						var moveNextAsyncDelegate = ObjectFactory.GetAsyncEnumeratorMoveNextAsync(elementTypes.Key);
						var currentDelegate = ObjectFactory.GetAsyncEnumeratorCurrent(elementTypes.Key);

						return new DisposableAsyncNewMapFactory(
							sourceType, destinationType,
							async (source, cancellationToken) => {
								NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));

								if (source == null)
									return null;
								else {
									object result;
									try {
										var localsAndPredicates = new List<EntityMappingInfo>();
										
										await dbContextSemaphore.WaitAsync(cancellationToken);
										try {
											// Retrieve tracked local entities and normalize keys
											var asyncEnumerator = getAsyncEnumeratorDelegate.Invoke(source, cancellationToken);
											try {
												while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
													var sourceElement = currentDelegate.Invoke(asyncEnumerator);
													if (sourceElement == null || TypeUtils.IsDefaultValue(elementTypes.Key.UnwrapNullable(), sourceElement)) { 
														localsAndPredicates.Add(new EntityMappingInfo());
														continue;
													}

													if (tupleToValueTupleDelegate != null)
														sourceElement = tupleToValueTupleDelegate.Invoke(sourceElement);

													localsAndPredicates.Add(new EntityMappingInfo {
														Key = sourceElement,
														LocalEntity = localView
															.Cast<object>()
															.FirstOrDefault(e => normalizedElementsMatcherFactory.Invoke(sourceElement, e))
													});
												}
											}
											finally {
												await asyncEnumerator.DisposeAsync();
											}

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
													}

													var query = TypeUtils.QueryableWhere.Invoke(dbSet, filterExpression);

													var entities = await EntityFrameworkQueryableExtensionsToArrayAsync.Invoke(query, cancellationToken);

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
							},
							normalizedElementsMatcherFactory);
					}
					else { 
						return new DisposableAsyncNewMapFactory(
							sourceType, destinationType,
							async (source, cancellationToken) => {
								NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));

								if (source is IEnumerable sourceEnumerable) {
									object result;
									try {
										List<EntityMappingInfo> localsAndPredicates;

										await dbContextSemaphore.WaitAsync(cancellationToken);
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
													}

													var query = TypeUtils.QueryableWhere.Invoke(dbSet, filterExpression);

													var entities = await EntityFrameworkQueryableExtensionsToArrayAsync.Invoke(query, cancellationToken);

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
				}
				else {
					return new DisposableAsyncNewMapFactory(
						sourceType, destinationType,
						async (source, cancellationToken) => {
							NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));

							if (source == null || TypeUtils.IsDefaultValue(sourceType.UnwrapNullable(), source))
								return null;

							object? result;
							try {
								var keyValues = keyToValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
									tupleToValueTupleDelegate.Invoke(source) :
									source);

								try { 
									await dbContextSemaphore.WaitAsync(cancellationToken);
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

										if (result == null) {
											// Query db for missing entity if needed (this also performs a local search first),
											// or attach the missing entity
											if (retrievalMode == EntitiesRetrievalMode.LocalOrRemote)
												result = await dbContext.FindAsync(types.To, keyValues, cancellationToken);
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

		public IAsyncMergeMapFactory MapAsyncMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapMergeInternal(sourceType, destinationType, mappingOptions, out var isCollection, out var elementTypes))
				throw new MapNotFoundException(types);

			var efCoreOptions = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>();
			var entitiesRetrievalMode = efCoreOptions?.EntitiesRetrievalMode
				?? _entityFrameworkCoreOptions.EntitiesRetrievalMode;

			// Adjust LocalOrAttach options to prevent attaching inside NewMap, we'll do it here when merging instead,
			// because we might attach provided entities instead of creating new ones
			MappingOptions? destinationMappingOptions;
			if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
				destinationMappingOptions = (mappingOptions ?? MappingOptions.Empty)
					.ReplaceOrAdd<EntityFrameworkCoreMappingOptions>(
						o => new EntityFrameworkCoreMappingOptions(EntitiesRetrievalMode.LocalOnly, o?.DbContextInstance, o?.ThrowOnDuplicateEntity));
			}
			else
				destinationMappingOptions = mappingOptions;

			var throwOnDuplicateEntity = efCoreOptions?.ThrowOnDuplicateEntity
				?? _entityFrameworkCoreOptions.ThrowOnDuplicateEntity;

			// Retrieve the db context from the services if we need to attach entities
			DbContext? dbContext;
			IKey? key;
			if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
				dbContext = RetrieveDbContext(mappingOptions);
				key = _model.FindEntityType(types.To)!.FindPrimaryKey()!;
			}
			else {
				dbContext = null;
				key = null;
			}

			// Check if we are mapping a collection or just a single entity
			if (isCollection) {
				var newFactory = MapAsyncNewFactory(sourceType, destinationType, mappingOptions);

				try {
					var mergeFactory = MergeCollection(elementTypes, dbContext, key, entitiesRetrievalMode, destinationMappingOptions, throwOnDuplicateEntity);

					try { 
						if (types.From.IsAsyncEnumerable()) {
							var getAsyncEnumeratorDelegate = ObjectFactory.GetAsyncEnumerableGetAsyncEnumerator(elementTypes.Key);
							var moveNextAsyncDelegate = ObjectFactory.GetAsyncEnumeratorMoveNextAsync(elementTypes.Key);
							var currentDelegate = ObjectFactory.GetAsyncEnumeratorCurrent(elementTypes.Key);

							var newSourceType = typeof(IEnumerable<>).MakeGenericType(elementTypes.Key);

							// IAsyncEnumerable<T> would require enumerating all the values twice
							// in order to merge them, so we buffer them once and map them as IEnumerable<T>
							Func<object> sourceCollectionFactory;
							Type actualSourceCollectionType;
							try {
								sourceCollectionFactory = ObjectFactory.CreateCollectionFactory(newSourceType, out actualSourceCollectionType);
							}
							catch (ObjectCreationException) {
								throw new MapNotFoundException(types);
							}
							
							// Since the method above is not 100% accurate in checking if the type is an actual collection
							// we check again here, if we do not get back a method to add elements then it is not a collection
							Action<object, object?> sourceAddDelegate;
							try {
								sourceAddDelegate = ObjectFactory.GetCollectionCustomAddDelegate(actualSourceCollectionType);
							}
							catch (InvalidOperationException) {
								throw new MapNotFoundException(types);
							}

							var destinationFactory = MapAsyncNewFactory(newSourceType, destinationType, destinationMappingOptions);

							try { 
								return new DisposableAsyncMergeMapFactory(
									sourceType, destinationType,
									async (source, destination, cancellationToken) => {
										try {
											NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));
											NeatMapper.TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

											if(source == null)
												return destination;
											else {
												// If we have to create the destination collection we forward to NewMap
												// Otherwise we must check that the collection can be mapped to
												if (destination == null)
													return await newFactory.Invoke(source, cancellationToken);
												else if (NeatMapper.TypeUtils.IsCollectionReadonly(destination)) {
													throw new InvalidOperationException("Cannot merge map to a readonly destination collection, destination type is: " +
														(destination.GetType().FullName ?? destination.GetType().Name));
												}

												// Retrieve all the values and buffer them
												var sourceCollection = sourceCollectionFactory.Invoke();
												var asyncEnumerator = getAsyncEnumeratorDelegate.Invoke(source, cancellationToken);
												try {
													while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
														var sourceElement = currentDelegate.Invoke(asyncEnumerator);
														sourceAddDelegate.Invoke(sourceCollection, sourceElement);
													}
												}
												finally {
													await asyncEnumerator.DisposeAsync();
												}

												// Retrieve the elements and merge them
												if (destination is IEnumerable destinationEnumerable) {
													var sourceEntitiesEnumerable = await destinationFactory.Invoke(sourceCollection, cancellationToken) as IEnumerable
														?? throw new InvalidOperationException("Invalid result"); // Should not happen

													mergeFactory.Invoke(destinationEnumerable, (IEnumerable)sourceCollection, sourceEntitiesEnumerable);
												}
												else
													throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
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
										NeatMapper.TypeUtils.CheckObjectType(destination, types.To);

										return destination;
									},
									destinationFactory, newFactory, mergeFactory);
							}
							catch {
								destinationFactory.Dispose();
								throw;
							}
						}
						else {
							var destinationFactory = MapAsyncNewFactory(sourceType, destinationType, destinationMappingOptions);

							try { 
							return new DisposableAsyncMergeMapFactory(
								sourceType, destinationType,
								async (source, destination, cancellationToken) => {
									try {
										NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));
										NeatMapper.TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

										if (source is IEnumerable sourceEnumerable) {
											// If we have to create the destination collection we forward to NewMap
											// Otherwise we must check that the collection can be mapped to
											if (destination == null)
												return await newFactory.Invoke(source, cancellationToken);
											else if (NeatMapper.TypeUtils.IsCollectionReadonly(destination)) {
												throw new InvalidOperationException("Cannot merge map to a readonly destination collection, destination type is: " +
													(destination.GetType().FullName ?? destination.GetType().Name));
											}

											if (destination is IEnumerable destinationEnumerable) {
												var sourceEntitiesEnumerable = (await destinationFactory.Invoke(source, cancellationToken)) as IEnumerable
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
								destinationFactory.Dispose();
								throw;
							}
						}
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

				var destinationFactory = MapAsyncNewFactory(sourceType, destinationType, destinationMappingOptions);

				try { 
					return new DisposableAsyncMergeMapFactory(
						sourceType, destinationType,
						async (source, destination, cancellationToken) => {
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
								result = await destinationFactory.Invoke(source, cancellationToken);

								if (result == null && entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach) {
									await dbContextSemaphore!.WaitAsync(cancellationToken);
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
				catch {
					destinationFactory.Dispose();
					throw;
				}
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

			// We could also map collections of keys/entities
			if ((sourceType.IsEnumerable() ? sourceType != typeof(string) : sourceType.IsAsyncEnumerable()) &&
				(destinationType.IsEnumerable() ? destinationType != typeof(string) : destinationType.IsAsyncEnumerable())) {

				isCollection = true;
				elementTypes = (sourceType.IsEnumerable() ? sourceType.GetEnumerableElementType() : sourceType.GetAsyncEnumerableElementType(),
					destinationType.IsEnumerable() ? destinationType.GetEnumerableElementType() : destinationType.GetAsyncEnumerableElementType());

				if (!ObjectFactory.CanCreateCollection(destinationType))
					return false;
			}
			else {
				isCollection = false;
				elementTypes = (sourceType, destinationType);
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
			if ((sourceType.IsEnumerable() ? sourceType != typeof(string) : sourceType.IsAsyncEnumerable()) &&
				destinationType.IsCollection()) {

				isCollection = true;
				elementTypes = (sourceType.IsEnumerable() ? sourceType.GetEnumerableElementType() : sourceType.GetAsyncEnumerableElementType(),
					destinationType.GetEnumerableElementType());

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
					dbContext = (mappingOptions?.GetOptions<AsyncMapperOverrideMappingOptions>()?.ServiceProvider ?? _serviceProvider)
						.GetRequiredService(_dbContextType) as DbContext;
				}
				catch { }
			}

			if (dbContext == null)
				throw new InvalidOperationException($"Could not retrieve a DbContext of type {_dbContextType.FullName ?? _dbContextType.Name}");

			return dbContext;
		}

		override protected bool CheckCollectionMapperNestedContextRecursive(MappingOptions? mappingOptions) {
			return CheckAsyncCollectionMapperNestedContextRecursive(mappingOptions?.GetOptions<AsyncNestedMappingContext>());
		}
	}
}
