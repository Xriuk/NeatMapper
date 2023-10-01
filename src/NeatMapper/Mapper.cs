﻿using NeatMapper.Common.Mapper;
using NeatMapper.Configuration;
using System.Collections;

namespace NeatMapper {
	public class Mapper : BaseMapper, IMapper {
		protected readonly MappingContext _mappingContext;

		protected override MatchingContext MatchingContext => _mappingContext;

		public Mapper(MapperConfigurationOptions? configuration = null, IServiceProvider? serviceProvider = null) :
			base(new MapperConfiguration(i => i == typeof(INewMap<,>)
#if NET7_0_OR_GREATER
                || i == typeof(INewMapStatic<,>)
#endif
				,
				i => i == typeof(IMergeMap<,>)
#if NET7_0_OR_GREATER
                || i == typeof(IMergeMapStatic<,>)
#endif
				, configuration ?? new MapperConfigurationOptions()), serviceProvider) {

			_mappingContext = new MappingContext {
				ServiceProvider = _serviceProvider,
				Mapper = this,
				Matcher = this
			};
		}


		public object? Map(object? source, Type sourceType, Type destinationType) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source?.GetType().IsAssignableTo(sourceType) == false)
				throw new ArgumentException($"Object of type {source.GetType().FullName} is not assignable to type {sourceType.FullName}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			var types = (From: sourceType, To: destinationType);
			object? result;
			try {
				result = MapInternal(types, newMaps)
					.Invoke(new object?[] { source, _mappingContext });
			}
			catch (MapNotFoundException exc) {
				try {
					result = MapCollectionNewRecursiveInternal(types).Invoke(new object[] { source!, _mappingContext });
				}
				catch (MapNotFoundException) {
					object destination;
					try {
						destination = CreateDestinationFactory(destinationType).Invoke();
					}
					catch (DestinationCreationException) {
						throw exc;
					}

					result = Map(source, sourceType, destination, destinationType);
				}
			}

			// Should not happen
			if (result?.GetType().IsAssignableTo(destinationType) == false)
				throw new InvalidOperationException($"Object of type {result.GetType().FullName} is not assignable to type {destinationType.FullName}");

			return result;
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source?.GetType().IsAssignableTo(sourceType) == false)
				throw new ArgumentException($"Object of type {source.GetType().FullName} is not assignable to type {sourceType.FullName}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));
			if (destination?.GetType().IsAssignableTo(destinationType) == false)
				throw new ArgumentException($"Object of type {destination.GetType().FullName} is not assignable to type {destinationType.FullName}", nameof(destination));

			var types = (From: sourceType, To: destinationType);
			object? result;
			try {
				result = MapInternal(types, mergeMaps)
					.Invoke(new object?[] { source, destination, _mappingContext });
			}
			catch (MapNotFoundException exc) {
				try {
					result = MapCollectionMergeRecursiveInternal(types, destination, mappingOptions).Invoke(new object[] { source!, destination!, _mappingContext });
				}
				catch (MapNotFoundException) {
					throw exc;
				}
			}

			// Should not happen
			if (result?.GetType().IsAssignableTo(destinationType) == false)
				throw new InvalidOperationException($"Object of type {result.GetType().FullName} is not assignable to type {destinationType.FullName}");

			return result;
		}


		// (source, context) => destination
		protected Func<object?[], object?> MapCollectionNewRecursiveInternal((Type From, Type To) types) {
			// If both types are collections try mapping the element types
			if (HasInterface(types.From, typeof(IEnumerable<>)) && types.From != typeof(string) &&
				HasInterface(types.To, typeof(IEnumerable<>)) && types.To != typeof(string)) {

				var elementTypes = (
					From: GetInterfaceElementType(types.From, typeof(IEnumerable<>)),
					To: GetInterfaceElementType(types.To, typeof(IEnumerable<>))
				);

				// (source, context) => destination
				Func<object?[], object?> elementMapper;
				try {
					// (source, context) => destination
					elementMapper = MapInternal(elementTypes, newMaps);
				}
				catch (MapNotFoundException) {
					try {
						// (source, destination, context) => destination
						var destinationElementMapper = MapInternal(elementTypes, mergeMaps);

						// () => destination
						var destinationElementFactory = CreateDestinationFactory(elementTypes.To);

						elementMapper = (sourceContext) => destinationElementMapper.Invoke(new object[] {
							sourceContext[0]!,
							destinationElementFactory.Invoke(),
							sourceContext[1]!
						});
					}
					catch (Exception e) when (e is MapNotFoundException || e is DestinationCreationException) {
						try {
							// (source, context) => destination
							elementMapper = MapCollectionNewRecursiveInternal(elementTypes);
						}
						catch (MapNotFoundException) {
							goto End;
						}
					}
				}

				// Check if collection can be created
				if (!CanCreateCollection(types.To))
					goto End;

				return (sourceAndContext) => {
					try {
						var destination = CreateCollection(types.To);
						var addMethod = GetCollectionAddMethod(destination);

						if (sourceAndContext[0] is IEnumerable sourceEnumerable) {
							foreach (var element in sourceEnumerable) {
								addMethod.Invoke(destination, new object?[] { elementMapper.Invoke(new object[] { element, sourceAndContext[1]! }) });
							}

							return ConvertCollectionToType(destination, types.To);
						}
						else if (sourceAndContext[0] == null)
							return null;
						else
							throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
					}
					catch (Exception e) {
						throw new CollectionMappingException(e, types);
					}
				};
			}

			End:

			throw new MapNotFoundException(types);
		}

		// (source, destination, context) => destination
		protected Func<object?[], object?> MapCollectionMergeRecursiveInternal(
			(Type From, Type To) types,
			object? destination,
			MappingOptions? mappingOptions = null) {

			// If both types are collections try mapping the element types
			if (HasInterface(types.From, typeof(IEnumerable<>)) && types.From != typeof(string) &&
				HasInterface(types.To, typeof(ICollection<>))) {

				if (destination == null) {
					// Try creating the collection
					try {
						destination = CreateCollection(types.To);
					}
					catch (DestinationCreationException) {
						goto End;
					}
				}

				var elementTypes = (
					From: GetInterfaceElementType(types.From, typeof(IEnumerable<>)),
					To: GetInterfaceElementType(types.To, typeof(ICollection<>))
				);

				var destinationInstanceType = destination.GetType();
				if (!destinationInstanceType.IsArray) {
					var interfaceMap = destinationInstanceType.GetInterfaceMap(destinationInstanceType.GetInterfaces()
						.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;

					// If the collection is readonly we cannot map to it
					if (!(bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly)))!.Invoke(destination, null)!) {
						// At least one of New or Merge mapper is required to map elements
						// If both are found they will be used in the following order:
						// - elements to update will use MergeMap first (on the existing element),
						//   then NewMap (by removing the existing element and adding the new one)
						// - elements to add will use NewMap first,
						//   then MergeMap (by creating a new element and merging to it)

						// (source, context) => destination
						Func<object?[], object?> newElementMapper = null!;
						// (source, destination, context) => destination
						Func<object?[], object?> mergeElementMapper = null!;
						// () => destination
						Func<object> destinationElementFactory = null!;
						try {
							newElementMapper = MapInternal(elementTypes, newMaps);
						}
						catch (MapNotFoundException) {
							// Here we could recreate any nested collection in case the corresponding merge map would fail,
							// but we are not doing it for consistency: if a merge map on a readonly collection fails,
							// then a nested one should too
							/*try {
								newElementMapper = MapCollectionRecursiveInternal(elementTypes);
							}
							catch (MapNotFoundException) { }*/
						}
						try {
							mergeElementMapper = MapInternal(elementTypes, mergeMaps);
						}
						catch (MapNotFoundException) {
							// If the types are not collections and we don't have a newElementMapper we already know that we can't map them,
							// Otherwise we will try to retrieve the collection map inside the mapping function for each element
							// (because we need to know if the passed runtime types are arrays or not)
							if ((!HasInterface(elementTypes.From, typeof(IEnumerable<>)) || elementTypes.From == typeof(string) ||
								!HasInterface(elementTypes.To, typeof(ICollection<>))) &&
								newElementMapper == null) {

								goto End;
							}
						}
						if (newElementMapper == null) {
							try {
								destinationElementFactory = CreateDestinationFactory(elementTypes.To);
							}
							catch (DestinationCreationException) {
								goto End;
							}
						}

						Func<object?[], bool> elementComparer;
						if (mappingOptions?.Matcher != null) {
							elementComparer = (parameters) => {
								try {
									return mappingOptions.Matcher.Invoke(parameters[0]!, parameters[1]!, (MatchingContext)parameters[2]!);
								}
								catch (Exception e) {
									throw new MatcherException(e, types);
								}
							};
						}
						else
							elementComparer = ElementComparerInternal(elementTypes);

						var addMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Add)));
						var removeMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Remove)));

						return (sourceDestinationAndContext) => {
							if (sourceDestinationAndContext[0] is IEnumerable sourceEnumerable) {
								try {
									if (sourceDestinationAndContext[1] == null)
										sourceDestinationAndContext[1] = CreateCollection(types.To);

									if (sourceDestinationAndContext[1] is IEnumerable destinationEnumerable) {
										var elementsToRemove = new List<object?>();
										var elementsToAdd = new List<object?>();
										Func<object?[], object?> nullMergeCollectionMapping = null!;
										var mergeCollectionMappings = new Dictionary<object, Func<object?[], object?>>();

										// Deleted elements (+ missing merge mappings)
										foreach (var destinationElement in destinationEnumerable) {
											bool found = false;
											foreach (var sourceElement in sourceEnumerable) {
												if (elementComparer.Invoke(new object?[] { sourceElement, destinationElement, sourceDestinationAndContext[2] })) {
													found = true;
													break;
												}
											}

											// If not found we remove it
											// Otherwise if we don't have a merge map we try retrieving it, so that we may fail
											// before mapping elements if needed
											if (!found) {
												if (mappingOptions?.CollectionRemoveNotMatchedDestinationElements
													?? _configuration.MergeMapsCollectionsOptions.RemoveNotMatchedDestinationElements) {

													elementsToRemove.Add(destinationElement);
												}
											}
											else if (mergeElementMapper == null &&
												(destinationElement == null ?
													nullMergeCollectionMapping == null :
													!mergeCollectionMappings.ContainsKey(destinationElement))) {

												try {
													var mapper = MapCollectionMergeRecursiveInternal(elementTypes, destinationElement);
													if (destinationElement == null)
														nullMergeCollectionMapping = mapper;
													else
														mergeCollectionMappings.Add(destinationElement, mapper);
												}
												catch (MapNotFoundException) {
													if (newElementMapper == null)
														throw;
												}
											}
										}

										// Added/updated elements
										foreach (var sourceElement in sourceEnumerable) {
											bool found = false;
											object? matchingDestinationElement = null;
											foreach (var destinationElement in destinationEnumerable) {
												if (elementComparer.Invoke(new object[] { sourceElement, destinationElement, sourceDestinationAndContext[2]! }) &&
													!elementsToRemove.Contains(destinationElement)) {

													matchingDestinationElement = destinationElement;
													found = true;
													break;
												}
											}

											bool tempMergeMapper = false;
											if (found) {
												// If we don't have an element mapper try retrieving it now
												// We don't pass along any collectionElementComparer as they are only for the current collection (if present at all)
												if (mergeElementMapper == null) {
													if (matchingDestinationElement == null) {
														mergeElementMapper = nullMergeCollectionMapping;
														tempMergeMapper = true;
													}
													else if (mergeCollectionMappings.ContainsKey(matchingDestinationElement)) {
														mergeElementMapper = mergeCollectionMappings[matchingDestinationElement];
														tempMergeMapper = true;
													}
												}

												if (mergeElementMapper != null) {
													var mergeResult = mergeElementMapper.Invoke(new object?[] { sourceElement, matchingDestinationElement, sourceDestinationAndContext[2] });
													if (mergeResult != matchingDestinationElement) {
														elementsToRemove.Add(matchingDestinationElement);
														elementsToAdd.Add(mergeResult);
													}
												}
												else {
													elementsToRemove.Add(matchingDestinationElement);
													elementsToAdd.Add(newElementMapper!.Invoke(new object?[] { sourceElement, sourceDestinationAndContext[2] }));
												}
											}
											else {
												if (newElementMapper != null)
													elementsToAdd.Add(newElementMapper.Invoke(new object?[] { sourceElement, sourceDestinationAndContext[2] }));
												else {
													var destinationInstance = destinationElementFactory.Invoke();

													// If we don't have an element mapper try retrieving it now
													// We don't pass along any collectionElementComparer as they are only for the current collection (if present at all)
													if (mergeElementMapper == null) {
														mergeElementMapper = MapCollectionMergeRecursiveInternal(elementTypes, destinationInstance);
														tempMergeMapper = true;
													}

													elementsToAdd.Add(mergeElementMapper!.Invoke(new object?[] { sourceElement, destinationInstance, sourceDestinationAndContext[2] }));
												}
											}

											if (tempMergeMapper)
												mergeElementMapper = null!;
										}

										foreach (var element in elementsToRemove) {
											if (!(bool)removeMethod.Invoke(sourceDestinationAndContext[1], new object?[] { element })!)
												throw new InvalidOperationException($"Could not remove element {element} from the destination collection {sourceDestinationAndContext[1]}");
										}
										foreach (var element in elementsToAdd) {
											addMethod.Invoke(sourceDestinationAndContext[1], new object?[] { element });
										}

										return ConvertCollectionToType(sourceDestinationAndContext[1]!, types.To);
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
				}
			}

			End:

			throw new MapNotFoundException(types);
		}
	}
}
