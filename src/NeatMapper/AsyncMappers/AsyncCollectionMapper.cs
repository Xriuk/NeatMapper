using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which maps collections by using another <see cref="IAsyncMapper"/> to map elements.
	/// <para>
	/// For new maps creates a new <see cref="IEnumerable{T}"/> (derived from <see cref="ICollection{T}"/>
	/// plus some special types like below) or <see cref="IAsyncEnumerable{T}"/> (even nested and readonly),
	/// from another <see cref="IEnumerable{T}"/> or <see cref="IAsyncEnumerable{T}"/>, asynchronously.<br/>
	/// Elements are then mapped with another <see cref="IAsyncMapper"/> by trying new map first, then merge map.<br/>
	/// Special collections which can be created are:
	/// <list type="bullet">
	/// <item><see cref="Stack{T}"/></item>
	/// <item><see cref="Queue{T}"/></item>
	/// <item><see cref="string"/> (considered as a collection of <see cref="char"/>s)</item>
	/// </list>
	/// Also supports parallel mapping via <see cref="AsyncCollectionMappersOptions"/> (and overrides).
	/// </para>
	/// <para>
	/// For merge maps merges a <see cref="IEnumerable{T}"/> or <see cref="IAsyncEnumerable{T}"/>
	/// (even nested) with an existing <see cref="ICollection{T}"/> (not readonly) asynchronously,
	/// will create a new <see cref="ICollection{T}"/> if destination is null.<br/>
	/// If <see cref="MergeCollectionsOptions.RecreateReadonlyDestination"/> (or overrides) is
	/// <see langword="true"/>, destination collections can also be <see cref="IEnumerable{T}"/>
	/// or <see cref="IAsyncEnumerable{T}"/> (also readonly), just like new maps.<br/>
	/// Will try to match elements of the source collection with the destination by using an
	/// <see cref="IMatcher"/> if provided:
	/// <list type="bullet">
	/// <item>
	/// If a match is found will try to merge the two elements or will replace with a new one by using
	/// a <see cref="IAsyncMapper"/>.
	/// </item>
	/// <item>
	/// If a match is not found a new element will be added by mapping the types with a <see cref="IAsyncMapper"/>
	/// by trying new map, then merge map.
	/// </item>
	/// </list>
	/// Not matched elements from the destination collection are treated according to
	/// <see cref="MergeCollectionsOptions"/> (and overrides).
	/// </para>
	/// </summary>
	/// <remarks>Collections are NOT mapped lazily, all source elements are evaluated during the map.</remarks>
	public sealed class AsyncCollectionMapper : IAsyncMapper {
		// Parallel tasks:
		// https://stackoverflow.com/a/63937542/2672235


		/// <summary>
		/// <see cref="IMapper"/> which is used to map the elements of the collections, will be also provided
		/// as a nested mapper in <see cref="AsyncMapperOverrideMappingOptions"/> (if not already present).
		/// </summary>
		private readonly AsyncCompositeMapper _elementsMapper;

		/// <summary>
		/// <see cref="IMatcher"/> which is used to match source elements with destination elements
		/// to try merging them together.
		/// </summary>
		private readonly IMatcher _elementsMatcher;

		/// <summary>
		/// Default async options.
		/// </summary>
		private readonly AsyncCollectionMappersOptions _asyncCollectionMappersOptions;

		/// <summary>
		/// Options to apply when merging elements in the collections.
		/// </summary>
		private readonly MergeCollectionsOptions _mergeCollectionOptions;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		/// <summary>
		/// Creates a new instance of <see cref="AsyncCollectionMapper"/>.
		/// </summary>
		/// <param name="elementsMapper">
		/// <see cref="IAsyncMapper"/> to use to map collection elements.<br/>
		/// Can be overridden during mapping with <see cref="AsyncMapperOverrideMappingOptions"/>.
		/// </param>
		/// <param name="asyncCollectionMappersOptions">
		/// Additional parallelization options to apply during mapping, null to use default.<br/>
		/// Can be overridden during mapping with <see cref="AsyncCollectionMappersMappingOptions"/>.
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
		public AsyncCollectionMapper(
			IAsyncMapper elementsMapper,
			AsyncCollectionMappersOptions? asyncCollectionMappersOptions = null,
			IMatcher? elementsMatcher = null,
			MergeCollectionsOptions? mergeCollectionsOptions = null) {

			_elementsMapper = new AsyncCompositeMapper(elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper)), this);
			_asyncCollectionMappersOptions = asyncCollectionMappersOptions ?? new AsyncCollectionMappersOptions();
			_elementsMatcher = elementsMatcher != null ?
				new SafeMatcher(elementsMatcher) :
				EmptyMatcher.Instance;
			_mergeCollectionOptions = mergeCollectionsOptions != null ?
				new MergeCollectionsOptions(mergeCollectionsOptions) :
				new MergeCollectionsOptions();
			var nestedMappingContext = new AsyncNestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<AsyncMapperOverrideMappingOptions, AsyncNestedMappingContext>(
				m => m?.Mapper != null ? m : new AsyncMapperOverrideMappingOptions(_elementsMapper, m?.ServiceProvider),
				n => n != null ? new AsyncNestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext, options.Cached));
		}


		#region IAsyncMapper methods
		public bool CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapAsyncNewInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
		}

		public bool CanMapAsyncMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapAsyncMergeInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
		}

		public async Task<object?> MapAsync(
			object? source,
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapAsyncNewInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper) || elementsMapper == null)
				throw new MapNotFoundException(types);

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			// At least one of New or Merge mapper is required to map elements
			IAsyncNewMapFactory elementsFactory;
			try {
				elementsFactory = elementsMapper.MapAsyncNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				try {
					elementsFactory = elementsMapper.MapAsyncMergeFactory(elementTypes.From, elementTypes.To, mappingOptions).MapAsyncNewFactory();
				}
				catch (MapNotFoundException) {
					throw new MapNotFoundException(types);
				}
			}

			using (elementsFactory) {
				if (source == null)
					return null;

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

				// Create parallel cancellation source and semaphore if needed
				var parallelMappings = mappingOptions.GetOptions<AsyncCollectionMappersMappingOptions>()?.MaxParallelMappings
					?? _asyncCollectionMappersOptions.MaxParallelMappings;

				using (var semaphore = parallelMappings > 1 ? new SemaphoreSlim(parallelMappings) : null) {
					if (types.From.IsAsyncEnumerable()) {
						var moveNextAsyncDelegate = ObjectFactory.GetAsyncEnumeratorMoveNextAsync(elementTypes.From);
						var currentDelegate = ObjectFactory.GetAsyncEnumeratorCurrent(elementTypes.From);

						if (parallelMappings > 1) {
							try {
								// Create and await all the tasks
								var tasks = new List<Task<object?>>();
								using (var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) {
									var asyncEnumerator = ObjectFactory.GetAsyncEnumerableGetAsyncEnumerator(elementTypes.From)
										.Invoke(source, cancellationSource.Token);
									try {
										while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
											var sourceElement = currentDelegate.Invoke(asyncEnumerator);

											await semaphore!.WaitAsync(cancellationSource.Token);

											tasks.Add(Task.Run(async () => {
												try {
													return await elementsFactory.Invoke(sourceElement, cancellationSource.Token);
												}
												finally {
													semaphore.Release();
												}
											}, cancellationSource.Token));
										}
									}
									finally {
										await asyncEnumerator.DisposeAsync();
									}
									try {
										await TaskUtils.WhenAllFailFast(tasks);
									}
									catch (Exception e) {
										// Cancel all the tasks
										cancellationSource.Cancel();

										if (!(e is AggregateException a) || a.InnerExceptions.Count > 1)
											throw;
										else
											throw a.InnerException!;
									}
								}

								// Add the results to the destination
								foreach (var task in tasks) {
									addDelegate.Invoke(destination, task.Result);
								}
							}
							catch (OperationCanceledException) {
								throw;
							}
							catch (Exception e) {
								throw new MappingException(e, types);
							}
						}
						else {
							var asyncEnumerator = ObjectFactory.GetAsyncEnumerableGetAsyncEnumerator(elementTypes.From)
								.Invoke(source, cancellationToken);
							try {
								while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
									var sourceElement = currentDelegate.Invoke(asyncEnumerator);
									addDelegate.Invoke(destination, await elementsFactory.Invoke(sourceElement, cancellationToken));
								}
							}
							finally {
								await asyncEnumerator.DisposeAsync();
							}
						}
					}
					else {
						if (source is IEnumerable sourceEnumerable) {
							if (parallelMappings > 1) {
								try {
									// Create and await all the tasks
									var tasks = new List<Task<object?>>();
									using (var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) {
										foreach (var sourceElement in sourceEnumerable) {
											await semaphore!.WaitAsync(cancellationSource.Token);

											tasks.Add(Task.Run(async () => {
												try {
													return await elementsFactory.Invoke(sourceElement, cancellationSource.Token);
												}
												finally {
													semaphore.Release();
												}
											}, cancellationSource.Token));
										}
										try {
											await TaskUtils.WhenAllFailFast(tasks);
										}
										catch (Exception e) {
											// Cancel all the tasks
											cancellationSource.Cancel();

											if (!(e is AggregateException a) || a.InnerExceptions.Count > 1)
												throw;
											else
												throw a.InnerException!;
										}
									}

									// Add the results to the destination
									foreach (var task in tasks) {
										addDelegate.Invoke(destination, task.Result);
									}
								}
								catch (OperationCanceledException) {
									throw;
								}
								catch (Exception e) {
									throw new MappingException(e, types);
								}
							}
							else {
								try {
									foreach (var sourceElement in sourceEnumerable) {
										addDelegate.Invoke(destination, await elementsFactory.Invoke(sourceElement, cancellationToken));
									}
								}
								catch (OperationCanceledException) {
									throw;
								}
								catch (Exception e) {
									throw new MappingException(e, types);
								}
							}
						}
						else
							throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
					}
				}

				var result = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType, types.To).Invoke(destination);

				// Should not happen
				TypeUtils.CheckObjectType(result, types.To);

				return result;
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

			if (!CanMapAsyncMergeInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper) || elementsMapper == null)
				throw new MapNotFoundException(types);

			// New mapper is required, Merge mapper is optional for mapping the elements:
			// - elements to update will use MergeMap (on the existing element),
			//   or NewMap (by removing the existing element and adding the new one)
			// - elements to add will use NewMap (or MergeMap by creating a new element and merging to it)
			IAsyncNewMapFactory newElementsFactory;
			try {
				newElementsFactory = elementsMapper.MapAsyncNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				newElementsFactory = null!;
			}

			// Need to use try/finally with newElementsFactory because it may be assigned after mergeElementsFactory
			try {
				IAsyncMergeMapFactory? mergeElementsFactory;
				try {
					mergeElementsFactory = elementsMapper.MapAsyncMergeFactory(elementTypes.From, elementTypes.To, mappingOptions);

					// newElementsFactory cannot be null, so if we have a merge factory we also create a new one from it (without disposing it)
					try {
						newElementsFactory ??= mergeElementsFactory.MapAsyncNewFactory(false);
					}
					catch {
						mergeElementsFactory.Dispose();
						throw;
					}
				}
				catch (MapNotFoundException) {
					// At least one map is required
					if (newElementsFactory == null)
						throw new MapNotFoundException(types);

					mergeElementsFactory = null;
				}

				using (mergeElementsFactory) {
					var mergeMappingOptions = mappingOptions.GetOptions<MergeCollectionsMappingOptions>();

					// Create the matcher (it will never throw because of SafeMatcher/EmptyMatcher)
					using (var elementsMatcherFactory = GetMatcher(mergeMappingOptions).MatchFactory(elementTypes.From, elementTypes.To, mappingOptions)) {
						var removeNotMatchedDestinationElements = mergeMappingOptions?.RemoveNotMatchedDestinationElements
							?? _mergeCollectionOptions.RemoveNotMatchedDestinationElements;

						TypeUtils.CheckObjectType(source, types.From, nameof(source));
						TypeUtils.CheckObjectType(destination, types.To, nameof(destination));

						if (types.From.IsAsyncEnumerable()) {
							var getAsyncEnumeratorDelegate = ObjectFactory.GetAsyncEnumerableGetAsyncEnumerator(elementTypes.From);
							var moveNextAsyncDelegate = ObjectFactory.GetAsyncEnumeratorMoveNextAsync(elementTypes.From);
							var currentDelegate = ObjectFactory.GetAsyncEnumeratorCurrent(elementTypes.From);

							if (source == null)
								return destination;
							else {
								object result;
								try {
									// If we have to create the destination collection we know that we can always map to it,
									// otherwise we check that it's not readonly
									Type? actualCollectionType;
									object? newDestination;
									if (destination == null) {
										try {
											destination = ObjectFactory.CreateCollectionFactory(types.To, out actualCollectionType).Invoke();
										}
										catch (ObjectCreationException) {
											throw new MapNotFoundException(types);
										}
										newDestination = null;
									}
									else if (TypeUtils.IsCollectionReadonly(destination)) {
										if (mergeMappingOptions?.RecreateReadonlyDestination ?? _mergeCollectionOptions.RecreateReadonlyDestination) {
											try {
												newDestination = ObjectFactory.CreateCollectionFactory(types.To, out actualCollectionType).Invoke();
											}
											catch (ObjectCreationException) {
												throw new MapNotFoundException(types);
											}
										}
										else {
											throw new InvalidOperationException("Cannot merge map to a readonly destination collection, destination type is: " +
												(destination.GetType().FullName ?? destination.GetType().Name));
										}
									}
									else {
										newDestination = null;
										actualCollectionType = null;
									}

									if (destination is IEnumerable destinationEnumerable) {
										var elementsToAdd = ObjectPool.Lists.Get();
										var elementsToRemove = ObjectPool.Lists.Get();

										// Deleted elements
										var matchedDestinations = removeNotMatchedDestinationElements ?
											ObjectPool.Lists.Get() :
											null;

										try { 
											// Added/updated elements
											var asyncEnumerator = getAsyncEnumeratorDelegate.Invoke(source, cancellationToken);
											try {
												while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
													var sourceElement = currentDelegate.Invoke(asyncEnumerator);

													bool found = false;
													object? matchingDestinationElement = null;
													foreach (var destinationElement in destinationEnumerable.Cast<object>().Concat(elementsToAdd)) {
														if (elementsMatcherFactory.Invoke(sourceElement, destinationElement) &&
															!elementsToRemove.Contains(destinationElement)) {

															matchingDestinationElement = destinationElement;
															matchedDestinations?.Add(matchingDestinationElement);
															found = true;
															break;
														}
													}

													if (found) {
														// MergeMap or NewMap
														if (mergeElementsFactory != null) {
															var mergeResult = await mergeElementsFactory.Invoke(sourceElement, matchingDestinationElement, cancellationToken);
															if (mergeResult != matchingDestinationElement) {
																elementsToRemove.Add(matchingDestinationElement);
																elementsToAdd.Add(mergeResult);
															}
														}
														else {
															elementsToRemove.Add(matchingDestinationElement);
															elementsToAdd.Add(await newElementsFactory.Invoke(sourceElement, cancellationToken));
														}
													}
													else
														elementsToAdd.Add(await newElementsFactory.Invoke(sourceElement, cancellationToken));
												}
											}
											finally {
												await asyncEnumerator.DisposeAsync();
											}

											// Deleted elements
											if (removeNotMatchedDestinationElements)
												elementsToRemove.AddRange(destinationEnumerable.Cast<object>().Except(matchedDestinations!));

											if (newDestination != null) {
												var addDelegate = ObjectFactory.GetCollectionCustomAddDelegate(actualCollectionType ?? types.To);

												// Fill new destination collection
												foreach (var element in destinationEnumerable.Cast<object?>().Except(elementsToRemove)) {
													addDelegate.Invoke(newDestination, element);
												}
												foreach (var element in elementsToAdd) {
													addDelegate.Invoke(newDestination, element);
												}
												destination = newDestination;
											}
											else {
												// Do not throw since we are dealing with ICollection<T>
												var addDelegate = ObjectFactory.GetCollectionAddDelegate(elementTypes.To);
												var removeDelegate = ObjectFactory.GetCollectionRemoveDelegate(elementTypes.To);

												// Update destination collection
												foreach (var element in elementsToAdd) {
													addDelegate.Invoke(destination, element);
												}
												foreach (var element in elementsToRemove) {
													if (!removeDelegate.Invoke(destination, element))
														throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
												}
											}
										}
										finally {
											ObjectPool.Lists.Return(elementsToAdd);
											ObjectPool.Lists.Return(elementsToRemove);
											if (matchedDestinations != null)
												ObjectPool.Lists.Return(matchedDestinations);
										}

										if (actualCollectionType != null)
											result = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType, types.To).Invoke(destination);
										else
											result = destination;
									}
									else
										throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
								}
								catch (OperationCanceledException) {
									throw;
								}
								catch (Exception e) {
									throw new MappingException(e, types);
								}

								// Should not happen
								TypeUtils.CheckObjectType(result, types.To);

								return result;
							}
						}
						else {
							if (source is IEnumerable sourceEnumerable) {
								object result;
								try {
									// If we have to create the destination collection we know that we can always map to it,
									// otherwise we check that it's not readonly
									Type? actualCollectionType;
									object? newDestination;
									if (destination == null) {
										try {
											destination = ObjectFactory.CreateCollectionFactory(types.To, out actualCollectionType).Invoke();
										}
										catch (ObjectCreationException) {
											throw new MapNotFoundException(types);
										}
										newDestination = null;
									}
									else if (TypeUtils.IsCollectionReadonly(destination)) {
										if (mergeMappingOptions?.RecreateReadonlyDestination ?? _mergeCollectionOptions.RecreateReadonlyDestination) {
											try {
												newDestination = ObjectFactory.CreateCollectionFactory(types.To, out actualCollectionType).Invoke();
											}
											catch (ObjectCreationException) {
												throw new MapNotFoundException(types);
											}
										}
										else { 
											throw new InvalidOperationException("Cannot merge map to a readonly destination collection, destination type is: " +
												(destination.GetType().FullName ?? destination.GetType().Name));
										}
									}
									else { 
										newDestination = null;
										actualCollectionType = null;
									}

									if (destination is IEnumerable destinationEnumerable) {
										var elementsToAdd = ObjectPool.Lists.Get();
										var elementsToRemove = ObjectPool.Lists.Get();

										// Deleted elements
										var matchedDestinations = removeNotMatchedDestinationElements ?
											ObjectPool.Lists.Get() :
											null;

										try { 
											// Added/updated elements
											foreach (var sourceElement in sourceEnumerable) {
												bool found = false;
												object? matchingDestinationElement = null;
												foreach (var destinationElement in destinationEnumerable.Cast<object>().Concat(elementsToAdd)) {
													if (elementsMatcherFactory.Invoke(sourceElement, destinationElement) &&
														!elementsToRemove.Contains(destinationElement)) {

														matchingDestinationElement = destinationElement;
														matchedDestinations?.Add(matchingDestinationElement);
														found = true;
														break;
													}
												}

												if (found) {
													// MergeMap or NewMap
													if (mergeElementsFactory != null) {
														var mergeResult = await mergeElementsFactory.Invoke(sourceElement, matchingDestinationElement, cancellationToken);
														if (mergeResult != matchingDestinationElement) {
															elementsToRemove.Add(matchingDestinationElement);
															elementsToAdd.Add(mergeResult);
														}
													}
													else {
														elementsToRemove.Add(matchingDestinationElement);
														elementsToAdd.Add(await newElementsFactory.Invoke(sourceElement, cancellationToken));
													}
												}
												else
													elementsToAdd.Add(await newElementsFactory.Invoke(sourceElement, cancellationToken));
											}

											// Deleted elements
											if (removeNotMatchedDestinationElements)
												elementsToRemove.AddRange(destinationEnumerable.Cast<object>().Except(matchedDestinations!));

											if(newDestination != null) {
												var addDelegate = ObjectFactory.GetCollectionCustomAddDelegate(actualCollectionType ?? types.To);

												// Fill new destination collection
												foreach (var element in destinationEnumerable.Cast<object?>().Except(elementsToRemove)) {
													addDelegate.Invoke(newDestination, element);
												}
												foreach (var element in elementsToAdd) {
													addDelegate.Invoke(newDestination, element);
												}
												destination = newDestination;
											}
											else {
												// Do not throw since we are dealing with ICollection<T>
												var addDelegate = ObjectFactory.GetCollectionAddDelegate(elementTypes.To);
												var removeDelegate = ObjectFactory.GetCollectionRemoveDelegate(elementTypes.To);

												// Update destination collection
												foreach (var element in elementsToAdd) {
													addDelegate.Invoke(destination, element);
												}
												foreach (var element in elementsToRemove) {
													if (!removeDelegate.Invoke(destination, element))
														throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
												}
											}
										}
										finally {
											ObjectPool.Lists.Return(elementsToAdd);
											ObjectPool.Lists.Return(elementsToRemove);
											if (matchedDestinations != null)
												ObjectPool.Lists.Return(matchedDestinations);
										}

										if (actualCollectionType != null)
											result = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType, types.To).Invoke(destination);
										else
											result = destination;
									}
									else
										throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
								}
								catch (OperationCanceledException) {
									throw;
								}
								catch (Exception e) {
									throw new MappingException(e, types);
								}

								// Should not happen
								TypeUtils.CheckObjectType(result, types.To);

								return result;
							}
							else if (source == null)
								return destination;
							else
								throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
						}
					}
				}
			}
			finally {
				newElementsFactory?.Dispose();
			}

			throw new MapNotFoundException(types);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region IAsyncMapperFactory methods
		public IAsyncNewMapFactory MapAsyncNewFactory(
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapAsyncNewInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper) || elementsMapper == null)
				throw new MapNotFoundException(types);

			// At least one of New or Merge mapper is required to map elements
			IAsyncNewMapFactory elementsFactory;
			try {
				elementsFactory = elementsMapper.MapAsyncNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				try {
					elementsFactory = elementsMapper.MapAsyncMergeFactory(elementTypes.From, elementTypes.To, mappingOptions).MapAsyncNewFactory();
				}
				catch (MapNotFoundException) {
					throw new MapNotFoundException(types);
				}
			}

			try {
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

				// Create parallel cancellation source and semaphore if needed
				var parallelMappings = mappingOptions.GetOptions<AsyncCollectionMappersMappingOptions>()?.MaxParallelMappings
					?? _asyncCollectionMappersOptions.MaxParallelMappings;

				if (types.From.IsAsyncEnumerable()) {
					var getAsyncEnumeratorDelegate = ObjectFactory.GetAsyncEnumerableGetAsyncEnumerator(elementTypes.From);
					var moveNextAsyncDelegate = ObjectFactory.GetAsyncEnumeratorMoveNextAsync(elementTypes.From);
					var currentDelegate = ObjectFactory.GetAsyncEnumeratorCurrent(elementTypes.From);

					if (parallelMappings > 1) {
						var semaphore = new SemaphoreSlim(parallelMappings);

						try {
							return new DisposableAsyncNewMapFactory(
								sourceType, destinationType,
								async (source, cancellationToken) => {
									TypeUtils.CheckObjectType(source, types.From, nameof(source));

									if (source == null)
										return null;
									else {
										object result;
										try {
											// Create and await all the tasks
											var tasks = new List<Task<object?>>();
											using (var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) {
												var asyncEnumerator = getAsyncEnumeratorDelegate.Invoke(source, cancellationSource.Token);
												try {
													while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
														var sourceElement = currentDelegate.Invoke(asyncEnumerator);

														await semaphore.WaitAsync(cancellationSource.Token);

														tasks.Add(Task.Run(async () => {
															try {
																return await elementsFactory.Invoke(sourceElement, cancellationSource.Token);
															}
															finally {
																semaphore.Release();
															}
														}, cancellationSource.Token));
													}
												}
												finally {
													await asyncEnumerator.DisposeAsync();
												}
												try {
													await TaskUtils.WhenAllFailFast(tasks);
												}
												catch (Exception e) {
													// Cancel all the tasks
													cancellationSource.Cancel();

													if (!(e is AggregateException a) || a.InnerExceptions.Count > 1)
														throw;
													else
														throw a.InnerException!;
												}
											}

											var destination = collectionFactory.Invoke();

											// Add the results to the destination
											foreach (var task in tasks) {
												addDelegate.Invoke(destination, task.Result);
											}

											result = collectionConversionDelegate.Invoke(destination);
										}
										catch (OperationCanceledException) {
											throw;
										}
										catch (Exception e) {
											throw new MappingException(e, types);
										}

										// Should not happen
										TypeUtils.CheckObjectType(result, types.To);

										return result;
									}
								},
								elementsFactory, semaphore);
						}
						catch {
							semaphore.Dispose();
							throw;
						}
					}
					else {
						return new DisposableAsyncNewMapFactory(
							sourceType, destinationType,
							async (source, cancellationToken) => {
								TypeUtils.CheckObjectType(source, types.From, nameof(source));

								if (source == null)
									return null;
								else {
									object result;
									try {
										var destination = collectionFactory.Invoke();

										var asyncEnumerator = getAsyncEnumeratorDelegate.Invoke(source, cancellationToken);
										try {
											while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
												var sourceElement = currentDelegate.Invoke(asyncEnumerator);
												addDelegate.Invoke(destination, await elementsFactory.Invoke(sourceElement, cancellationToken));
											}
										}
										finally {
											await asyncEnumerator.DisposeAsync();
										}

										result = collectionConversionDelegate.Invoke(destination);
									}
									catch (OperationCanceledException) {
										throw;
									}
									catch (Exception e) {
										throw new MappingException(e, types);
									}

									// Should not happen
									TypeUtils.CheckObjectType(result, types.To);

									return result;
								}
							},
							elementsFactory);
					}
				}
				else {
					if (parallelMappings > 1) {
						var semaphore = new SemaphoreSlim(parallelMappings);

						try {
							return new DisposableAsyncNewMapFactory(
								sourceType, destinationType,
								async (source, cancellationToken) => {
									TypeUtils.CheckObjectType(source, types.From, nameof(source));

									if (source is IEnumerable sourceEnumerable) {
										object result;
										try {
											// Create and await all the tasks
											var tasks = new List<Task<object?>>();
											using (var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) {
												foreach (var sourceElement in sourceEnumerable) {
													await semaphore.WaitAsync(cancellationSource.Token);

													tasks.Add(Task.Run(async () => {
														try {
															return await elementsFactory.Invoke(sourceElement, cancellationSource.Token);
														}
														finally {
															semaphore.Release();
														}
													}, cancellationSource.Token));
												}
												try {
													await TaskUtils.WhenAllFailFast(tasks);
												}
												catch (Exception e) {
													// Cancel all the tasks
													cancellationSource.Cancel();

													if (!(e is AggregateException a) || a.InnerExceptions.Count > 1)
														throw;
													else
														throw a.InnerException!;
												}
											}

											var destination = collectionFactory.Invoke();

											// Add the results to the destination
											foreach (var task in tasks) {
												addDelegate.Invoke(destination, task.Result);
											}

											result = collectionConversionDelegate.Invoke(destination);
										}
										catch (OperationCanceledException) {
											throw;
										}
										catch (Exception e) {
											throw new MappingException(e, types);
										}

										// Should not happen
										TypeUtils.CheckObjectType(result, types.To);

										return result;
									}
									else if (source == null)
										return null;
									else
										throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
								},
								elementsFactory, semaphore);
						}
						catch {
							semaphore.Dispose();
							throw;
						}
					}
					else {
						return new DisposableAsyncNewMapFactory(
							sourceType, destinationType,
							async (source, cancellationToken) => {
								TypeUtils.CheckObjectType(source, types.From, nameof(source));

								if (source is IEnumerable sourceEnumerable) {
									object result;
									try {
										var destination = collectionFactory.Invoke();

										foreach (var sourceElement in sourceEnumerable) {
											addDelegate.Invoke(destination, await elementsFactory.Invoke(sourceElement, cancellationToken));
										}

										result = collectionConversionDelegate.Invoke(destination);
									}
									catch (OperationCanceledException) {
										throw;
									}
									catch (Exception e) {
										throw new MappingException(e, types);
									}

									// Should not happen
									TypeUtils.CheckObjectType(result, types.To);

									return result;
								}
								else if (source == null)
									return null;
								else
									throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
							},
							elementsFactory);
					}
				}
			}
			catch {
				elementsFactory?.Dispose();
				throw;
			}

			throw new MapNotFoundException(types);
		}

		public IAsyncMergeMapFactory MapAsyncMergeFactory(
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapAsyncMergeInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper) || elementsMapper == null)
				throw new MapNotFoundException(types);

			// New mapper is required, Merge mapper is optional for mapping the elements:
			// - elements to update will use MergeMap (on the existing element),
			//   or NewMap (by removing the existing element and adding the new one)
			// - elements to add will use NewMap (or MergeMap by creating a new element and merging to it)
			IAsyncNewMapFactory newElementsFactory;
			try {
				newElementsFactory = elementsMapper.MapAsyncNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				newElementsFactory = null!;
			}

			try {
				IAsyncMergeMapFactory? mergeElementsFactory;
				try {
					mergeElementsFactory = elementsMapper.MapAsyncMergeFactory(elementTypes.From, elementTypes.To, mappingOptions);

					// newElementsFactory cannot be null, so if we have a merge factory we also create a new one from it (without disposing it)
					try {
						newElementsFactory ??= mergeElementsFactory.MapAsyncNewFactory(false);
					}
					catch {
						mergeElementsFactory.Dispose();
						throw;
					}
				}
				catch (MapNotFoundException) {
					// At least one map is required
					if (newElementsFactory == null)
						throw new MapNotFoundException(types);

					mergeElementsFactory = null;
				}

				try {
					var mergeMappingOptions = mappingOptions.GetOptions<MergeCollectionsMappingOptions>();

					// Create the matcher (it will never throw because of SafeMatcher/EmptyMatcher)
					var elementsMatcherFactory = GetMatcher(mergeMappingOptions).MatchFactory(elementTypes.From, elementTypes.To, mappingOptions);

					try {
						var removeNotMatchedDestinationElements = mergeMappingOptions?.RemoveNotMatchedDestinationElements
							?? _mergeCollectionOptions.RemoveNotMatchedDestinationElements;

						// Create the collection factories (in case we will map null destination collections)
						Func<object> collectionFactory;
						Type actualCollectionType;
						try {
							collectionFactory = ObjectFactory.CreateCollectionFactory(types.To, out actualCollectionType);
						}
						catch (ObjectCreationException) {
							throw new MapNotFoundException(types);
						}

						// Do not throw since we are dealing with ICollection<T>
						var addDelegate = ObjectFactory.GetCollectionAddDelegate(elementTypes.To);
						var removeDelegate = ObjectFactory.GetCollectionRemoveDelegate(elementTypes.To);

						// Used in case we create a new collection
						var collectionConversionDelegate = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType ?? types.To, types.To);

						if (types.From.IsAsyncEnumerable()) {
							var getAsyncEnumeratorDelegate = ObjectFactory.GetAsyncEnumerableGetAsyncEnumerator(elementTypes.From);
							var moveNextAsyncDelegate = ObjectFactory.GetAsyncEnumeratorMoveNextAsync(elementTypes.From);
							var currentDelegate = ObjectFactory.GetAsyncEnumeratorCurrent(elementTypes.From);

							return new DisposableAsyncMergeMapFactory(
								sourceType, destinationType,
								async (source, destination, cancellationToken) => {
									TypeUtils.CheckObjectType(source, types.From, nameof(source));
									TypeUtils.CheckObjectType(destination, types.To, nameof(destination));

									if (source == null)
										return destination;
									else {
										object result;
										try {
											// If we have to create the destination collection we know that we can always map to it,
											// otherwise we check that it's not readonly
											if (destination == null)
												destination = collectionFactory.Invoke();
											else if (TypeUtils.IsCollectionReadonly(destination)) {
												throw new InvalidOperationException("Cannot merge map to a readonly destination collection, destination type is: " +
													(destination.GetType().FullName ?? destination.GetType().Name));
											}

											if (destination is IEnumerable destinationEnumerable) {
												var elementsToAdd = ObjectPool.Lists.Get();
												var elementsToRemove = ObjectPool.Lists.Get();

												// Deleted elements
												var matchedDestinations = removeNotMatchedDestinationElements ?
													ObjectPool.Lists.Get() :
													null;

												try { 
													// Added/updated elements
													var asyncEnumerator = getAsyncEnumeratorDelegate.Invoke(source, cancellationToken);
													try {
														while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
															var sourceElement = currentDelegate.Invoke(asyncEnumerator);

															bool found = false;
															object? matchingDestinationElement = null;
															foreach (var destinationElement in destinationEnumerable.Cast<object>().Concat(elementsToAdd)) {
																if (elementsMatcherFactory.Invoke(sourceElement, destinationElement) &&
																	!elementsToRemove.Contains(destinationElement)) {

																	matchingDestinationElement = destinationElement;
																	matchedDestinations?.Add(matchingDestinationElement);
																	found = true;
																	break;
																}
															}

															if (found) {
																// MergeMap or NewMap
																if (mergeElementsFactory != null) {
																	var mergeResult = await mergeElementsFactory.Invoke(sourceElement, matchingDestinationElement, cancellationToken);
																	if (mergeResult != matchingDestinationElement) {
																		elementsToRemove.Add(matchingDestinationElement);
																		elementsToAdd.Add(mergeResult);
																	}
																}
																else {
																	elementsToRemove.Add(matchingDestinationElement);
																	elementsToAdd.Add(await newElementsFactory.Invoke(sourceElement, cancellationToken));
																}
															}
															else
																elementsToAdd.Add(await newElementsFactory.Invoke(sourceElement, cancellationToken));
														}
													}
													finally {
														await asyncEnumerator.DisposeAsync();
													}

													// Deleted elements
													if (removeNotMatchedDestinationElements)
														elementsToRemove.AddRange(destinationEnumerable.Cast<object>().Except(matchedDestinations!));

													// Update destination collection
													foreach (var element in elementsToAdd) {
														addDelegate.Invoke(destination, element);
													}
													foreach (var element in elementsToRemove) {
														if (!removeDelegate.Invoke(destination, element))
															throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
													}
												}
												finally {
													ObjectPool.Lists.Return(elementsToAdd);
													ObjectPool.Lists.Return(elementsToRemove);
													if (matchedDestinations != null)
														ObjectPool.Lists.Return(matchedDestinations);
												}

												result = collectionConversionDelegate.Invoke(destination);
											}
											else
												throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
										}
										catch (OperationCanceledException) {
											throw;
										}
										catch (Exception e) {
											throw new MappingException(e, types);
										}

										// Should not happen
										TypeUtils.CheckObjectType(result, types.To);

										return result;
									}
								},
								newElementsFactory, mergeElementsFactory, elementsMatcherFactory);
						}
						else {
							return new DisposableAsyncMergeMapFactory(
								sourceType, destinationType,
								async (source, destination, cancellationToken) => {
									TypeUtils.CheckObjectType(source, types.From, nameof(source));
									TypeUtils.CheckObjectType(destination, types.To, nameof(destination));

									if (source is IEnumerable sourceEnumerable) {
										object result;
										try {
											// If we have to create the destination collection we know that we can always map to it,
											// otherwise we check that it's not readonly
											if (destination == null)
												destination = collectionFactory.Invoke();
											else if (TypeUtils.IsCollectionReadonly(destination)) {
												throw new InvalidOperationException("Cannot merge map to a readonly destination collection, destination type is: " +
													(destination.GetType().FullName ?? destination.GetType().Name));
											}

											if (destination is IEnumerable destinationEnumerable) {
												var elementsToAdd = ObjectPool.Lists.Get();
												var elementsToRemove = ObjectPool.Lists.Get();

												// Deleted elements
												var matchedDestinations = removeNotMatchedDestinationElements ?
													ObjectPool.Lists.Get() :
													null;

												try { 
													// Added/updated elements
													foreach (var sourceElement in sourceEnumerable) {
														bool found = false;
														object? matchingDestinationElement = null;
														foreach (var destinationElement in destinationEnumerable.Cast<object>().Concat(elementsToAdd)) {
															if (elementsMatcherFactory.Invoke(sourceElement, destinationElement) &&
																!elementsToRemove.Contains(destinationElement)) {

																matchingDestinationElement = destinationElement;
																matchedDestinations?.Add(matchingDestinationElement);
																found = true;
																break;
															}
														}

														if (found) {
															// MergeMap or NewMap
															if (mergeElementsFactory != null) {
																var mergeResult = await mergeElementsFactory.Invoke(sourceElement, matchingDestinationElement, cancellationToken);
																if (mergeResult != matchingDestinationElement) {
																	elementsToRemove.Add(matchingDestinationElement);
																	elementsToAdd.Add(mergeResult);
																}
															}
															else {
																elementsToRemove.Add(matchingDestinationElement);
																elementsToAdd.Add(await newElementsFactory.Invoke(sourceElement, cancellationToken));
															}
														}
														else
															elementsToAdd.Add(await newElementsFactory.Invoke(sourceElement, cancellationToken));
													}

													// Deleted elements
													if (removeNotMatchedDestinationElements)
														elementsToRemove.AddRange(destinationEnumerable.Cast<object>().Except(matchedDestinations!));

													// Update destination collection
													foreach (var element in elementsToAdd) {
														addDelegate.Invoke(destination, element);
													}
													foreach (var element in elementsToRemove) {
														if (!removeDelegate.Invoke(destination, element))
															throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
													}
												}
												finally {
													ObjectPool.Lists.Return(elementsToAdd);
													ObjectPool.Lists.Return(elementsToRemove);
													if (matchedDestinations != null)
														ObjectPool.Lists.Return(matchedDestinations);
												}

												result = collectionConversionDelegate.Invoke(destination);
											}
											else
												throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
										}
										catch (OperationCanceledException) {
											throw;
										}
										catch (Exception e) {
											throw new MappingException(e, types);
										}

										// Should not happen
										TypeUtils.CheckObjectType(result, types.To);

										return result;
									}
									else if (source == null)
										return destination;
									else
										throw new InvalidOperationException("Source is not an enumerable"); // Should not happen

								},
								newElementsFactory, mergeElementsFactory, elementsMatcherFactory);
						}
					}
					catch {
						elementsMatcherFactory?.Dispose();
						throw;
					}
				}
				catch {
					mergeElementsFactory?.Dispose();
					throw;
				}
			}
			catch {
				newElementsFactory?.Dispose();
				throw;
			}

			throw new MapNotFoundException(types);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion


		private bool CanMapAsyncNewInternal(
			Type sourceType,
			Type destinationType,
			[NotNullWhen(true)] ref MappingOptions? mappingOptions,
			out (Type From, Type To) elementTypes,
			out IAsyncMapper elementsMapper) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if ((sourceType.IsEnumerable() || sourceType.IsAsyncEnumerable()) &&
				(destinationType.IsEnumerable() || destinationType.IsAsyncEnumerable()) &&
				ObjectFactory.CanCreateCollection(destinationType)) {

				if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition) {
					elementTypes = default;
					elementsMapper = null!;
					mappingOptions = null!;

					return true;
				}
				else { 
					elementTypes = (From: sourceType.IsEnumerable() ? sourceType.GetEnumerableElementType() : sourceType.GetAsyncEnumerableElementType(),
						To: destinationType.IsEnumerable() ? destinationType.GetEnumerableElementType() : destinationType.GetAsyncEnumerableElementType());
					mappingOptions = _optionsCache.GetOrCreate(mappingOptions);
					elementsMapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

					return elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions) ||
						(ObjectFactory.CanCreate(elementTypes.To) && elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions));
				}
			}
			else {
				elementTypes = default;
				elementsMapper = null!;

				return false;
			}
		}

		private bool CanMapAsyncMergeInternal(
			Type sourceType,
			Type destinationType,
			[NotNullWhen(true)] ref MappingOptions? mappingOptions,
			out (Type From, Type To) elementTypes,
			out IAsyncMapper elementsMapper) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			var recreate = mappingOptions?.GetOptions<MergeCollectionsMappingOptions>()?.RecreateReadonlyDestination
				?? _mergeCollectionOptions.RecreateReadonlyDestination;

			if ((sourceType.IsEnumerable() || sourceType.IsAsyncEnumerable()) &&
				((destinationType.IsCollection() && !destinationType.IsArray) ||
					(recreate && (destinationType.IsEnumerable() || destinationType.IsAsyncEnumerable()))) &&
				ObjectFactory.CanCreateCollection(destinationType)) {

				// If we are not recreating, check if the destination type is not readonly
				if (!recreate && TypeUtils.IsCollectionReadonly(destinationType)) {
					elementTypes = default;
					elementsMapper = null!;

					return false;
				}

				if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition) {
					elementTypes = default;
					elementsMapper = null!;
					mappingOptions = null!;

					return true;
				}
				else {
					elementTypes = (
						sourceType.IsEnumerable() ? sourceType.GetEnumerableElementType() : sourceType.GetAsyncEnumerableElementType(),
						recreate ? (destinationType.IsEnumerable() ? destinationType.GetEnumerableElementType() : destinationType.GetAsyncEnumerableElementType()) : destinationType.GetCollectionElementType());
					mappingOptions = _optionsCache.GetOrCreate(mappingOptions);
					elementsMapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

					return elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions) ||
						(ObjectFactory.CanCreate(elementTypes.To) && elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions));
				}
			}
			else {
				elementTypes = default;
				elementsMapper = null!;

				return false;
			}
		}

		private IMatcher GetMatcher(MergeCollectionsMappingOptions? mergeMappingOptions) {
			if (mergeMappingOptions?.Matcher != null && mergeMappingOptions.Matcher != _elementsMatcher) {
				// Creating a CompositeMatcher because the provided matcher just overrides any maps in _elementsMatcher
				// so all the others should be available
				var options = new CompositeMatcherOptions();
				options.Matchers.Add(mergeMappingOptions.Matcher);
				options.Matchers.Add(_elementsMatcher);
				return new SafeMatcher(new CompositeMatcher(options));
			}
			else
				return _elementsMatcher;
		}
	}
}
