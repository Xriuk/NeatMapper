using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which merges a <see cref="IEnumerable{T}"/> or <see cref="IAsyncEnumerable{T}"/>
	/// (even nested) with an existing <see cref="ICollection{T}"/> (not readonly) asynchronously,
	/// will create a new <see cref="ICollection{T}"/> if destination is null.<br/>
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
	/// </summary>
	/// <remarks>Collections are NOT mapped lazily, all source elements are evaluated during the map.</remarks>
	public sealed class AsyncMergeCollectionMapper : AsyncCollectionMapper, IAsyncMapperCanMap, IAsyncMapperFactory {
		// DEV: what is it used for? Try to remove. Maybe used to check the original mapper capabilities for nested maps, to exclude collections
		private readonly IAsyncMapper _originalElementMapper;

		/// <summary>
		/// <see cref="IMatcher"/> which is used to match source elements with destination elements
		/// to try merging them together.
		/// </summary>
		private readonly IMatcher _elementsMatcher;

		/// <summary>
		/// Options to apply when merging elements in the collections.
		/// </summary>
		private readonly MergeCollectionsOptions _mergeCollectionOptions;


		/// <summary>
		/// Creates a new instance of <see cref="AsyncMergeCollectionMapper"/>.
		/// </summary>
		/// <param name="elementsMapper">
		/// <see cref="IAsyncMapper"/> to use to map collection elements.<br/>
		/// Can be overridden during mapping with <see cref="AsyncMapperOverrideMappingOptions.Mapper"/>.
		/// </param>
		/// <param name="elementsMatcher">
		/// <see cref="IMatcher"/> used to match elements between collections to merge them,
		/// if null the elements won't be matched (this will effectively be the same as using
		/// <see cref="AsyncNewCollectionMapper"/>).<br/>
		/// Can be overridden during mapping with <see cref="MergeCollectionsMappingOptions.Matcher"/>.
		/// </param>
		/// <param name="asyncCollectionMappersOptions">
		/// Additional parallelization options to apply during mapping, null to use default.<br/>
		/// Can be overridden during mapping with <see cref="AsyncCollectionMappersMappingOptions"/>.</param>
		/// <param name="mergeCollectionsOptions">
		/// Additional merging options to apply during mapping, null to use default.<br/>
		/// Can be overridden during mapping with <see cref="MergeCollectionsMappingOptions"/>.
		/// </param>
		public AsyncMergeCollectionMapper(
			IAsyncMapper elementsMapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IMatcher?
#else
			IMatcher
#endif
			elementsMatcher = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			AsyncCollectionMappersOptions?
#else
			AsyncCollectionMappersOptions
#endif
			asyncCollectionMappersOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MergeCollectionsOptions?
#else
			MergeCollectionsOptions
#endif
			mergeCollectionsOptions = null) :
				base(elementsMapper, asyncCollectionMappersOptions != null ? new AsyncCollectionMappersOptions(asyncCollectionMappersOptions) : null) {

			_originalElementMapper = elementsMapper;
			_elementsMatcher = elementsMatcher != null ?
				new SafeMatcher(elementsMatcher) :
				EmptyMatcher.Instance;
			_mergeCollectionOptions = mergeCollectionsOptions != null ?
				new MergeCollectionsOptions(mergeCollectionsOptions) :
				new MergeCollectionsOptions();
		}


		#region IAsyncMapper methods
		override public Task<
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

			// Not mapping new
			throw new MapNotFoundException((sourceType, destinationType));
		}

		override public async Task<
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

			using (var factory = MapAsyncMergeFactory(sourceType, destinationType, mappingOptions)) {
				return await factory.Invoke(source, destination, cancellationToken);
			}
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

			return Task.FromResult(false);
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

			return CanMapMerge(sourceType, destinationType, null, mappingOptions, cancellationToken);
		}
		#endregion

		#region IAsyncMapperFactory methods
		public IAsyncNewMapFactory MapAsyncNewFactory(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			// Not mapping new
			throw new MapNotFoundException((sourceType, destinationType));
		}

		public IAsyncMergeMapFactory MapAsyncMergeFactory(
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

			(Type From, Type To) types = (sourceType, destinationType);

			// If both types are collections try mapping the element types
			if ((types.From.IsEnumerable() || types.From.IsAsyncEnumerable()) &&
				types.To.IsCollection() && !types.To.IsArray) {

				// If the destination type is not an interface, check if it is not readonly
				if (!destinationType.IsInterface && destinationType.IsGenericType) {
					var collectionDefinition = destinationType.GetGenericTypeDefinition();
					if (collectionDefinition == typeof(ReadOnlyCollection<>) ||
						collectionDefinition == typeof(ReadOnlyDictionary<,>) ||
						collectionDefinition == typeof(ReadOnlyObservableCollection<>)) {

						throw new MapNotFoundException(types);
					}
				}

				var elementTypes = (From: types.From.IsEnumerable() ? types.From.GetEnumerableElementType() : types.From.GetAsyncEnumerableElementType(),
					To: types.To.GetCollectionElementType());

				// Create the collection factories (in case we will map null destination collections)
				Func<object> collectionFactory;
				Type actualCollectionType;
				try {
					collectionFactory = ObjectFactory.CreateCollectionFactory(types.To, out actualCollectionType);
				}
				catch (ObjectCreationException) {
					collectionFactory = null;
					actualCollectionType = null;
				}

				// Retrieve ICollection<T> delegates for the destination
				var addDelegate = ObjectFactory.GetCollectionAddDelegate(elementTypes.To);
				var removeDelegate = ObjectFactory.GetCollectionRemoveDelegate(elementTypes.To);

				// Used in case we create a new collection
				var collectionConversionDelegate = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType ?? types.To, types.To);

				mappingOptions = _optionsCache.GetOrCreate(mappingOptions);
				var mergeMappingOptions = mappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				var elementsMapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

				// At least one of New or Merge mapper is required to map elements
				// If both are found they will be used in the following order:
				// - elements to update will use MergeMap first (on the existing element),
				//   then NewMap (by removing the existing element and adding the new one)
				// - elements to add will use NewMap first,
				//   then MergeMap (by creating a new element and merging to it)
				IAsyncNewMapFactory newElementsFactory;
				try {
					newElementsFactory = elementsMapper.MapAsyncNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
				}
				catch (MapNotFoundException) {
					newElementsFactory = null;
				}

				try {
					IAsyncMergeMapFactory mergeElementsFactory;
					Func<object> destinationFactory = null;
					try {
						mergeElementsFactory = elementsMapper.MapAsyncMergeFactory(elementTypes.From, elementTypes.To, mappingOptions);
						try {
							destinationFactory = ObjectFactory.CreateFactory(elementTypes.To);
						}
						catch (ObjectCreationException) { }
					}
					catch (MapNotFoundException) {
						// At least one map is required
						if (newElementsFactory == null)
							throw new MapNotFoundException(types);

						mergeElementsFactory = null;
					}

					try {
						// Create the matcher (it will never throw because of SafeMatcher)
						IMatcher elementsMatcher;
						if (mergeMappingOptions?.Matcher != null) {
							// Creating a CompositeMatcher because the provided matcher just overrides any maps in _elementsMatcher
							// so all the others should be available
							var options = new CompositeMatcherOptions();
							options.Matchers.Add(mergeMappingOptions.Matcher);
							options.Matchers.Add(_elementsMatcher);
							elementsMatcher = new CompositeMatcher(options);
						}
						else
							elementsMatcher = _elementsMatcher;
						var elementsMatcherFactory = elementsMatcher.MatchFactory(elementTypes.From, elementTypes.To, mappingOptions);

						try {
							var removeNotMatchedDestinationElements = mergeMappingOptions?.RemoveNotMatchedDestinationElements
								?? _mergeCollectionOptions.RemoveNotMatchedDestinationElements;

							if (types.From.IsAsyncEnumerable()) {
								var getAsyncEnumeratorDelegate = ObjectFactory.GetAsyncEnumerableGetAsyncEnumerator(elementTypes.From);
								var moveNextAsyncDelegate = ObjectFactory.GetAsyncEnumeratorMoveNextAsync(elementTypes.From);

								return new DisposableAsyncMergeMapFactory(
									sourceType, destinationType,
									async (source, destination, cancellationToken) => {
										TypeUtils.CheckObjectType(source, types.From, nameof(source));
										TypeUtils.CheckObjectType(destination, types.To, nameof(destination));

										if (source == null) {
											// DEV: maybe remove because we already know that we could map the types,
											// find edge-cases + tests
											try {
												if (await elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken))
													return null;
											}
											catch { }

											try {
												if (await elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken) &&
													ObjectFactory.CanCreate(elementTypes.To)) {

													return null;
												}
											}
											catch { }

											throw new MapNotFoundException(types);
										}
										else {
											// If we have to create the destination collection we know that we can always map to it
											// Otherwise we must check that first
											if (destination == null) {
												if (collectionFactory == null)
													throw new MapNotFoundException(types);

												destination = collectionFactory.Invoke();
											}
											else {
												// Check if the collection is not readonly recursively, if it throws it means that
												// the element mapper will be responsible for mapping the object and not collection mapper recursively
												try {
													if (!await CanMapMerge(sourceType, destinationType, destination as IEnumerable, mappingOptions, cancellationToken))
														throw new MapNotFoundException(types);
												}
												catch (MapNotFoundException) {
													throw;
												}
												catch { }
											}

											if (destination is IEnumerable destinationEnumerable) {
												var elementsToRemove = new List<object>();
												var elementsToAdd = new List<object>();

												var canNew = newElementsFactory != null;
												var canMerge = mergeElementsFactory != null;

												object result;
												try {
													var matchedDestinations = removeNotMatchedDestinationElements ?
														new List<object>() :
														null;

													// Added/updated elements
													var asyncEnumerator = getAsyncEnumeratorDelegate.Invoke(source, cancellationToken);
													try {
														while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
															var sourceElement = asyncEnumerator.GetType().GetProperty(nameof(IAsyncEnumerator<object>.Current))
																.GetValue(asyncEnumerator);

															// Cannot use FirstOrDefault because there might be matching null elements
															bool found = false;
															object matchingDestinationElement = null;
															foreach (var destinationElement in destinationEnumerable) {
																if (elementsMatcherFactory.Invoke(sourceElement, destinationElement)) {
																	matchingDestinationElement = destinationElement;
																	matchedDestinations?.Add(matchingDestinationElement);
																	found = true;
																	break;
																}
															}

															if (found) {
																// Try merge map
																if (canMerge) {
																	try {
																		var mergeResult = await mergeElementsFactory.Invoke(sourceElement, matchingDestinationElement, cancellationToken);
																		if (mergeResult != matchingDestinationElement) {
																			elementsToRemove.Add(matchingDestinationElement);
																			elementsToAdd.Add(mergeResult);
																		}
																		continue;
																	}
																	catch (MapNotFoundException) {
																		canMerge = false;
																	}
																}

																// Try new map
																if (!canNew)
																	throw new MapNotFoundException(types);

																try {
																	elementsToRemove.Add(matchingDestinationElement);
																	elementsToAdd.Add(await newElementsFactory.Invoke(sourceElement, cancellationToken));
																}
																catch (MapNotFoundException) {
																	throw new MapNotFoundException(types);
																}
															}
															else {
																// Try new map
																if (canNew) {
																	try {
																		elementsToAdd.Add(await newElementsFactory.Invoke(sourceElement, cancellationToken));
																		continue;
																	}
																	catch (MapNotFoundException) {
																		canNew = false;
																	}
																}

																// Try merge map
																if (!canMerge || destinationFactory == null)
																	throw new MapNotFoundException(types);

																try {
																	elementsToAdd.Add(await mergeElementsFactory.Invoke(sourceElement, destinationFactory.Invoke(), cancellationToken));
																}
																catch (MapNotFoundException) {
																	throw new MapNotFoundException(types);
																}
															}
														}
													}
													finally {
														await asyncEnumerator.DisposeAsync();
													}

													// Deleted elements
													if (removeNotMatchedDestinationElements) 
														elementsToRemove.AddRange(destinationEnumerable.Cast<object>().Except(matchedDestinations));

													// Update destination collection
													foreach (var element in elementsToRemove) {
														if (!removeDelegate.Invoke(destination, element))
															throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
													}
													foreach (var element in elementsToAdd) {
														addDelegate.Invoke(destination, element);
													}

													result = collectionConversionDelegate.Invoke(destination);
												}
												catch (MapNotFoundException) {
													throw;
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
											else
												throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
										}
									},
									newElementsFactory, mergeElementsFactory, elementsMatcherFactory);
							}
							else {
								// Create parallel cancellation source and semaphore if needed
								var parallelMappings = mappingOptions.GetOptions<AsyncCollectionMappersMappingOptions>()?.MaxParallelMappings
									?? _asyncCollectionMappersOptions.MaxParallelMappings;

								if(parallelMappings > 1) {
									var semaphore = new SemaphoreSlim(parallelMappings);

									try {
										return new DisposableAsyncMergeMapFactory(
										sourceType, destinationType,
										async (source, destination, cancellationToken) => {
											TypeUtils.CheckObjectType(source, types.From, nameof(source));
											TypeUtils.CheckObjectType(destination, types.To, nameof(destination));

											if (source is IEnumerable sourceEnumerable) {
												// If we have to create the destination collection we know that we can always map to it
												// Otherwise we must check that first
												if (destination == null) {
													if (collectionFactory == null)
														throw new MapNotFoundException(types);

													destination = collectionFactory.Invoke();
												}
												else {
													// Check if the collection is not readonly recursively, if it throws it means that
													// the element mapper will be responsible for mapping the object and not collection mapper recursively
													try {
														if (!await CanMapMerge(sourceType, destinationType, destination as IEnumerable, mappingOptions, cancellationToken))
															throw new MapNotFoundException(types);
													}
													catch (MapNotFoundException) {
														throw;
													}
													catch { }
												}

												if (destination is IEnumerable destinationEnumerable) {
													var elementsToRemove = new List<object>();
													var elementsToAdd = new List<object>();

													var canNew = newElementsFactory != null ? 1 : 0;
													var canMerge = mergeElementsFactory != null ? 1 : 0;

													object result;
													try {
														// Deleted elements
														var matchedDestinations = removeNotMatchedDestinationElements ?
															new List<object>() :
															null;

														// Added/updated elements
														var sourceDestinationMatches = sourceEnumerable
															.Cast<object>()
															.Select(sourceElement => {
																// Cannot use FirstOrDefault/First because we need to throw exceptions from matcher
																bool found = false;
																object matchingDestinationElement = null;
																foreach(var destinationElement in destinationEnumerable) {
																	if(elementsMatcherFactory.Invoke(sourceElement, destinationElement)) {
																		matchingDestinationElement = destinationElement;
																		matchedDestinations?.Add(matchingDestinationElement);
																		found = true;
																		break;
																	}
																}

																return (SourceElement: sourceElement, Found: found, MatchingDestinationElement: matchingDestinationElement);
															});

														// Create all the tasks, we group by found destination element because
														// multiple source elements could match with the same destination element
														using (var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) {
															var tasks = sourceDestinationMatches
																.Select(sourceMapping => {
																	var resultTaskFactory = new Lazy<Task<object>>(() => Task.Run<object>(async () => {
																		await semaphore.WaitAsync(cancellationSource.Token);
																		try {
																			if (sourceMapping.Found) {
																				// Try merge map
																				if (Interlocked.CompareExchange(ref canMerge, 0, 0) == 1) {
																					try {
																						return await mergeElementsFactory.Invoke(sourceMapping.SourceElement, sourceMapping.MatchingDestinationElement, cancellationSource.Token);
																					}
																					catch (MapNotFoundException) {
																						Interlocked.CompareExchange(ref canMerge, 0, 1);
																					}
																				}

																				// Try new map
																				if (Interlocked.CompareExchange(ref canNew, 0, 0) == 0)
																					throw new MapNotFoundException(types);
																				try {
																					return await newElementsFactory.Invoke(sourceMapping.SourceElement, cancellationSource.Token);
																				}
																				catch (MapNotFoundException) {
																					throw new MapNotFoundException(types);
																				}
																			}
																			else {
																				// Try new map
																				if (Interlocked.CompareExchange(ref canNew, 0, 0) == 1) {
																					try {
																						return await newElementsFactory.Invoke(sourceMapping.SourceElement, cancellationSource.Token);
																					}
																					catch (MapNotFoundException) {
																						Interlocked.CompareExchange(ref canNew, 0, 1);
																					}
																				}

																				// Try merge map
																				if (destinationFactory == null || Interlocked.CompareExchange(ref canMerge, 0, 0) == 0)
																					throw new MapNotFoundException(types);
																				try {
																					return await mergeElementsFactory.Invoke(sourceMapping.SourceElement, destinationFactory.Invoke(), cancellationSource.Token);
																				}
																				catch (MapNotFoundException) {
																					throw new MapNotFoundException(types);
																				}
																			}
																		}
																		finally {
																			semaphore.Release();
																		}
																	}, cancellationSource.Token));
																	return (sourceMapping.Found, sourceMapping.MatchingDestinationElement, Result: resultTaskFactory);
																})
																.ToArray();

															// This groups found elements together by the destination, while not found are kept individually
															var groupedTasks = tasks
																.GroupBy(task => (task.Found, task.MatchingDestinationElement))
																.SelectMany(sourceGroup => sourceGroup.Key.Found ?
																	(IEnumerable<IEnumerable<(bool Found, object MatchingDestinationElement, Lazy<Task<object>> Result)>>)new[] { sourceGroup } :
																	sourceGroup.Select(e => new[] { e }))
																.Select(sourceGroup => sourceGroup.ToArray())
																.ToArray();
															var nestedTasks = groupedTasks
																.Select(innerTasks => Task.Run<object>(async () => {
																	foreach (var innerTask in innerTasks) {
																		await innerTask.Result.Value;
																	}
																	return null;
																}, cancellationToken))
																.ToArray();

															// Await all the tasks
															try {
																await TaskUtils.WhenAllFailFast(nestedTasks);
															}
															catch {
																// Cancel all the tasks
																cancellationSource.Cancel();

																throw;
															}

															// Process results
															foreach (var (found, matchingDestinationElement, resultTask) in tasks) {
																var destinationElement = resultTask.Value.Result;
																if (found) {
																	if (matchingDestinationElement != destinationElement) {
																		elementsToRemove.Add(matchingDestinationElement);
																		elementsToAdd.Add(destinationElement);
																	}
																}
																else
																	elementsToAdd.Add(destinationElement);
															}
														}

														// Deleted elements
														if (removeNotMatchedDestinationElements)
															elementsToRemove.AddRange(destinationEnumerable.Cast<object>().Except(matchedDestinations));

														// Update destination collection
														foreach (var element in elementsToRemove) {
															if (!removeDelegate.Invoke(destination, element))
																throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
														}
														foreach (var element in elementsToAdd) {
															addDelegate.Invoke(destination, element);
														}

														result = collectionConversionDelegate.Invoke(destination);
													}
													catch (MapNotFoundException) {
														throw;
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
												else
													throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
											}
											else if (source == null) {
												// DEV: maybe remove because we already know that we could map the types,
												// find edge-cases + tests
												try {
													if (await elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken))
														return null;
												}
												catch { }

												try {
													if (await elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken) &&
														ObjectFactory.CanCreate(elementTypes.To)) {

														return null;
													}
												}
												catch { }

												throw new MapNotFoundException(types);
											}
											else
												throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
										},
										semaphore, newElementsFactory, mergeElementsFactory, elementsMatcherFactory);
									}
									catch {
										semaphore?.Dispose();
										throw;
									}
								}
								else {
									return new DisposableAsyncMergeMapFactory(
										sourceType, destinationType,
										async (source, destination, cancellationToken) => {
											TypeUtils.CheckObjectType(source, types.From, nameof(source));
											TypeUtils.CheckObjectType(destination, types.To, nameof(destination));

											if (source is IEnumerable sourceEnumerable) {
												// If we have to create the destination collection we know that we can always map to it
												// Otherwise we must check that first
												if (destination == null) {
													if (collectionFactory == null)
														throw new MapNotFoundException(types);

													destination = collectionFactory.Invoke();
												}
												else {
													// Check if the collection is not readonly recursively, if it throws it means that
													// the element mapper will be responsible for mapping the object and not collection mapper recursively
													try {
														if (!await CanMapMerge(sourceType, destinationType, destination as IEnumerable, mappingOptions, cancellationToken))
															throw new MapNotFoundException(types);
													}
													catch (MapNotFoundException) {
														throw;
													}
													catch { }
												}

												if (destination is IEnumerable destinationEnumerable) {
													var elementsToRemove = new List<object>();
													var elementsToAdd = new List<object>();

													var canNew = newElementsFactory != null;
													var canMerge = mergeElementsFactory != null;

													object result;
													try {
														// Deleted elements
														var matchedDestinations = removeNotMatchedDestinationElements ?
															new List<object>() :
															null;

														// Added/updated elements
														foreach (var sourceElement in sourceEnumerable) {
															// Cannot use FirstOrDefault because there might be matching null elements
															bool found = false;
															object matchingDestinationElement = null;
															foreach (var destinationElement in destinationEnumerable) {
																if (elementsMatcherFactory.Invoke(sourceElement, destinationElement)) {
																	matchingDestinationElement = destinationElement;
																	matchedDestinations?.Add(matchingDestinationElement);
																	found = true;
																	break;
																}
															}

															if (found) {
																// Try merge map
																if (canMerge) {
																	try {
																		var mergeResult = await mergeElementsFactory.Invoke(sourceElement, matchingDestinationElement, cancellationToken);
																		if (mergeResult != matchingDestinationElement) {
																			elementsToRemove.Add(matchingDestinationElement);
																			elementsToAdd.Add(mergeResult);
																		}
																		continue;
																	}
																	catch (MapNotFoundException) {
																		canMerge = false;
																	}
																}

																// Try new map
																if (!canNew)
																	throw new MapNotFoundException(types);

																try {
																	elementsToRemove.Add(matchingDestinationElement);
																	elementsToAdd.Add(await newElementsFactory.Invoke(sourceElement, cancellationToken));
																}
																catch (MapNotFoundException) {
																	throw new MapNotFoundException(types);
																}
															}
															else {
																// Try new map
																if (canNew) {
																	try {
																		elementsToAdd.Add(await newElementsFactory.Invoke(sourceElement, cancellationToken));
																		continue;
																	}
																	catch (MapNotFoundException) {
																		canNew = false;
																	}
																}

																// Try merge map
																if (!canMerge || destinationFactory == null)
																	throw new MapNotFoundException(types);

																try {
																	elementsToAdd.Add(await mergeElementsFactory.Invoke(sourceElement, destinationFactory.Invoke(), cancellationToken));
																}
																catch (MapNotFoundException) {
																	throw new MapNotFoundException(types);
																}
															}
														}

														// Deleted elements
														if (removeNotMatchedDestinationElements)
															elementsToRemove.AddRange(destinationEnumerable.Cast<object>().Except(matchedDestinations));

														// Update destination collection
														foreach (var element in elementsToRemove) {
															if (!removeDelegate.Invoke(destination, element))
																throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
														}
														foreach (var element in elementsToAdd) {
															addDelegate.Invoke(destination, element);
														}

														result = collectionConversionDelegate.Invoke(destination);
													}
													catch (MapNotFoundException) {
														throw;
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
												else
													throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
											}
											else if (source == null) {
												// DEV: maybe remove because we already know that we could map the types,
												// find edge-cases + tests
												try {
													if (await elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken))
														return null;
												}
												catch { }

												try {
													if (await elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken) &&
														ObjectFactory.CanCreate(elementTypes.To)) {

														return null;
													}
												}
												catch { }

												throw new MapNotFoundException(types);
											}
											else
												throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
										},
										newElementsFactory, mergeElementsFactory, elementsMatcherFactory);
								}
							}
						}
						catch {
							elementsMatcherFactory.Dispose();
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
			}

			throw new MapNotFoundException(types);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		async Task<bool> CanMapMerge(
			Type sourceType,
			Type destinationType,
			IEnumerable destination = null,
			MappingOptions mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if ((sourceType.IsEnumerable() || sourceType.IsAsyncEnumerable()) &&
				destinationType.IsCollection() && !destinationType.IsArray) {

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
				else if (destination != null && TypeUtils.IsCollectionReadonly(destination))
					return false;

				var elementTypes = (From: sourceType.IsEnumerable() ? sourceType.GetEnumerableElementType() : sourceType.GetAsyncEnumerableElementType(),
					To: destinationType.GetCollectionElementType());

				mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

				var elementsMapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper ?? _originalElementMapper;

				// If the original element mapper can map the types on its own we succeed
				bool? canMapNew;
				try {
					canMapNew = await elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken);
				}
				catch (InvalidOperationException) {
					canMapNew = null;
				}
				bool? canMapMerge;
				try {
					canMapMerge = await elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken);

					// If we can only merge map we must be able to create a destination too, in order to map new elements
					if (canMapMerge == true && canMapNew != true && !ObjectFactory.CanCreate(elementTypes.To))
						canMapMerge = false;
				}
				catch (InvalidOperationException) {
					canMapMerge = null;
				}

				// Otherwise we try to check if we can map the types recursively
				bool? canMapNested;
				try {
					canMapNested = await CanMapMerge(elementTypes.From, elementTypes.To, null, mappingOptions, cancellationToken);
				}
				catch (InvalidOperationException) {
					canMapNested = null;
				}

				// If we have a concrete class we already checked that it's not readonly
				// Otherwise if we have a destination check if all its elements can be mapped
				if ((canMapNew == true || canMapMerge == true || canMapNested == true) && !destinationType.IsInterface)
					return true;
				else if(canMapNested == null && destination != null){
					foreach (var element in destination) {
						if (TypeUtils.IsCollectionReadonly(element))
							return false;
					}

					return true;
				}
				else if(canMapNew == false && canMapMerge == false && canMapNested == false)
					return false;
			}
			else
				return false;

			throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
