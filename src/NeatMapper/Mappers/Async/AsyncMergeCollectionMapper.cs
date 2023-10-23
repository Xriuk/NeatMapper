using NeatMapper.Common.Matchers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which merges a collection (even nested) with an existing one asynchronously, can also create a new collection.<br/>
	/// Will try to match elements of the source collection with the destination by using an <see cref="IMatcher"/> if provided:<br/>
	/// - If a match is found will try to merge the two elements or will replace with a new one by using a <see cref="IAsyncMapper"/>.<br/>
	/// - If a match is not found a new element will be added by mapping them with a <see cref="IAsyncMapper"/> by trying new map, then merge map.<br/>
	/// Not matched elements from the destination collection are treated according to <see cref="MergeCollectionsOptions"/> (and overrides).
	/// </summary>
	public sealed class AsyncMergeCollectionMapper : AsyncCustomCollectionMapper, IAsyncMapperCanMap {
		readonly IAsyncMapper _originalElementMapper;
		readonly IMatcher _elementsMatcher;
		readonly MergeCollectionsOptions _mergeCollectionOptions;

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
			mergeCollectionsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) :
			base(elementsMapper, serviceProvider) {

			_originalElementMapper = elementsMapper;
			_elementsMatcher = elementsMatcher != null ? new SafeMatcher(elementsMatcher) : EmptyMatcher.Instance;
			_mergeCollectionOptions = mergeCollectionsOptions ?? new MergeCollectionsOptions();
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
			if (source != null && !sourceType.IsAssignableFrom(source.GetType()))
				throw new ArgumentException($"Object of type {source.GetType().FullName ?? source.GetType().Name} is not assignable to type {sourceType.FullName ?? sourceType.Name}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			// If both types are collections try mapping the element types
			if (TypeUtils.HasInterface(types.From, typeof(IEnumerable<>)) && types.From != typeof(string) &&
				TypeUtils.HasInterface(types.To, typeof(ICollection<>)) && !types.To.IsArray) {

				var elementTypes = (
					From: TypeUtils.GetInterfaceElementType(types.From, typeof(IEnumerable<>)),
					To: TypeUtils.GetInterfaceElementType(types.To, typeof(ICollection<>))
				);

				if (source is IEnumerable sourceEnumerable) {
					try {
						// If we have to create the destination collection we know that we can always map to it
						// Otherwise we must check that first
						if (destination == null) {
							try {
								destination = ObjectFactory.CreateCollection(types.To);
							}
							catch (ObjectCreationException) {
								throw new MapNotFoundException(types);
							}
						}
						else {
							// Check if the collection is not readonly recursively, if it throws it means that
							// the element mapper will be responsible for mapping the object and not collection mapper recursively
							try { 
								if(!await CanMapMerge(sourceType, destinationType, destination as IEnumerable, cancellationToken))
									throw new MapNotFoundException(types);
							}
							catch (MapNotFoundException) {
								throw;
							}
							catch {}
						}

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
							var mergeMappingOptions = mappingOptions?.GetOptions<MergeCollectionsMappingOptions>();
							if(mergeMappingOptions != null) {
								mappingOptions = new MappingOptions(mappingOptions.AsEnumerable().Select(o => {
									if (o is MergeCollectionsMappingOptions merge) {
										var mergeOpts = new MergeCollectionsMappingOptions(merge) {
											Matcher = null
										};
										return mergeOpts;
									}
									else
										return o;
								}));
							}

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

							// At least one of New or Merge mapper is required to map elements
							// If both are found they will be used in the following order:
							// - elements to update will use MergeMap first (on the existing element),
							//   then NewMap (by removing the existing element and adding the new one)
							// - elements to add will use NewMap first,
							//   then MergeMap (by creating a new element and merging to it)

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
									// Try merge map
									if (canCreateMerge) {
										try {
											var mergeResult = await _elementsMapper.MapAsync(sourceElement, elementTypes.From, matchingDestinationElement, elementTypes.To, mappingOptions, cancellationToken);
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
									elementsToAdd.Add(await _elementsMapper.MapAsync(sourceElement, elementTypes.From, elementTypes.To, mappingOptions, cancellationToken));
								}
								else {
									// Try new map
									if (canCreateNew) {
										try {
											elementsToAdd.Add(await _elementsMapper.MapAsync(sourceElement, elementTypes.From, elementTypes.To, mappingOptions, cancellationToken));
											continue;
										}
										catch (MapNotFoundException) {
											canCreateNew = false;
										}
									}

									// Try merge map
									if (!canCreateMerge)
										throw new MapNotFoundException(types);
									object destinationInstance;
									try {
										destinationInstance = ObjectFactory.Create(elementTypes.To);
									}
									catch (ObjectCreationException) {
										throw new MapNotFoundException(types);
									}
									elementsToAdd.Add(await _elementsMapper.MapAsync(sourceElement, elementTypes.From, destinationInstance, elementTypes.To, mappingOptions, cancellationToken));
								}
							}

							foreach (var element in elementsToRemove) {
								if (!(bool)removeMethod.Invoke(destination, new object[] { element }))
									throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
							}
							foreach (var element in elementsToAdd) {
								addMethod.Invoke(destination, new object[] { element });
							}

							var result = ObjectFactory.ConvertCollectionToType(destination, types.To);

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
				else if (source == null) {
					// Check if we can map elements
					try {
						// Check if we can map elements
						try {
							if (await _elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, cancellationToken))
								return null;
						}
						catch { }

						try {
							if (await _elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, cancellationToken))
								return null;
						}
						catch { }
					}
					catch { }
				}
				else
					throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
			}

			throw new MapNotFoundException(types);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region IAsyncMapperCanMap methods
		public Task<bool> CanMapAsyncNew(Type sourceType, Type destinationType, CancellationToken cancellationToken = default) {
			return Task.FromResult(false);
		}

		public Task<bool> CanMapAsyncMerge(Type sourceType, Type destinationType, CancellationToken cancellationToken = default) {
			return CanMapMerge(sourceType, destinationType, null, cancellationToken);
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		async Task<bool> CanMapMerge(Type sourceType, Type destinationType, IEnumerable destination = null, CancellationToken cancellationToken = default) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (TypeUtils.HasInterface(sourceType, typeof(IEnumerable<>)) && sourceType != typeof(string) &&
				TypeUtils.HasInterface(destinationType, typeof(ICollection<>)) && !destinationType.IsArray) {

				// Check the destination if provided
				if(destination != null) { 
					var destinationInstanceType = destination.GetType();
					if (destinationInstanceType.IsArray)
						return false;

					var interfaceMap = destinationInstanceType.GetInterfaceMap(destinationInstanceType.GetInterfaces()
						.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;

					// If the collection is readonly we cannot map to it
					if ((bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly))).Invoke(destination, null))
						return false;
				}

				var elementTypes = (
					From: TypeUtils.GetInterfaceElementType(sourceType, typeof(IEnumerable<>)),
					To: TypeUtils.GetInterfaceElementType(destinationType, typeof(IEnumerable<>))
				);

				// If the original element mapper can map the types on its own we succeed
				bool? canMapNew;
				try {
					canMapNew = await _originalElementMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, cancellationToken);
				}
				catch (InvalidOperationException) {
					canMapNew = null;
				}
				bool? canMapMerge;
				try {
					canMapMerge = await _originalElementMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, cancellationToken);
				}
				catch (InvalidOperationException) {
					canMapMerge = null;
				}

				// Otherwise we try to check if we can map the types recursively
				bool? canMapNested;
				try {
					canMapNested = await CanMapMerge(elementTypes.From, elementTypes.To, null, cancellationToken);
				}
				catch (InvalidOperationException) {
					canMapNested = null;
				}

				// If we have a destination check if all its elements can be mapped
				if((canMapNew == true || canMapMerge == true || canMapNested != false) && destination != null) {
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
