using NeatMapper.Common.Mapper;
using NeatMapper.Configuration;
using System;
using System.Threading.Tasks;
using System.Threading;
using NeatMapper.Async.Internal;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using NeatMapper.Common.Internal;

namespace NeatMapper.Async
{
    /// <summary>
    /// Default implementation for <see cref="IAsyncMapper"/> and <see cref="IMatcher"/>
    /// </summary>
    public class AsyncMapper : BaseMapper, IAsyncMapper {
		public AsyncMapper(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			AsyncMapperOptions?
#else
			AsyncMapperOptions
#endif
			mapperOptions,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) : this(new MapperConfigurationOptions(), mapperOptions, serviceProvider) { }
		public AsyncMapper(MapperConfigurationOptions configurationOptions,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) : this(configurationOptions, null, serviceProvider) { }
		public AsyncMapper(MapperConfigurationOptions configuration,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			AsyncMapperOptions?
#else
			AsyncMapperOptions
#endif
			mapperOptions,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) :
				base(i => i == typeof(IAsyncNewMap<,>)
#if NET7_0_OR_GREATER
					|| i == typeof(IAsyncNewMapStatic<,>)
#endif
					,
					i => i == typeof(IAsyncMergeMap<,>)
#if NET7_0_OR_GREATER
					|| i == typeof(IAsyncMergeMapStatic<,>)
#endif
					,
					configuration ?? new MapperConfigurationOptions(),
					mapperOptions,
					serviceProvider) {}

		public async Task<
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
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

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
				result = await TaskUtils.AwaitTask<object>((Task)MapInternal(types, newMaps, CreateOrReturnInstance)
					.Invoke(new object[] { source, CreateMappingContext(mappingOptions, cancellationToken) }));
			}
			catch (MapNotFoundException exc) {
				try {
					result = await MapCollectionNewRecursiveInternal(types).Invoke(new object[] { source, CreateMappingContext(mappingOptions, cancellationToken) });
				}
				catch (MapNotFoundException) {
					object destination;
					try {
						destination = CreateDestinationFactory(destinationType).Invoke();
					}
					catch (ObjectCreationException) {
						throw exc;
					}

					// Should handle exceptions
					result = await MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken);
				}
				catch (Exception e) when (!(e is MappingException) && !(e is CollectionMappingException)) {
					throw new MappingException(e, types);
				}
			}
			catch (Exception e) when (!(e is MappingException) && !(e is CollectionMappingException)) {
				throw new MappingException(e, types);
			}

			// Should not happen
			if (result != null && !destinationType.IsAssignableFrom(result.GetType()))
				throw new InvalidOperationException($"Object of type {result.GetType().FullName ?? result.GetType().Name}  is not assignable to type  {destinationType.FullName ?? destinationType.Name}");

			return result;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public async Task<
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
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

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
				throw new ArgumentException($"Object of type {destination.GetType().FullName ?? destination.GetType().Name}  is not assignable to type  {destinationType.FullName ?? destinationType.Name}", nameof(destination));

			var types = (From: sourceType, To: destinationType);
			object result;
			try {
				result = await TaskUtils.AwaitTask<object>((Task)MapInternal(types, mergeMaps, CreateOrReturnInstance)
					.Invoke(new object[] { source, destination, CreateMappingContext(mappingOptions, cancellationToken) }));
			}
			catch (MapNotFoundException exc) {
				try {
					result = await MapCollectionMergeRecursiveInternal(types, destination).Invoke(new object[] { source, destination, CreateMappingContext(mappingOptions, cancellationToken) });
				}
				catch (MapNotFoundException) {
					throw exc;
				}
				catch (Exception e) when (!(e is MappingException) && !(e is CollectionMappingException)) {
					throw new MappingException(e, types);
				}
			}
			catch (Exception e) when (!(e is MappingException) && !(e is CollectionMappingException)) {
				throw new MappingException(e, types);
			}

			// Should not happen
			if (result != null && !destinationType.IsAssignableFrom(result.GetType()))
				throw new InvalidOperationException($"Object of type {result.GetType().FullName ?? result.GetType().Name}  is not assignable to type  {destinationType.FullName ?? destinationType.Name}");

			return result;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		protected AsyncMappingContext CreateMappingContext(IEnumerable mappingOptions, CancellationToken cancellationToken) {
			return new AsyncMappingContext {
				Mapper = this,
				Matcher = this,
				ServiceProvider = _serviceProvider,
				CancellationToken = cancellationToken,
				MappingOptions = new MappingOptions(mappingOptions)
			};
		}

		// (source, context) => Task<destination>
		protected Func<object[], Task<object>> MapCollectionNewRecursiveInternal((Type From, Type To) types) {
			// If both types are collections try mapping the element types
			if (HasInterface(types.From, typeof(IEnumerable<>)) && types.From != typeof(string) &&
				HasInterface(types.To, typeof(IEnumerable<>)) && types.To != typeof(string)) {

				var elementTypes = (
					From: GetInterfaceElementType(types.From, typeof(IEnumerable<>)),
					To: GetInterfaceElementType(types.To, typeof(IEnumerable<>))
				);

				// (source, context) => Task<destination>
				Func<object[], object> elementMapper;
				try {
					// (source, context) => Task<destination>
					elementMapper = MapInternal(elementTypes, newMaps, CreateOrReturnInstance);
				}
				catch (MapNotFoundException) {
					try {
						// (source, destination, context) => Task<destination>
						var destinationElementMapper = MapInternal(elementTypes, mergeMaps, CreateOrReturnInstance);

						// () => destination
						var destinationElementFactory = CreateDestinationFactory(elementTypes.To);

						elementMapper = (sourceContext) => destinationElementMapper.Invoke(new object[] {
							sourceContext[0],
							destinationElementFactory.Invoke(),
							sourceContext[1]
						});
					}
					catch (Exception e) when (e is MapNotFoundException || e is ObjectCreationException) {
						try {
							// (source, context) => Task<destination>
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

				return async (sourceAndContext) => {
					try {
						if (sourceAndContext[0] is IEnumerable sourceEnumerable) {
							var destination = CreateCollection(types.To);
							var addMethod = GetCollectionAddMethod(destination);

							// Adjust the context so that we don't pass any merge matcher along
							var context = (AsyncMappingContext)sourceAndContext[1];
							context.MappingOptions = new MappingOptions(context.MappingOptions.AsEnumerable().Select(o => {
								if (!(o is MergeCollectionsMappingOptions merge))
									return o;
								else {
									var mergeOpts = new MergeCollectionsMappingOptions(merge);
									mergeOpts.Matcher = null;
									return mergeOpts;
								}
							}));

							foreach (var element in sourceEnumerable) {
								object destinationElement;
								try {
									destinationElement = await TaskUtils.AwaitTask<object>((Task)elementMapper.Invoke(new object[] { element, context }));
								}
								catch (Exception e) when (!(e is MappingException) && !(e is CollectionMappingException)) {
									throw new MappingException(e, types);
								}
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

		// (source, destination, context) => Task<destination>
		protected Func<object[], Task<object>> MapCollectionMergeRecursiveInternal(
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
					catch (ObjectCreationException) {
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

						// (source, context) => Task<destination>
						Func<object[], object> newElementMapper = null;
						// (source, destination, context) => Task<destination>
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
							catch (ObjectCreationException) {
								goto End;
							}
						}

						var addMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Add)));
						var removeMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Remove)));

						return async (sourceDestinationAndContext) => {
							if (sourceDestinationAndContext[0] is IEnumerable sourceEnumerable) {
								try {
									if (sourceDestinationAndContext[1] == null)
										sourceDestinationAndContext[1] = CreateCollection(types.To);

									if (sourceDestinationAndContext[1] is IEnumerable destinationEnumerable) {
										var elementsToRemove = new List<object>();
										var elementsToAdd = new List<object>();
										// (source, destination, context) => Task<destination>
										Func<object[], Task<object>> nullMergeCollectionMapping = null;
										var mergeCollectionMappings = new Dictionary<object, Func<object[], Task<object>>>();

										// Adjust the context so that we don't pass any merge matcher along
										var context = (AsyncMappingContext)sourceDestinationAndContext[2];
										var mergeMappingOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
										context.MappingOptions = new MappingOptions(context.MappingOptions.AsEnumerable().Select(o => {
											if (!(o is MergeCollectionsMappingOptions merge))
												return o;
											else {
												var mergeOpts = new MergeCollectionsMappingOptions(merge);
												mergeOpts.Matcher = null;
												return mergeOpts;
											}
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
										else
											elementComparer = ElementComparerInternal(elementTypes);

										var colletionRemoveNotMatched = mergeMappingOptions?.RemoveNotMatchedDestinationElements
											?? _configuration.MergeMapsCollectionsOptions.RemoveNotMatchedDestinationElements;

										// Deleted elements (+ missing merge mappings)
										foreach (var destinationElement in destinationEnumerable) {
											bool found = false;
											foreach (var sourceElement in sourceEnumerable) {
												if (elementComparer.Invoke(new object[] { sourceElement, destinationElement, context })) {
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
												if (elementComparer.Invoke(new object[] { sourceElement, destinationElement, context }) &&
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
													object mergeResult;
													try { 
														mergeResult = await TaskUtils.AwaitTask<object>((Task)mergeElementMapper.Invoke(new object[] { sourceElement, matchingDestinationElement, context }));
													}
													catch (Exception e) when (!(e is MappingException) && !(e is CollectionMappingException)) {
														throw new MappingException(e, types);
													}

													if (mergeResult != matchingDestinationElement) {
														elementsToRemove.Add(matchingDestinationElement);
														elementsToAdd.Add(mergeResult);
													}
												}
												else {
													elementsToRemove.Add(matchingDestinationElement);

													object newResult;
													try { 
														newResult = await TaskUtils.AwaitTask<object>((Task)newElementMapper.Invoke(new object[] { sourceElement, context }));
													}
													catch (Exception e) when (!(e is MappingException) && !(e is CollectionMappingException)) {
														throw new MappingException(e, types);
													}

													elementsToAdd.Add(newResult);
												}
											}
											else {
												if (newElementMapper != null) {
													object newResult;
													try { 
														newResult = await TaskUtils.AwaitTask<object>((Task)newElementMapper.Invoke(new object[] { sourceElement, context }));
													}
													catch (Exception e) when (!(e is MappingException) && !(e is CollectionMappingException)) {
														throw new MappingException(e, types);
													}

													elementsToAdd.Add(newResult);
												}
												else {
													var destinationInstance = destinationElementFactory.Invoke();

													// If we don't have an element mapper try retrieving it now
													// We don't pass along any collectionElementComparer as they are only for the current collection (if present at all)
													if (mergeElementMapper == null) {
														mergeElementMapper = MapCollectionMergeRecursiveInternal(elementTypes, destinationInstance);
														tempMergeMapper = true;
													}

													object mergeResult;
													try { 
														mergeResult = await TaskUtils.AwaitTask<object>((Task)mergeElementMapper.Invoke(new object[] { sourceElement, destinationInstance, context }));
													}
													catch (Exception e) when (!(e is MappingException) && !(e is CollectionMappingException)) {
														throw new MappingException(e, types);
													}

													elementsToAdd.Add(mergeResult);
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
