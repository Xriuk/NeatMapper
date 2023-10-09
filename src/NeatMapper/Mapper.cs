using NeatMapper.Common.Mapper;
using NeatMapper.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// Default implementation for <see cref="IMapper"/> and <see cref="IMatcher"/>
	/// </summary>
	public class Mapper : BaseMapper, IMapper {
		public Mapper(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MapperOptions?
#else
			MapperOptions
#endif
			mapperOptions,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) : this(new MapperConfigurationOptions(), mapperOptions, serviceProvider) { }
		public Mapper(MapperConfigurationOptions configurationOptions,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) : this(configurationOptions, null, serviceProvider) {}
		public Mapper(MapperConfigurationOptions configurationOptions,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MapperOptions?
#else
			MapperOptions
#endif
			mapperOptions,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) :
				base(i => i == typeof(INewMap<,>)
#if NET7_0_OR_GREATER
					|| i == typeof(INewMapStatic<,>)
#endif
					,
					i => i == typeof(IMergeMap<,>)
#if NET7_0_OR_GREATER
					|| i == typeof(IMergeMapStatic<,>)
#endif
					,
					configurationOptions,
					mapperOptions,
					serviceProvider) {}


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

			var types = (From: sourceType, To: destinationType);
			object result;
			try {
				result = MapInternal(types, newMaps, CreateOrReturnInstance)
					.Invoke(new object[] { source, CreateMappingContext(mappingOptions) });
			}
			catch (MapNotFoundException exc) {
				try {
					result = MapCollectionNewRecursiveInternal(types).Invoke(new object[] { source, CreateMappingContext(mappingOptions) });
				}
				catch (MapNotFoundException) {
					object destination;
					try {
						destination = CreateDestinationFactory(destinationType).Invoke();
					}
					catch (DestinationCreationException) {
						throw exc;
					}

					result = Map(source, sourceType, destination, destinationType, mappingOptions);
				}
			}

			// Should not happen
			if (result != null && !destinationType.IsAssignableFrom(result.GetType()))
				throw new InvalidOperationException($"Object of type {result.GetType().FullName ?? result.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}");

			return result;

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
			if (destination != null && !destinationType.IsAssignableFrom(destination.GetType()))
				throw new ArgumentException($"Object of type {destination.GetType().FullName ?? destination.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}", nameof(destination));

			var types = (From: sourceType, To: destinationType);
			object result;
			try {
				result = MapInternal(types, mergeMaps, CreateOrReturnInstance)
					.Invoke(new object[] { source, destination, CreateMappingContext(mappingOptions) });
			}
			catch (MapNotFoundException exc) {
				try {
					result = MapCollectionMergeRecursiveInternal(types, destination).Invoke(new object[] { source, destination, CreateMappingContext(mappingOptions) });
				}
				catch (MapNotFoundException) {
					throw exc;
				}
			}

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

		protected MappingContext CreateMappingContext(IEnumerable mappingOptions) {
			return new MappingContext {
				Mapper = this,
				Matcher = this,
				ServiceProvider = _serviceProvider,
				MappingOptions = new MappingOptions(mappingOptions)
			};
		}

		// (source, context) => destination
		protected Func<object[], object> MapCollectionNewRecursiveInternal((Type From, Type To) types) {
			// If both types are collections try mapping the element types
			if (HasInterface(types.From, typeof(IEnumerable<>)) && types.From != typeof(string) &&
				HasInterface(types.To, typeof(IEnumerable<>)) && types.To != typeof(string)) {

				var elementTypes = (
					From: GetInterfaceElementType(types.From, typeof(IEnumerable<>)),
					To: GetInterfaceElementType(types.To, typeof(IEnumerable<>))
				);

				// (source, context) => destination
				Func<object[], object> elementMapper;
				try {
					// (source, context) => destination
					elementMapper = MapInternal(elementTypes, newMaps, CreateOrReturnInstance);
				}
				catch (MapNotFoundException) {
					try {
						// (source, destination, context) => destination
						var destinationElementMapper = MapInternal(elementTypes, mergeMaps, CreateOrReturnInstance);

						// () => destination
						var destinationElementFactory = CreateDestinationFactory(elementTypes.To);

						elementMapper = (sourceContext) => destinationElementMapper.Invoke(new object[] {
							sourceContext[0],
							destinationElementFactory.Invoke(),
							sourceContext[1]
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
								var destinationElement = elementMapper.Invoke(new object[] { element, sourceAndContext[1] });
								addMethod.Invoke(destination, new object[] { destinationElement });
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
		protected Func<object[], object> MapCollectionMergeRecursiveInternal(
			(Type From, Type To) types,
			object destination) {

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
					if (!(bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly))).Invoke(destination, null)) {
						// At least one of New or Merge mapper is required to map elements
						// If both are found they will be used in the following order:
						// - elements to update will use MergeMap first (on the existing element),
						//   then NewMap (by removing the existing element and adding the new one)
						// - elements to add will use NewMap first,
						//   then MergeMap (by creating a new element and merging to it)

						// (source, context) => destination
						Func<object[], object> newElementMapper = null;
						// (source, destination, context) => destination
						Func<object[], object> mergeElementMapper = null;
						// () => destination
						Func<object> destinationElementFactory = null;
						try {
							newElementMapper = MapInternal(elementTypes, newMaps, CreateOrReturnInstance);
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
							mergeElementMapper = MapInternal(elementTypes, mergeMaps, CreateOrReturnInstance);
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

						var addMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Add)));
						var removeMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Remove)));

						return (sourceDestinationAndContext) => {
							if (sourceDestinationAndContext[0] is IEnumerable sourceEnumerable) {
								try {
									if (sourceDestinationAndContext[1] == null)
										sourceDestinationAndContext[1] = CreateCollection(types.To);

									if (sourceDestinationAndContext[1] is IEnumerable destinationEnumerable) {
										var elementsToRemove = new List<object>();
										var elementsToAdd = new List<object>();
										Func<object[], object> nullMergeCollectionMapping = null;
										var mergeCollectionMappings = new Dictionary<object, Func<object[], object>>();


										var mergeMappingOptions = ((MappingContext)sourceDestinationAndContext[2]).MappingOptions.GetOptions<MergeMappingOptions>();

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
										else
											elementComparer = ElementComparerInternal(elementTypes);

										var colletionRemoveNotMatched = mergeMappingOptions?.CollectionRemoveNotMatchedDestinationElements
											?? _configuration.MergeMapsCollectionsOptions.RemoveNotMatchedDestinationElements;

										// Deleted elements (+ missing merge mappings)
										foreach (var destinationElement in destinationEnumerable) {
											bool found = false;
											foreach (var sourceElement in sourceEnumerable) {
												if (elementComparer.Invoke(new object[] { sourceElement, destinationElement, sourceDestinationAndContext[2] })) {
													found = true;
													break;
												}
											}

											// If not found we remove it
											// Otherwise if we don't have a merge map we try retrieving it, so that we may fail
											// before mapping elements if needed
											if (!found) {
												if (colletionRemoveNotMatched)
													elementsToRemove.Add(destinationElement);
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
											object matchingDestinationElement = null;
											foreach (var destinationElement in destinationEnumerable) {
												if (elementComparer.Invoke(new object[] { sourceElement, destinationElement, sourceDestinationAndContext[2] }) &&
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
													var mergeResult = mergeElementMapper.Invoke(new object[] { sourceElement, matchingDestinationElement, sourceDestinationAndContext[2] });
													if (mergeResult != matchingDestinationElement) {
														elementsToRemove.Add(matchingDestinationElement);
														elementsToAdd.Add(mergeResult);
													}
												}
												else {
													elementsToRemove.Add(matchingDestinationElement);
													elementsToAdd.Add(newElementMapper.Invoke(new object[] { sourceElement, sourceDestinationAndContext[2] }));
												}
											}
											else {
												if (newElementMapper != null)
													elementsToAdd.Add(newElementMapper.Invoke(new object[] { sourceElement, sourceDestinationAndContext[2] }));
												else {
													var destinationInstance = destinationElementFactory.Invoke();

													// If we don't have an element mapper try retrieving it now
													// We don't pass along any collectionElementComparer as they are only for the current collection (if present at all)
													if (mergeElementMapper == null) {
														mergeElementMapper = MapCollectionMergeRecursiveInternal(elementTypes, destinationInstance);
														tempMergeMapper = true;
													}

													elementsToAdd.Add(mergeElementMapper.Invoke(new object[] { sourceElement, destinationInstance, sourceDestinationAndContext[2] }));
												}
											}

											if (tempMergeMapper)
												mergeElementMapper = null;
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
				}
			}

			End:

			throw new MapNotFoundException(types);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
