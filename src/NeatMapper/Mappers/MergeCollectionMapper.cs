using NeatMapper.Common.Matchers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which merges a collection with an existing one, will try to match elements of the source collection
	/// with the destination by using an <see cref="IMatcher"/> if provided:<br/>
	/// - If a match is found will try to merge the two elements or will replace with a new one by using a <see cref="IMapper"/>.<br/>
	/// - If a match is not found a new element will be added by mapping them with a <see cref="IMapper"/> by trying new map, then merge map.<br/>
	/// Not matched elements from the destination collection are treated according to <see cref="MergeCollectionsOptions"/> (and overrides).
	/// </summary>
	public sealed class MergeCollectionMapper : CustomCollectionMapper {
		readonly IMatcher _elementsMatcher;
		readonly MergeCollectionsOptions _mergeCollectionOptions;

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
			base(elementsMapper, serviceProvider) {

			_elementsMatcher = elementsMatcher != null ? new SafeMatcher(elementsMatcher) : EmptyMatcher.Instance;
			_mergeCollectionOptions = mergeCollectionsOptions ?? new MergeCollectionsOptions();
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
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions = null) {
			// Forward new map to merge by creating a destination collection
			var destination = Map(source, sourceType, CreateCollection(destinationType), destinationType, mappingOptions);
			return ConvertCollectionToType(destination, destinationType);
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
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source != null && !sourceType.IsAssignableFrom(source.GetType()))
				throw new ArgumentException($"Object of type {source.GetType().FullName ?? source.GetType().Name} is not assignable to type {sourceType.FullName ?? sourceType.Name}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			// If both types are collections try mapping the element types
			if (TypeUtils.HasInterface(types.From, typeof(IEnumerable<>)) && types.From != typeof(string) &&
				TypeUtils.HasInterface(types.To, typeof(ICollection<>))) {

				var elementTypes = (
					From: TypeUtils.GetInterfaceElementType(types.From, typeof(IEnumerable<>)),
					To: TypeUtils.GetInterfaceElementType(types.To, typeof(ICollection<>))
				);

				if (source is IEnumerable sourceEnumerable) {
					try {
						if (destination == null)
							destination = CreateCollection(types.To);

						if (destination is IEnumerable destinationEnumerable) {
							var destinationInstanceType = destination.GetType();
							if (destinationInstanceType.IsArray)
								throw new MapNotFoundException(types);

							var interfaceMap = destinationInstanceType.GetInterfaceMap(destinationInstanceType.GetInterfaces()
								.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;

							// If the collection is readonly we cannot map to it
							if ((bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly))).Invoke(destination, null))
								throw new MapNotFoundException(types);

							// Any element that is not mappable will fail on the first mapping

							var addMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Add)));
							var removeMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Remove)));

							var elementsToRemove = new List<object>();
							var elementsToAdd = new List<object>();

							// Adjust the context so that we don't pass any merge matcher along
							var context = CreateMappingContext(mappingOptions);
							var mergeMappingOptions = mappingOptions?.Cast<object>().OfType<MergeCollectionsMappingOptions>().SingleOrDefault();
							mappingOptions = mappingOptions?.Cast<object>().Select(o => {
								if (o is MergeCollectionsMappingOptions merge) {
									var mergeOpts = new MergeCollectionsMappingOptions(merge) {
										Matcher = null
									};
									return mergeOpts;
								}
								else
									return o;
							});

							// Create the matcher and the mapping options
							IMatcher elementMatcher;
							if (mergeMappingOptions?.Matcher != null)
								elementMatcher = new SafeMatcher(new DelegateMatcher(mergeMappingOptions.Matcher, _elementsMatcher, _serviceProvider));
							else
								elementMatcher = _elementsMatcher;

							// Deleted elements
							if (mergeMappingOptions?.RemoveNotMatchedDestinationElements
								?? _mergeCollectionOptions.RemoveNotMatchedDestinationElements) {
								foreach (var destinationElement in destinationEnumerable) {
									bool found = false;
									foreach (var sourceElement in sourceEnumerable) {
										if (elementMatcher.Match(sourceElement, elementTypes.From, destinationElement, elementTypes.To, mappingOptions)) {
											found = true;
											break;
										}
									}

									if (!found)
										elementsToRemove.Add(destinationElement);
								}
							}

							var canCreateNew = true;
							var canCreateMerge = true;

							// Added/updated elements
							foreach (var sourceElement in sourceEnumerable) {
								bool found = false;
								object matchingDestinationElement = null;
								foreach (var destinationElement in destinationEnumerable) {
									if (elementMatcher.Match(sourceElement, elementTypes.From, destinationElement, elementTypes.To, mappingOptions) &&
										!elementsToRemove.Contains(destinationElement)) {

										matchingDestinationElement = destinationElement;
										found = true;
										break;
									}
								}

								if (found) {
									object mergeResult;
									// Try merge map
									if (canCreateMerge) {
										try {
											mergeResult = _elementsMapper.Map(sourceElement, elementTypes.From, matchingDestinationElement, elementTypes.To, mappingOptions);
											if (mergeResult != matchingDestinationElement) {
												elementsToRemove.Add(matchingDestinationElement);
												elementsToAdd.Add(mergeResult);
											}
											continue;
										}
										catch (MapNotFoundException) {
											canCreateMerge = false;
										}
									}

									// Try new map
									if (!canCreateNew)
										throw new MapNotFoundException(types);
									elementsToRemove.Add(matchingDestinationElement);
									elementsToAdd.Add(_elementsMapper.Map(sourceElement, elementTypes.From, elementTypes.To, mappingOptions));
								}
								else {
									// Try new map
									if (canCreateNew) {
										try {
											elementsToAdd.Add(_elementsMapper.Map(sourceElement, elementTypes.From, elementTypes.To, mappingOptions));
											continue;
										}
										catch (MapNotFoundException) {
											canCreateNew = false;
										}
									}

									// Try merge map
									object destinationInstance;
									try {
										destinationInstance = ObjectFactory.Create(elementTypes.To);
									}
									catch (ObjectCreationException) {
										throw new MapNotFoundException(types);
									}

									// Try merge map
									if (!canCreateMerge)
										throw new MapNotFoundException(types);
									elementsToAdd.Add(_elementsMapper.Map(sourceElement, elementTypes.From, destinationInstance, elementTypes.To, mappingOptions));
								}
							}

							foreach (var element in elementsToRemove) {
								if (!(bool)removeMethod.Invoke(destination, new object[] { element }))
									throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
							}
							foreach (var element in elementsToAdd) {
								addMethod.Invoke(destination, new object[] { element });
							}

							var result = ConvertCollectionToType(destination, types.To);

							// Should not happen
							if (result != null && !destinationType.IsAssignableFrom(result.GetType()))
								throw new InvalidOperationException($"Object of type {result.GetType().FullName ?? result.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}");

							return result;
						}
						else
							throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
					}
					catch (MapNotFoundException) {
						throw;
					}
					catch (Exception e) {
						throw new CollectionMappingException(e, types);
					}
				}
				else if (source == null)
					return null;
				else
					throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
			}

			throw new MapNotFoundException(types);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
