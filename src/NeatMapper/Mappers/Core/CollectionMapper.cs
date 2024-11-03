using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which maps collections by using another <see cref="IMapper"/> to map elements.
	/// <para>
	/// For new maps creates a new <see cref="IEnumerable{T}"/> (derived from <see cref="ICollection{T}"/>
	/// plus some special types like below), even nested and readonly, from another <see cref="IEnumerable{T}"/>.<br/>
	/// Elements are then mapped with another <see cref="IMapper"/> by trying new map first, then merge map.<br/>
	/// Special collections which can be created are:
	/// <list type="bullet">
	/// <item><see cref="Stack{T}"/></item>
	/// <item><see cref="Queue{T}"/></item>
	/// <item><see cref="string"/> (considered as a collection of <see cref="char"/>s)</item>
	/// </list>
	/// </para>
	/// <para>
	/// For merge maps merges a <see cref="IEnumerable{T}"/> (even nested) with an existing
	/// <see cref="ICollection{T}"/> (not readonly), will create a new <see cref="ICollection{T}"/>
	/// if destination is null.<br/>
	/// Will try to match elements of the source collection with the destination by using an
	/// <see cref="IMatcher"/> if provided:
	/// <list type="bullet">
	/// <item>
	/// If a match is found will try to merge the two elements or will replace with a new one by using
	/// a <see cref="IMapper"/>.
	/// </item>
	/// <item>
	/// If a match is not found a new element will be added by mapping the types with a <see cref="IMapper"/>
	/// by trying new map, then merge map.
	/// </item>
	/// </list>
	/// Not matched elements from the destination collection are treated according to
	/// <see cref="MergeCollectionsOptions"/> (and overrides).
	/// </para>
	/// </summary>
	/// <remarks>Collections are NOT mapped lazily, all source elements are evaluated during the map.</remarks>
	public sealed class CollectionMapper : IMapper, IMapperFactory {
		/// <summary>
		/// <see cref="IMapper"/> which is used to map the elements of the collections, will be also provided
		/// as a nested mapper in <see cref="MapperOverrideMappingOptions"/> (if not already present).
		/// </summary>
		private readonly IMapper _elementsMapper;

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
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		public CollectionMapper(
			IMapper elementsMapper,
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

			_elementsMapper = new CompositeMapper(elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper)), this);
			_elementsMatcher = elementsMatcher != null ? new SafeMatcher(elementsMatcher) : EmptyMatcher.Instance;
			_mergeCollectionOptions = mergeCollectionsOptions != null ? new MergeCollectionsOptions(mergeCollectionsOptions) : new MergeCollectionsOptions();
			var nestedMappingContext = new NestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<MapperOverrideMappingOptions, NestedMappingContext>(
				m => m?.Mapper != null ? m : new MapperOverrideMappingOptions(_elementsMapper, m?.ServiceProvider),
				n => n != null ? new NestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext, options.Cached));
		}


		#region IMapper methods
		public bool CanMapNew(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return CanMapNewInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
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

			return CanMapMergeInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
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

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapNewInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper))
				throw new MapNotFoundException(types);

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			if (source is IEnumerable sourceEnumerable) {
				// At least one of New or Merge mapper is required to map elements
				INewMapFactory elementsFactory;
				try {
					elementsFactory = elementsMapper.MapNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
				}
				catch (MapNotFoundException) {
					try {
						elementsFactory = elementsMapper.MapMergeFactory(elementTypes.From, elementTypes.To, mappingOptions).MapNewFactory();
					}
					catch (MapNotFoundException) {
						throw new MapNotFoundException(types);
					}
				}

				using (elementsFactory) {
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
					Action<object, object> addDelegate;
					try {
						addDelegate = ObjectFactory.GetCollectionCustomAddDelegate(actualCollectionType);
					}
					catch (InvalidOperationException) {
						throw new MapNotFoundException(types);
					}

					try {
						foreach (var sourceElement in sourceEnumerable) {
							addDelegate.Invoke(destination, elementsFactory.Invoke(sourceElement));
						}
					}
					catch (OperationCanceledException) {
						throw;
					}
					catch (Exception e) {
						throw new MappingException(e, types);
					}

					var result = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType, types.To).Invoke(destination);

					// Should not happen
					TypeUtils.CheckObjectType(result, types.To);

					return result;
				}
			}
			else if (source == null)
				return null;
			else
				throw new InvalidOperationException("Source is not an enumerable"); // Should not happen

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

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapMergeInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper))
				throw new MapNotFoundException(types);

			// New mapper is required, Merge mapper is optional for mapping the elements:
			// - elements to update will use MergeMap (on the existing element),
			//   or NewMap (by removing the existing element and adding the new one)
			// - elements to add will use NewMap (or MergeMap by creating a new element and merging to it)
			INewMapFactory newElementsFactory;
			try {
				newElementsFactory = elementsMapper.MapNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				newElementsFactory = null;
			}

			// Need to use try/finally with newElementsFactory because it may be assigned after mergeElementsFactory
			try {
				IMergeMapFactory mergeElementsFactory;
				try {
					mergeElementsFactory = elementsMapper.MapMergeFactory(elementTypes.From, elementTypes.To, mappingOptions);

					// newElementsFactory cannot be null, so if we have a merge factory we also create a new one from it (without disposing it)
					try {
						if (newElementsFactory == null)
							newElementsFactory = mergeElementsFactory.MapNewFactory(false);
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

						// Do not throw since we are dealing with ICollection<T>
						var addDelegate = ObjectFactory.GetCollectionAddDelegate(elementTypes.To);
						var removeDelegate = ObjectFactory.GetCollectionRemoveDelegate(elementTypes.To);

						TypeUtils.CheckObjectType(source, types.From, nameof(source));
						TypeUtils.CheckObjectType(destination, types.To, nameof(destination));

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
													var mergeResult = mergeElementsFactory.Invoke(sourceElement, matchingDestinationElement);
													if (mergeResult != matchingDestinationElement) {
														elementsToRemove.Add(matchingDestinationElement);
														elementsToAdd.Add(mergeResult);
													}
												}
												else {
													elementsToRemove.Add(matchingDestinationElement);
													elementsToAdd.Add(newElementsFactory.Invoke(sourceElement));
												}
											}
											else
												elementsToAdd.Add(newElementsFactory.Invoke(sourceElement));
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
									}
									finally {
										ObjectPool.Lists.Return(elementsToAdd);
										ObjectPool.Lists.Return(elementsToRemove);
										if(matchedDestinations != null)
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
			finally {
				newElementsFactory?.Dispose();
			}

			throw new MapNotFoundException(types);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapNewInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper))
				throw new MapNotFoundException(types);

			// At least one of New or Merge mapper is required to map elements
			INewMapFactory elementsFactory;
			try {
				elementsFactory = elementsMapper.MapNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				try {
					elementsFactory = elementsMapper.MapMergeFactory(elementTypes.From, elementTypes.To, mappingOptions).MapNewFactory();
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
				Action<object, object> addDelegate;
				try {
					addDelegate = ObjectFactory.GetCollectionCustomAddDelegate(actualCollectionType);
				}
				catch (InvalidOperationException) {
					throw new MapNotFoundException(types);
				}

				var collectionConversionDelegate = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType, types.To);

				return new DisposableNewMapFactory(
					sourceType, destinationType,
					source => {
						TypeUtils.CheckObjectType(source, types.From, nameof(source));

						if (source is IEnumerable sourceEnumerable) {
							object result;
							try {
								var destination = collectionFactory.Invoke();

								foreach (var sourceElement in sourceEnumerable) {
									addDelegate.Invoke(destination, elementsFactory.Invoke(sourceElement));
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
			catch {
				elementsFactory?.Dispose();
				throw;
			}

			throw new MapNotFoundException(types);

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

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapMergeInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper))
				throw new MapNotFoundException(types);

			// New mapper is required, Merge mapper is optional for mapping the elements:
			// - elements to update will use MergeMap (on the existing element),
			//   or NewMap (by removing the existing element and adding the new one)
			// - elements to add will use NewMap (or MergeMap by creating a new element and merging to it)
			INewMapFactory newElementsFactory;
			try {
				newElementsFactory = elementsMapper.MapNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				newElementsFactory = null;
			}

			try {
				IMergeMapFactory mergeElementsFactory;
				try {
					mergeElementsFactory = elementsMapper.MapMergeFactory(elementTypes.From, elementTypes.To, mappingOptions);

					// newElementsFactory cannot be null, so if we have a merge factory we also create a new one from it (without disposing it)
					try {
						if (newElementsFactory == null)
							newElementsFactory = mergeElementsFactory.MapNewFactory(false);
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

						return new DisposableMergeMapFactory(
							sourceType, destinationType,
							(source, destination) => {
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
															var mergeResult = mergeElementsFactory.Invoke(sourceElement, matchingDestinationElement);
															if (mergeResult != matchingDestinationElement) {
																elementsToRemove.Add(matchingDestinationElement);
																elementsToAdd.Add(mergeResult);
															}
														}
														else {
															elementsToRemove.Add(matchingDestinationElement);
															elementsToAdd.Add(newElementsFactory.Invoke(sourceElement));
														}
													}
													else
														elementsToAdd.Add(newElementsFactory.Invoke(sourceElement));
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

		private bool CanMapNewInternal(
			Type sourceType,
			Type destinationType,
			ref MappingOptions mappingOptions,
			out (Type From, Type To) elementTypes,
			out IMapper elementsMapper) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsEnumerable() && destinationType.IsEnumerable() &&
				ObjectFactory.CanCreateCollection(destinationType)) {

				elementTypes = (sourceType.GetEnumerableElementType(), destinationType.GetEnumerableElementType());
				mappingOptions = _optionsCache.GetOrCreate(mappingOptions);
				elementsMapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

				return elementsMapper.CanMapNew(elementTypes.From, elementTypes.To, mappingOptions) ||
					(ObjectFactory.CanCreate(elementTypes.To) && elementsMapper.CanMapMerge(elementTypes.From, elementTypes.To, mappingOptions));
			}
			else {
				elementTypes = default;
				elementsMapper = null;

				return false;
			}
		}

		private bool CanMapMergeInternal(
			Type sourceType,
			Type destinationType,
			ref MappingOptions mappingOptions,
			out (Type From, Type To) elementTypes,
			out IMapper elementsMapper) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsEnumerable() && destinationType.IsCollection() && !destinationType.IsArray &&
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

				elementTypes = (sourceType.GetEnumerableElementType(), destinationType.GetCollectionElementType());
				mappingOptions = _optionsCache.GetOrCreate(mappingOptions);
				elementsMapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

				return elementsMapper.CanMapNew(elementTypes.From, elementTypes.To, mappingOptions) ||
					(ObjectFactory.CanCreate(elementTypes.To) && elementsMapper.CanMapMerge(elementTypes.From, elementTypes.To, mappingOptions));
			}
			else {
				elementTypes = default;
				elementsMapper = null;

				return false;
			}
		}

		private IMatcher GetMatcher(MergeCollectionsMappingOptions mergeMappingOptions) {
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
