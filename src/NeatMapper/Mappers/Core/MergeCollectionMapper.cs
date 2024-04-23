using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which merges a <see cref="IEnumerable{T}"/> (even nested) with an existing
	/// <see cref="ICollection{T}"/> (not readonly), will create a new <see cref="ICollection{T}"/>
	/// if destination is null.<br/>
	/// Will try to match elements of the source collection with the destination by using an
	/// <see cref="IMatcher"/> if provided:<br/>
	/// - If a match is found will try to merge the two elements or will replace with a new one by using
	/// a <see cref="IMapper"/>.<br/>
	/// - If a match is not found a new element will be added by mapping the types with a <see cref="IMapper"/>
	/// by trying new map, then merge map.<br/>
	/// Not matched elements from the destination collection are treated according to
	/// <see cref="MergeCollectionsOptions"/> (and overrides).<br/>
	/// Collections are NOT mapped lazily, all source elements are evaluated during the map.
	/// </summary>
	public sealed class MergeCollectionMapper : CollectionMapper, IMapperCanMap, IMapperFactory {
		// DEV: what is it used for? Try to remove
		private readonly IMapper _originalElementMapper;

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
		/// Service provider to be passed to <see cref="MergeCollectionsMappingOptions.Matcher"/>.
		/// </summary>
		private readonly IServiceProvider _serviceProvider;


		/// <summary>
		/// Creates a new instance of <see cref="MergeCollectionMapper"/>.
		/// </summary>
		/// <param name="elementsMapper">
		/// <see cref="IMapper"/> to use to map collection elements.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions.Mapper"/>.
		/// </param>
		/// <param name="elementsMatcher">
		/// <see cref="IMatcher"/> used to match elements between collections to merge them,
		/// if null the elements won't be matched (this will effectively be the same as using
		/// <see cref="NewCollectionMapper"/>).<br/>
		/// Can be overridden during mapping with <see cref="MergeCollectionsMappingOptions.Matcher"/>.
		/// </param>
		/// <param name="mergeCollectionsOptions">
		/// Additional merging options to apply during mapping, null to use default.<br/>
		/// Can be overridden during mapping with <see cref="MergeCollectionsMappingOptions"/>.
		/// </param>
		/// <param name="serviceProvider">
		/// Service provider to be passed to <see cref="MergeCollectionsMappingOptions.Matcher"/>, 
		/// null to pass an empty service provider.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions.ServiceProvider"/>.
		/// </param>
		public MergeCollectionMapper(
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
			mergeCollectionsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) :
				base(elementsMapper) {

			_originalElementMapper = elementsMapper;
			_elementsMatcher = elementsMatcher != null ? new SafeMatcher(elementsMatcher) : EmptyMatcher.Instance;
			_mergeCollectionOptions = mergeCollectionsOptions ?? new MergeCollectionsOptions();
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
		}


		#region IMapper methods
		override public
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

			// Not mapping new
			throw new MapNotFoundException((sourceType, destinationType));
		}

		override public
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
			return false;
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

			// Not mapping new
			throw new MapNotFoundException((sourceType, destinationType));
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

			(Type From, Type To) types = (sourceType, destinationType);

			// If both types are collections try mapping the element types
			if (types.From.IsEnumerable() && types.To.IsCollection() && !types.To.IsArray) {
				// If the destination type is not an interface, check if it is not readonly
				if (!destinationType.IsInterface && destinationType.IsGenericType) {
					var collectionDefinition = destinationType.GetGenericTypeDefinition();
					if (collectionDefinition == typeof(ReadOnlyCollection<>) ||
						collectionDefinition == typeof(ReadOnlyDictionary<,>) ||
						collectionDefinition == typeof(ReadOnlyObservableCollection<>)) {

						throw new MapNotFoundException(types);
					}
				}

				var elementTypes = (From: types.From.GetEnumerableElementType(), To: types.To.GetCollectionElementType());

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

				var addDelegate = ObjectFactory.GetCollectionAddDelegate(elementTypes.To);
				var removeDelegate = ObjectFactory.GetCollectionRemoveDelegate(elementTypes.To);

				// Used in case we create a new collection
				var collectionConversionDelegate = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType ?? types.To, types.To);

				mappingOptions = MergeOrCreateMappingOptions(mappingOptions, out var mergeMappingOptions);
				var elementsMapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

				// At least one of New or Merge mapper is required to map elements
				// If both are found they will be used in the following order:
				// - elements to update will use MergeMap first (on the existing element),
				//   then NewMap (by removing the existing element and adding the new one)
				// - elements to add will use NewMap first,
				//   then MergeMap (by creating a new element and merging to it)
				INewMapFactory newElementsFactory;
				try {
					newElementsFactory = elementsMapper.MapNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
				}
				catch (MapNotFoundException) {
					newElementsFactory = null;
				}

				try { 
					IMergeMapFactory mergeElementsFactory;
					Func<object> destinationFactory = null;
					try {
						mergeElementsFactory = elementsMapper.MapMergeFactory(elementTypes.From, elementTypes.To, mappingOptions);
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
						// Create the matcher (it will never throw because of SafeMatcher/EmptyMatcher)
						IMatcher elementsMatcher;
						if (mergeMappingOptions?.Matcher != null)
							elementsMatcher = new SafeMatcher(new DelegateMatcher(mergeMappingOptions.Matcher, _elementsMatcher, _serviceProvider));
						else
							elementsMatcher = _elementsMatcher;
						var elementsMatcherFactory = elementsMatcher.MatchFactory(elementTypes.From, elementTypes.To, mappingOptions);

						var removeNotMatchedDestinationElements = mergeMappingOptions?.RemoveNotMatchedDestinationElements
							?? _mergeCollectionOptions.RemoveNotMatchedDestinationElements;

						return new DisposableMergeMapFactory(
							sourceType, destinationType,
							(source, destination) => {
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
											if (!CanMapMerge(types.From, types.To, destination as IEnumerable, mappingOptions))
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
											if (removeNotMatchedDestinationElements) {
												foreach (var destinationElement in destinationEnumerable) {
													bool found = false;
													foreach (var sourceElement in sourceEnumerable) {
														if (elementsMatcherFactory.Invoke(sourceElement, destinationElement)) {
															found = true;
															break;
														}
													}

													if (!found)
														elementsToRemove.Add(destinationElement);
												}
											}

											// Added/updated elements
											foreach (var sourceElement in sourceEnumerable) {
												bool found = false;
												object matchingDestinationElement = null;
												foreach (var destinationElement in destinationEnumerable) {
													if (elementsMatcherFactory.Invoke(sourceElement, destinationElement) &&
														!elementsToRemove.Contains(destinationElement)) {

														matchingDestinationElement = destinationElement;
														found = true;
														break;
													}
												}

												if (found) {
													// Try merge map
													if (canMerge) {
														try {
															var mergeResult = mergeElementsFactory.Invoke(sourceElement, matchingDestinationElement);
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
														elementsToAdd.Add(newElementsFactory.Invoke(sourceElement));
													}
													catch (MapNotFoundException) {
														throw new MapNotFoundException(types);
													}
												}
												else {
													// Try new map
													if (canNew) {
														try {
															elementsToAdd.Add(newElementsFactory.Invoke(sourceElement));
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
														elementsToAdd.Add(mergeElementsFactory.Invoke(sourceElement, destinationFactory.Invoke()));
													}
													catch (MapNotFoundException) {
														throw new MapNotFoundException(types);
													}
												}
											}

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
									try {
										if (elementsMapper.CanMapNew(elementTypes.From, elementTypes.To, mappingOptions))
											return null;
									}
									catch { }

									try {
										if (elementsMapper.CanMapMerge(elementTypes.From, elementTypes.To, mappingOptions) && ObjectFactory.CanCreate(elementTypes.To))
											return null;
									}
									catch { }

									throw new MapNotFoundException(types);
								}
								else
									throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
							},
							newElementsFactory, mergeElementsFactory, elementsMatcherFactory);
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

		bool CanMapMerge(
			Type sourceType,
			Type destinationType,
			IEnumerable destination = null,
			MappingOptions mappingOptions = null) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsEnumerable() && destinationType.IsCollection() && !destinationType.IsArray) {
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
				else if (destination != null) { 
					var destinationInstanceType = destination.GetType();
					if (destinationInstanceType.IsArray)
						return false;

					var interfaceMap = destinationInstanceType.GetInterfaceMap(destinationInstanceType.GetInterfaces()
						.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;

					// If the collection is readonly we cannot map to it
					if ((bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly))).Invoke(destination, null))
						return false;
				}

				var elementTypes = (From: sourceType.GetEnumerableElementType(), To: destinationType.GetCollectionElementType());

				mappingOptions = MergeOrCreateMappingOptions(mappingOptions, out _);

				var elementsMapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper ?? _originalElementMapper;

				// If the original element mapper can map the types on its own we succeed
				bool? canMapNew;
				try {
					canMapNew = elementsMapper.CanMapNew(elementTypes.From, elementTypes.To, mappingOptions);
				}
				catch (InvalidOperationException) {
					canMapNew = null;
				}
				bool? canMapMerge;
				try {
					canMapMerge = elementsMapper.CanMapMerge(elementTypes.From, elementTypes.To, mappingOptions);

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
					canMapNested = CanMapMerge(elementTypes.From, elementTypes.To, null, mappingOptions);
				}
				catch (InvalidOperationException) {
					canMapNested = null;
				}

				// If we have a concrete class we already checked that it's not readonly
				// Otherwise if we have a destination check if all its elements can be mapped
				if ((canMapNew == true || canMapMerge == true || canMapNested == true) && !destinationType.IsInterface)
					return true;
				else if (canMapNested == null && destination != null) {
					foreach (var element in destination) {
						var elementInstanceType = element.GetType();
						if (elementInstanceType.IsArray)
							return false;

						var interfaceMap = elementInstanceType.GetInterfaceMap(elementInstanceType.GetInterfaces()
							.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;

						// If the collection is readonly we cannot map to it
						if ((bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly))).Invoke(element, null))
							return false;
					}

					return true;
				}
				else if (canMapNew == false && canMapMerge == false && canMapNested == false)
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
