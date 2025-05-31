using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
	/// If <see cref="MergeCollectionsOptions.RecreateReadonlyDestination"/> (or overrides) is
	/// <see langword="true"/>, destination collections can also be <see cref="IEnumerable{T}"/>
	/// (also readonly), just like new maps.<br/>
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
		private readonly CompositeMapper _elementsMapper;

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


		/// <summary>
		/// Creates a new instance of <see cref="CollectionMapper"/>.
		/// </summary>
		/// <param name="elementsMapper">
		/// <see cref="IMapper"/> to use to map collection elements.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions"/>.
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
		public CollectionMapper(
			IMapper elementsMapper,
			IMatcher? elementsMatcher = null,
			MergeCollectionsOptions? mergeCollectionsOptions = null) {

			_elementsMapper = new CompositeMapper(elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper)), this);
			_elementsMatcher = elementsMatcher != null ? new SafeMatcher(elementsMatcher) : EmptyMatcher.Instance;
			_mergeCollectionOptions = mergeCollectionsOptions != null ? new MergeCollectionsOptions(mergeCollectionsOptions) : new MergeCollectionsOptions();
			var nestedMappingContext = new NestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<MapperOverrideMappingOptions, NestedMappingContext>(
				m => m?.Mapper != null ? m : new MapperOverrideMappingOptions(_elementsMapper, m?.ServiceProvider),
				n => n != null ? new NestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext, options.Cached));
		}


		#region IMapper methods
		public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapNewInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
		}

		public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapMergeInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
		}

		public object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapNewInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper) || elementsMapper == null)
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
					Action<object, object?> addDelegate;
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
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapMergeInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper) || elementsMapper == null)
				throw new MapNotFoundException(types);

			TypeUtils.CheckObjectType(source, types.From, nameof(source));
			TypeUtils.CheckObjectType(destination, types.To, nameof(destination));

			// New mapper is required, Merge mapper is optional for mapping the elements:
			// - elements to update will use MergeMap (on the existing element),
			//   or NewMap (by removing the existing element and adding the new one)
			// - elements to add will use NewMap (or MergeMap by creating a new element and merging to it)
			INewMapFactory newElementsFactory;
			try {
				newElementsFactory = elementsMapper.MapNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				newElementsFactory = null!;
			}

			// Need to use try/finally with newElementsFactory because it may be assigned after mergeElementsFactory
			try {
				IMergeMapFactory? mergeElementsFactory;
				try {
					mergeElementsFactory = elementsMapper.MapMergeFactory(elementTypes.From, elementTypes.To, mappingOptions);

					// newElementsFactory cannot be null, so if we have a merge factory we also create a new one from it (without disposing it)
					try {
						newElementsFactory ??= mergeElementsFactory.MapNewFactory(false);
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
													matchedDestinations?.Add(destinationElement);
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
											elementsToRemove.AddRange(destinationEnumerable.Cast<object?>().Except(matchedDestinations!));

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
		}
		#endregion

		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapNewInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper) || elementsMapper == null)
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
				Action<object, object?> addDelegate;
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
		}

		public IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapMergeInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper) || elementsMapper == null)
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
				newElementsFactory = null!;
			}

			try {
				IMergeMapFactory? mergeElementsFactory;
				try {
					mergeElementsFactory = elementsMapper.MapMergeFactory(elementTypes.From, elementTypes.To, mappingOptions);

					// newElementsFactory cannot be null, so if we have a merge factory we also create a new one from it (without disposing it)
					try {
						newElementsFactory ??= mergeElementsFactory.MapNewFactory(false);
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
						Action<object, object?> addDelegate;
						Func<object, object?, bool> removeDelegate;
						if (!TypeUtils.IsCollectionReadonly(types.To)) { 
							addDelegate = ObjectFactory.GetCollectionAddDelegate(elementTypes.To);
							removeDelegate = ObjectFactory.GetCollectionRemoveDelegate(elementTypes.To);
						}
						else {
							addDelegate = null!;
							removeDelegate = null!;
						}

						// Used in case we create a new collection
						var customAddDelegate = ObjectFactory.GetCollectionCustomAddDelegate(actualCollectionType ?? types.To);
						var collectionConversionDelegate = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType ?? types.To, types.To);

						return new DisposableMergeMapFactory(
							sourceType, destinationType,
							(source, destination) => {
								TypeUtils.CheckObjectType(source, types.From, nameof(source));
								TypeUtils.CheckObjectType(destination, types.To, nameof(destination));

								if (source is IEnumerable sourceEnumerable) {
									object result;
									bool recreate;
									try {
										// If we have to create the destination collection we know that we can always map to it,
										// otherwise we check that it's not readonly
										if (destination == null) { 
											destination = collectionFactory.Invoke();
											recreate = false;
										}
										else if (TypeUtils.IsCollectionReadonly(destination)) {
											if (mergeMappingOptions?.RecreateReadonlyDestination ?? _mergeCollectionOptions.RecreateReadonlyDestination)
												recreate = true;
											else { 
												throw new InvalidOperationException("Cannot merge map to a readonly destination collection, destination type is: " +
													(destination.GetType().FullName ?? destination.GetType().Name));
											}
										}
										else
											recreate = false;

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
													elementsToRemove.AddRange(destinationEnumerable.Cast<object?>().Except(matchedDestinations!));

												if (recreate) {
													// Create new collection
													var newDestination = collectionFactory.Invoke();

													// Fill destination collection
													foreach (var element in destinationEnumerable.Cast<object?>().Except(elementsToRemove)) {
														addDelegate.Invoke(newDestination, element);
													}
													foreach (var element in elementsToAdd) {
														addDelegate.Invoke(newDestination, element);
													}
													destination = newDestination;
												}
												else { 
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
		}
		#endregion


		private bool CanMapNewInternal(
			Type sourceType,
			Type destinationType,
			[NotNullWhen(true)] ref MappingOptions? mappingOptions,
			out (Type From, Type To) elementTypes,
			out IMapper elementsMapper) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsEnumerable() && destinationType.IsEnumerable() &&
				ObjectFactory.CanCreateCollection(destinationType)) {

				if(sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition) {
					elementTypes = default;
					elementsMapper = null!;
					mappingOptions = null!;

					return true;
				}
				else { 
					elementTypes = (sourceType.GetEnumerableElementType(), destinationType.GetEnumerableElementType());
					mappingOptions = _optionsCache.GetOrCreate(mappingOptions);
					elementsMapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
						?? _elementsMapper;

					return elementsMapper.CanMapNew(elementTypes.From, elementTypes.To, mappingOptions) ||
						(ObjectFactory.CanCreate(elementTypes.To) && elementsMapper.CanMapMerge(elementTypes.From, elementTypes.To, mappingOptions));
				}
			}
			else {
				elementTypes = default;
				elementsMapper = null!;

				return false;
			}
		}

		private bool CanMapMergeInternal(
			Type sourceType,
			Type destinationType,
			[NotNullWhen(true)] ref MappingOptions? mappingOptions,
			out (Type From, Type To) elementTypes,
			out IMapper elementsMapper) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			var recreate = mappingOptions?.GetOptions<MergeCollectionsMappingOptions>()?.RecreateReadonlyDestination
				?? _mergeCollectionOptions.RecreateReadonlyDestination;

			if (sourceType.IsEnumerable() &&
				((destinationType.IsCollection() && !destinationType.IsArray) ||
					(recreate && destinationType.IsEnumerable())) &&
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
						sourceType.GetEnumerableElementType(),
						recreate ? destinationType.GetEnumerableElementType() : destinationType.GetCollectionElementType());
					mappingOptions = _optionsCache.GetOrCreate(mappingOptions);
					elementsMapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

					return elementsMapper.CanMapNew(elementTypes.From, elementTypes.To, mappingOptions) ||
						(ObjectFactory.CanCreate(elementTypes.To) && elementsMapper.CanMapMerge(elementTypes.From, elementTypes.To, mappingOptions));
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
