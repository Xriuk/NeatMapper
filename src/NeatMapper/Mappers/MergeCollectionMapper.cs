using NeatMapper.Configuration;
using NeatMapper.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Mappers {
	/// <summary>
	/// <see cref="IMapper"/> which merges a collection with an existing one, will try to match elements of the source collection
	/// with the destination:<br/>
	/// - If a match is found will try to merge the two elements or will replace with a new one by using a <see cref="IMapper"/>.<br/>
	/// - If a match is not found a new element will be added by mapping them with a <see cref="IMapper"/> by trying new map, then merge map.<br/>
	/// Not matched elements from the destination collection are treated according to <see cref="MergeCollectionsOptions"/> (and overrides).
	/// </summary>
	public sealed class MergeCollectionMapper : CustomCollectionMapper {
		readonly CustomMapsConfiguration _matchMapsConfiguration;
		readonly CustomMapsConfiguration _hierarchyMatchMapsConfiguration;
		readonly MergeCollectionsOptions _mergeCollectionOptions;

		public MergeCollectionMapper(
			IMapper elementsMapper,
#if NET5_0_OR_GREATER
			CustomMapsOptions?
#else
			CustomMapsOptions
#endif
			matchMapsOptions = null,
#if NET5_0_OR_GREATER
			CustomMatchAdditionalMapsOptions?
#else
			CustomMatchAdditionalMapsOptions
#endif
			additionalMatchMapsOptions = null,
#if NET5_0_OR_GREATER
			MergeCollectionsOptions?
#else
			MergeCollectionsOptions
#endif
			mergeCollectionsOptions = null,
#if NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) :
			base(elementsMapper, serviceProvider) { // DEV: pass Composite mapper of elementsMapper and this as element mapper to base

			_matchMapsConfiguration = new CustomMapsConfiguration(
				(_, i) => {
					if (!i.IsGenericType)
						return false;
					var type = i.GetGenericTypeDefinition();
					return type == typeof(IMatchMap<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(IMatchMapStatic<,>)
#endif
					;
				},
				matchMapsOptions ?? new CustomMapsOptions(),
				additionalMatchMapsOptions?._maps.Values
			);

			_hierarchyMatchMapsConfiguration = new CustomMapsConfiguration(
				(t, i) => {
					// Hierarchy matchers do not support generic maps
					if (!i.IsGenericType || t.ContainsGenericParameters)
						return false;
					var type = i.GetGenericTypeDefinition();
					return type == typeof(IHierarchyMatchMap<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(IHierarchyMatchMapStatic<,>)
#endif
					;
				},
				matchMapsOptions ?? new CustomMapsOptions(),
				additionalMatchMapsOptions?._maps.Values
			);

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

			var result = MapCollectionMergeRecursiveInternal((sourceType, destinationType)).Invoke(new object[] { source, CreateMappingContext(mappingOptions) });

			// Should not happen
			if (result != null && !destinationType.IsAssignableFrom(result.GetType()))
				throw new InvalidOperationException($"Object of type {result.GetType().FullName ?? result.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}");

			return result;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		// (source, destination, context) => destination
		Func<object[], object> MapCollectionMergeRecursiveInternal((Type From, Type To) types) {
			// If both types are collections try mapping the element types
			if (TypeUtils.HasInterface(types.From, typeof(IEnumerable<>)) && types.From != typeof(string) &&
				TypeUtils.HasInterface(types.To, typeof(ICollection<>))) {

				var elementTypes = (
					From: TypeUtils.GetInterfaceElementType(types.From, typeof(IEnumerable<>)),
					To: TypeUtils.GetInterfaceElementType(types.To, typeof(ICollection<>))
				);

				return (sourceDestinationAndContext) => {
					if (sourceDestinationAndContext[0] is IEnumerable sourceEnumerable) {
						try {
							if (sourceDestinationAndContext[1] == null)
								sourceDestinationAndContext[1] = CreateCollection(types.To);

							if (sourceDestinationAndContext[1] is IEnumerable destinationEnumerable) {
								var destinationInstanceType = sourceDestinationAndContext[1].GetType();
								if (destinationInstanceType.IsArray)
									throw new MapNotFoundException(types);

								var interfaceMap = destinationInstanceType.GetInterfaceMap(destinationInstanceType.GetInterfaces()
									.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;

								// If the collection is readonly we cannot map to it
								if ((bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly))).Invoke(sourceDestinationAndContext[1], null))
									throw new MapNotFoundException(types);

								// Any element that is not mappable will fail on the first mapping

								var addMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Add)));
								var removeMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Remove)));

								var elementsToRemove = new List<object>();
								var elementsToAdd = new List<object>();

								// Adjust the context so that we don't pass any merge matcher along
								var context = (MappingContext)sourceDestinationAndContext[2];
								var mergeMappingOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
								context.MappingOptions = new MappingOptions(context.MappingOptions.AsEnumerable().Select(o => {
									if (o is MergeCollectionsMappingOptions merge) {
										var mergeOpts = new MergeCollectionsMappingOptions(merge) {
											Matcher = null
										};
										return mergeOpts;
									}
									else
										return o;
								}));

								// (source, destination, context) => bool
								Func<object[], bool> elementComparer;
								if (mergeMappingOptions?.Matcher != null) {
									elementComparer = (parameters) => {
										try {
											return mergeMappingOptions.Matcher.Invoke(parameters[0], parameters[1], (MatchingContext)parameters[2]);
										}
										catch (Exception e) {
											throw new MatcherException(e, types);
										}
									};
								}
								else {
									try {
										// Try retrieving a regular map
										var comparer = _matchMapsConfiguration.GetMap(elementTypes);
										elementComparer = (parameters) => {
											try {
												return (bool)comparer.Invoke(parameters);
											}
											catch (MappingException e) {
												throw new MatcherException(e.InnerException, types);
											}
										};
									}
									catch (MapNotFoundException) {
										// Try retrieving a hierarchy map
										try {
											var map = _hierarchyMatchMapsConfiguration.Maps.First(m =>
												m.Key.From.IsAssignableFrom(types.From) &&
												m.Key.To.IsAssignableFrom(types.To));
											elementComparer = (parameters) => {
												try {
													return (bool)map.Value.Method.Invoke(map.Value.Method.IsStatic ? null : map.Value.Instance ?? CustomMapsConfiguration.CreateOrReturnInstance(map.Value.Method.DeclaringType), parameters);
												}
												catch (Exception e) {
													throw new MappingException(e, types);
												}
											};
										}
										catch {
											elementComparer = (_) => false;
										}
									}
								}

								// Deleted elements
								if (mergeMappingOptions?.RemoveNotMatchedDestinationElements
									?? _mergeCollectionOptions.RemoveNotMatchedDestinationElements) {
									foreach (var destinationElement in destinationEnumerable) {
										bool found = false;
										foreach (var sourceElement in sourceEnumerable) {
											if (elementComparer.Invoke(new object[] { sourceElement, destinationElement, context })) {
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
										if (elementComparer.Invoke(new object[] { sourceElement, destinationElement, context }) &&
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
												mergeResult = _elementsMapper.Map(sourceElement, elementTypes.From, matchingDestinationElement, elementTypes.To, context.MappingOptions.AsEnumerable());
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
										elementsToAdd.Add(_elementsMapper.Map(sourceElement, elementTypes.From, elementTypes.To, context.MappingOptions.AsEnumerable()));
									}
									else {
										// Try new map
										if (canCreateNew) {
											try {
												elementsToAdd.Add(_elementsMapper.Map(sourceElement, elementTypes.From, elementTypes.To, context.MappingOptions.AsEnumerable()));
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
										elementsToAdd.Add(_elementsMapper.Map(sourceElement, elementTypes.From, destinationInstance, elementTypes.To, context.MappingOptions.AsEnumerable()));
									}
								}

								foreach (var element in elementsToRemove) {
									if (!(bool)removeMethod.Invoke(sourceDestinationAndContext[1], new object[] { element }))
										throw new InvalidOperationException($"Could not remove element {element} from the destination collection {sourceDestinationAndContext[1]}");
								}
								foreach (var element in elementsToAdd) {
									addMethod.Invoke(sourceDestinationAndContext[1], new object[] { element });
								}

								return ConvertCollectionToType(sourceDestinationAndContext[1], types.To);
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
					else if (sourceDestinationAndContext[0] == null)
						return null;
					else
						throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
				};
			}

			throw new MapNotFoundException(types);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
