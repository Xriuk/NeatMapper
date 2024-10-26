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
	/// (even nested) with an existing <see cref="ICollection{T}"/> (not readonly) asynchronously
	/// (even in parallel, check below for details),
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
	public sealed class AsyncMergeCollectionMapper : AsyncCollectionMapper, IAsyncMapperFactory {
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
			MergeCollectionsOptions?
#else
			MergeCollectionsOptions
#endif
			mergeCollectionsOptions = null) :
				base(elementsMapper) {

			_elementsMatcher = elementsMatcher != null ?
				new SafeMatcher(elementsMatcher) :
				EmptyMatcher.Instance;
			_mergeCollectionOptions = mergeCollectionsOptions != null ?
				new MergeCollectionsOptions(mergeCollectionsOptions) :
				new MergeCollectionsOptions();
		}


		#region IAsyncMapper methods
		override public bool CanMapAsyncNew(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return false;
		}

		override public bool CanMapAsyncMerge(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return CanMapAsyncMergeInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
		}

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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapAsyncMergeInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper))
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
				newElementsFactory = null;
			}

			// Need to use try/finally with newElementsFactory because it may be assigned after mergeElementsFactory
			try {
				IAsyncMergeMapFactory mergeElementsFactory;
				try {
					mergeElementsFactory = elementsMapper.MapAsyncMergeFactory(elementTypes.From, elementTypes.To, mappingOptions);

					// newElementsFactory cannot be null, so if we have a merge factory we also create a new one from it (without disposing it)
					try {
						if (newElementsFactory == null)
							newElementsFactory = mergeElementsFactory.MapAsyncNewFactory(false);
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
					IMatcher elementsMatcher;
					if (mergeMappingOptions?.Matcher != null && mergeMappingOptions.Matcher != _elementsMatcher) {
						// Creating a CompositeMatcher because the provided matcher just overrides any maps in _elementsMatcher
						// so all the others should be available
						var options = new CompositeMatcherOptions();
						options.Matchers.Add(mergeMappingOptions.Matcher);
						options.Matchers.Add(_elementsMatcher);
						elementsMatcher = new SafeMatcher(new CompositeMatcher(options));
					}
					else
						elementsMatcher = _elementsMatcher;

					using(var elementsMatcherFactory = elementsMatcher.MatchFactory(elementTypes.From, elementTypes.To, mappingOptions)) {
						var removeNotMatchedDestinationElements = mergeMappingOptions?.RemoveNotMatchedDestinationElements
							?? _mergeCollectionOptions.RemoveNotMatchedDestinationElements;

						// Do not throw since we are dealing with ICollection<T>
						var addDelegate = ObjectFactory.GetCollectionAddDelegate(elementTypes.To);
						var removeDelegate = ObjectFactory.GetCollectionRemoveDelegate(elementTypes.To);

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
									Type actualCollectionType;
									if (destination == null) {
										try {
											destination = ObjectFactory.CreateCollectionFactory(types.To, out actualCollectionType).Invoke();
										}
										catch (ObjectCreationException) {
											throw new MapNotFoundException(types);
										}
									}
									else {
										if (TypeUtils.IsCollectionReadonly(destination)) {
											throw new InvalidOperationException("Cannot merge map to a readonly destination collection, destination type is: " +
												(destination.GetType().FullName ?? destination.GetType().Name));
										}
										actualCollectionType = null;
									}

									if (destination is IEnumerable destinationEnumerable) {
										// DEV: use pool?
										var elementsToAdd = new List<object>();
										var elementsToRemove = new List<object>();

										// Deleted elements
										var matchedDestinations = removeNotMatchedDestinationElements ?
											new List<object>() :
											null;

										// Added/updated elements
										var asyncEnumerator = getAsyncEnumeratorDelegate.Invoke(source, cancellationToken);
										try {
											while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
												var sourceElement = currentDelegate.Invoke(asyncEnumerator);

												bool found = false;
												object matchingDestinationElement = null;
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
											elementsToRemove.AddRange(destinationEnumerable.Cast<object>().Except(matchedDestinations));

										// Update destination collection
										foreach (var element in elementsToAdd) {
											addDelegate.Invoke(destination, element);
										}
										foreach (var element in elementsToRemove) {
											if (!removeDelegate.Invoke(destination, element))
												throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
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
									Type actualCollectionType;
									if (destination == null) {
										try {
											destination = ObjectFactory.CreateCollectionFactory(types.To, out actualCollectionType).Invoke();
										}
										catch (ObjectCreationException) {
											throw new MapNotFoundException(types);
										}
									}
									else {
										if (TypeUtils.IsCollectionReadonly(destination)) {
											throw new InvalidOperationException("Cannot merge map to a readonly destination collection, destination type is: " +
												(destination.GetType().FullName ?? destination.GetType().Name));
										}
										actualCollectionType = null;
									}

									if (destination is IEnumerable destinationEnumerable) {
										// DEV: use pool?
										var elementsToAdd = new List<object>();
										var elementsToRemove = new List<object>();

										// Deleted elements
										var matchedDestinations = removeNotMatchedDestinationElements ?
											new List<object>() :
											null;

										// Added/updated elements
										foreach (var sourceElement in sourceEnumerable) {
											bool found = false;
											object matchingDestinationElement = null;
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
											elementsToRemove.AddRange(destinationEnumerable.Cast<object>().Except(matchedDestinations));

										// Update destination collection
										foreach (var element in elementsToAdd) {
											addDelegate.Invoke(destination, element);
										}
										foreach (var element in elementsToRemove) {
											if (!removeDelegate.Invoke(destination, element))
												throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
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

			if (!CanMapAsyncMergeInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper))
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
				newElementsFactory = null;
			}

			try {
				IAsyncMergeMapFactory mergeElementsFactory;
				try {
					mergeElementsFactory = elementsMapper.MapAsyncMergeFactory(elementTypes.From, elementTypes.To, mappingOptions);

					// newElementsFactory cannot be null, so if we have a merge factory we also create a new one from it (without disposing it)
					try {
						if (newElementsFactory == null)
							newElementsFactory = mergeElementsFactory.MapAsyncNewFactory(false);
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
					IMatcher elementsMatcher;
					if (mergeMappingOptions?.Matcher != null && mergeMappingOptions.Matcher != _elementsMatcher) {
						// Creating a CompositeMatcher because the provided matcher just overrides any maps in _elementsMatcher
						// so all the others should be available
						var options = new CompositeMatcherOptions();
						options.Matchers.Add(mergeMappingOptions.Matcher);
						options.Matchers.Add(_elementsMatcher);
						elementsMatcher = new SafeMatcher(new CompositeMatcher(options));
					}
					else
						elementsMatcher = _elementsMatcher;
					var elementsMatcherFactory = elementsMatcher.MatchFactory(elementTypes.From, elementTypes.To, mappingOptions);

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
												// DEV: use pool?
												var elementsToAdd = new List<object>();
												var elementsToRemove = new List<object>();

												// Deleted elements
												var matchedDestinations = removeNotMatchedDestinationElements ?
													new List<object>() :
													null;

												// Added/updated elements
												var asyncEnumerator = getAsyncEnumeratorDelegate.Invoke(source, cancellationToken);
												try {
													while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
														var sourceElement = currentDelegate.Invoke(asyncEnumerator);

														bool found = false;
														object matchingDestinationElement = null;
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
													elementsToRemove.AddRange(destinationEnumerable.Cast<object>().Except(matchedDestinations));

												// Update destination collection
												foreach (var element in elementsToAdd) {
													addDelegate.Invoke(destination, element);
												}
												foreach (var element in elementsToRemove) {
													if (!removeDelegate.Invoke(destination, element))
														throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
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
												// DEV: use pool?
												var elementsToAdd = new List<object>();
												var elementsToRemove = new List<object>();

												// Deleted elements
												var matchedDestinations = removeNotMatchedDestinationElements ?
													new List<object>() :
													null;

												// Added/updated elements
												foreach (var sourceElement in sourceEnumerable) {
													bool found = false;
													object matchingDestinationElement = null;
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
													elementsToRemove.AddRange(destinationEnumerable.Cast<object>().Except(matchedDestinations));

												// Update destination collection
												foreach (var element in elementsToAdd) {
													addDelegate.Invoke(destination, element);
												}
												foreach (var element in elementsToRemove) {
													if (!removeDelegate.Invoke(destination, element))
														throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
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


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private bool CanMapAsyncMergeInternal(
			Type sourceType,
			Type destinationType,
			ref MappingOptions mappingOptions,
			out (Type From, Type To) elementTypes,
			out IAsyncMapper elementsMapper) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if ((sourceType.IsEnumerable() || sourceType.IsAsyncEnumerable()) &&
				destinationType.IsCollection() && !destinationType.IsArray &&
				ObjectFactory.CanCreateCollection(destinationType)) {

				// If the destination type is not an interface, check if it is not readonly
				if (!destinationType.IsInterface && destinationType.IsGenericType) {
					var collectionDefinition = destinationType.GetGenericTypeDefinition();
					if (collectionDefinition == typeof(ReadOnlyCollection<>) ||
						collectionDefinition == typeof(ReadOnlyDictionary<,>) ||
						collectionDefinition == typeof(ReadOnlyObservableCollection<>)) {

						elementTypes = default;
						elementsMapper = null;

						return false;
					}
				}

				elementTypes = (From: sourceType.IsEnumerable() ? sourceType.GetEnumerableElementType() : sourceType.GetAsyncEnumerableElementType(),
					To: destinationType.GetCollectionElementType());
				mappingOptions = _optionsCache.GetOrCreate(mappingOptions);
				elementsMapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

				return elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions) ||
					(ObjectFactory.CanCreate(elementTypes.To) && elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions));
			}
			else {
				elementTypes = default;
				elementsMapper = null;

				return false;
			}
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
