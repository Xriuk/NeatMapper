using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which merges a <see cref="IEnumerable{T}"/> (even nested) with an existing
	/// <see cref="ICollection{T}"/> (not readonly) asynchronously, will create a new <see cref="ICollection{T}"/> if destination is null.<br/>
	/// Will try to match elements of the source collection with the destination by using an <see cref="IMatcher"/> if provided:<br/>
	/// - If a match is found will try to merge the two elements or will replace with a new one by using a <see cref="IAsyncMapper"/>.<br/>
	/// - If a match is not found a new element will be added by mapping them with a <see cref="IAsyncMapper"/> by trying new map, then merge map.<br/>
	/// Not matched elements from the destination collection are treated according to <see cref="MergeCollectionsOptions"/> (and overrides).
	/// </summary>
	public sealed class AsyncMergeCollectionMapper : AsyncCollectionMapper, IAsyncMapperCanMap, IAsyncMapperFactory {
		private readonly IAsyncMapper _originalElementMapper;
		private readonly IMatcher _elementsMatcher;
		private readonly MergeCollectionsOptions _mergeCollectionOptions;
		private readonly IServiceProvider _serviceProvider;

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
		/// <param name="asyncCollectionMappersOptions">
		/// Additional parallelization options to apply during mapping, null to use default.<br/>
		/// Can be overridden during mapping with <see cref="AsyncCollectionMappersMappingOptions"/>.</param>
		/// <param name="mergeCollectionsOptions">
		/// Additional merging options to apply during mapping, null to use default.<br/>
		/// Can be overridden during mapping with <see cref="MergeCollectionsMappingOptions"/>.
		/// </param>
		/// <param name="serviceProvider">
		/// Service provider to be passed to <see cref="MergeCollectionsMappingOptions.Matcher"/>, 
		/// null to pass an empty service provider.<br/>
		/// Can be overridden during mapping with <see cref="AsyncMapperOverrideMappingOptions.ServiceProvider"/>.
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
			AsyncCollectionMappersOptions?
#else
			AsyncCollectionMappersOptions
#endif
			asyncCollectionMappersOptions = null,
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
			base(elementsMapper, asyncCollectionMappersOptions) {

			_originalElementMapper = elementsMapper;
			_elementsMatcher = elementsMatcher != null ? new SafeMatcher(elementsMatcher) : EmptyMatcher.Instance;
			_mergeCollectionOptions = mergeCollectionsOptions ?? new MergeCollectionsOptions();
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
		}


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#pragma warning disable CA1068
#endif

		private Func<object, object, Task<object>> CreateMergeFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions, CancellationToken cancellationToken, bool isRealFactory) {
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

				mappingOptions = MergeOrCreateMappingOptions(mappingOptions, isRealFactory, out var mergeMappingOptions);

				// Create parallel cancellation source and semaphore
				var parallelMappings = mappingOptions.GetOptions<AsyncCollectionMappersMappingOptions>()?.MaxParallelMappings
					?? _asyncCollectionMappersOptions.MaxParallelMappings;
				CancellationTokenSource cancellationSource;
				SemaphoreSlim semaphore;
				if (parallelMappings > 1) {
					cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
					semaphore = new SemaphoreSlim(parallelMappings);
					cancellationToken = cancellationSource.Token;
				}
				else {
					cancellationSource = null;
					// Semaphore to lock between multiple invocations of the factory to guarantee thread-safety
					semaphore = isRealFactory ? new SemaphoreSlim(1) : null;
				}

				var elementsMapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

				// At least one of New or Merge mapper is required to map elements
				// If both are found they will be used in the following order:
				// - elements to update will use MergeMap first (on the existing element),
				//   then NewMap (by removing the existing element and adding the new one)
				// - elements to add will use NewMap first,
				//   then MergeMap (by creating a new element and merging to it)
				Func<object, Task<object>> newElementsFactory;
				try {
					newElementsFactory = elementsMapper.MapAsyncNewFactory(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken);
				}
				catch (MapNotFoundException) {
					newElementsFactory = null;
				}
				Func<object, object, Task<object>> mergeElementsFactory;
				Func<object> destinationFactory = null;
				try {
					mergeElementsFactory = elementsMapper.MapAsyncMergeFactory(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken);
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

				// Create the matcher factory (it will always be created because of SafeMatcher)
				IMatcher elementsMatcher;
				if (mergeMappingOptions?.Matcher != null)
					elementsMatcher = new SafeMatcher(new DelegateMatcher(mergeMappingOptions.Matcher, _elementsMatcher, _serviceProvider));
				else
					elementsMatcher = _elementsMatcher;
				var elementsMatcherFactory = elementsMatcher.MatchFactory(elementTypes.From, elementTypes.To, mappingOptions);

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
				var collectionConversion = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType ?? types.To);

				var removeNotMatchedDestinationElements = mergeMappingOptions?.RemoveNotMatchedDestinationElements
					?? _mergeCollectionOptions.RemoveNotMatchedDestinationElements;

				return async (source, destination) => {
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
								if (!await CanMapMerge(sourceType, destinationType, destination as IEnumerable, mappingOptions, cancellationToken))
									throw new MapNotFoundException(types);
							}
							catch (MapNotFoundException) {
								throw;
							}
							catch { }
						}

						if (destination is IEnumerable destinationEnumerable) {
							var destinationInstanceType = destination.GetType();
							if (destinationInstanceType.IsArray)
								throw new MapNotFoundException(types);

							var interfaceMap = destinationInstanceType.GetInterfaceMap(destinationInstanceType.GetInterfaces()
								.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;

							var addMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Add)));
							var removeMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Remove)));

							var elementsToRemove = new List<object>();
							var elementsToAdd = new List<object>();

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
								if (sourceEnumerable.Cast<object>().Any()) {
									var sourceDestinationMatches = sourceEnumerable
										.Cast<object>()
										.Select(sourceElement => {
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
											return (SourceElement: sourceElement, Found: found, MatchingDestinationElement: matchingDestinationElement);
										});

									// Check if we need to enable parallel mapping or not
									if (parallelMappings > 1) {
										// Create and await all the tasks
										// We group by found destination element because multiple source elements could match with the same destination element
										var tasks = sourceDestinationMatches
											.Select(sourceMapping => (sourceMapping.Found, sourceMapping.MatchingDestinationElement, Result: Task.Run<object>(async () => {
												await semaphore.WaitAsync(cancellationToken);
												try {
													Func<object, Task<object>> newElementsFactoryLocal;
													Func<object, object, Task<object>> mergeElementsFactoryLocal;
													if (sourceMapping.Found) {
														// Try merge map
														mergeElementsFactoryLocal = Interlocked.CompareExchange(ref mergeElementsFactory, null, null);
														if (mergeElementsFactoryLocal != null) {
															try {
																return await mergeElementsFactoryLocal.Invoke(sourceMapping.SourceElement, sourceMapping.MatchingDestinationElement);
															}
															catch (MapNotFoundException) {
																Interlocked.CompareExchange(ref mergeElementsFactory, null, mergeElementsFactoryLocal);
															}
														}

														// Try new map
														newElementsFactoryLocal = Interlocked.CompareExchange(ref newElementsFactory, null, null);
														if(newElementsFactoryLocal == null)
															throw new MapNotFoundException(types);
														try {
															return await newElementsFactoryLocal.Invoke(sourceMapping.SourceElement);
														}
														catch (MapNotFoundException) {
															throw new MapNotFoundException(types);
														}
													}
													else {
														// Try new map
														newElementsFactoryLocal = Interlocked.CompareExchange(ref newElementsFactory, null, null);
														if (newElementsFactoryLocal != null) {
															try {
																return await newElementsFactoryLocal.Invoke(sourceMapping.SourceElement);
															}
															catch (MapNotFoundException) {
																Interlocked.CompareExchange(ref newElementsFactory, null, newElementsFactoryLocal);
															}
														}

														// Try merge map
														if(destinationFactory == null)
															throw new MapNotFoundException(types);
														mergeElementsFactoryLocal = Interlocked.CompareExchange(ref mergeElementsFactory, null, null);
														if (mergeElementsFactoryLocal == null)
															throw new MapNotFoundException(types);
														try {
															return await mergeElementsFactoryLocal.Invoke(sourceMapping.SourceElement, destinationFactory.Invoke());
														}
														catch (MapNotFoundException) {
															throw new MapNotFoundException(types);
														}
													}
												}
												finally {
													semaphore.Release();
												}
											}, cancellationToken)))
											.ToArray();

										// This groups found elements together by the destination, while not found are kept singularly
										var groupedTasks = tasks
											.GroupBy(task => (task.Found, task.MatchingDestinationElement))
											.SelectMany(sourceGroup => sourceGroup.Key.Found ?
												(IEnumerable<IEnumerable<(bool Found, object MatchingDestinationElement, Task<object> Result)>>)new[] { sourceGroup } :
												sourceGroup.Select(e => new[] { e }))
											.Select(sourceGroup => sourceGroup.ToArray())
											.ToArray();
										var nestedTasks = groupedTasks
											.Select(innerTasks => Task.Run<object>(async () => {
												foreach (var innerTask in innerTasks) {
													await innerTask.Result;
												}
												return null;
											}, cancellationToken))
											.ToArray();
										try {
											await TaskUtils.WhenAllFailFast(nestedTasks);
										}
										catch {
											// Cancel all the tasks
											cancellationSource.Cancel();

											throw;
										}

										// Process results
										foreach (var (found, matchingDestinationElement, resultTask) in tasks) {
											var destinationElement = resultTask.Result;
											if (found) {
												if (matchingDestinationElement != destinationElement) {
													elementsToRemove.Add(matchingDestinationElement);
													elementsToAdd.Add(destinationElement);
												}
											}
											else
												elementsToAdd.Add(destinationElement);
										}
									}
									else { 
										// Lock for thread-safety
										if(semaphore != null)
											await semaphore.WaitAsync(cancellationToken);
										try { 
											foreach (var (sourceElement, found, matchingDestinationElement) in sourceDestinationMatches.ToArray()) {
												if (found) {
													// Try merge map
													if (mergeElementsFactory != null) {
														try {
															var mergeResult = await mergeElementsFactory.Invoke(sourceElement, matchingDestinationElement);
															if (mergeResult != matchingDestinationElement) {
																elementsToRemove.Add(matchingDestinationElement);
																elementsToAdd.Add(mergeResult);
															}
															continue;
														}
														catch (MapNotFoundException) {
															mergeElementsFactory = null;
														}
													}

													// Try new map
													if (newElementsFactory == null)
														throw new MapNotFoundException(types);

													try {
														elementsToRemove.Add(matchingDestinationElement);
														elementsToAdd.Add(await newElementsFactory.Invoke(sourceElement));
													}
													catch (MapNotFoundException) {
														throw new MapNotFoundException(types);
													}
												}
												else {
													// Try new map
													if (newElementsFactory != null) {
														try {
															elementsToAdd.Add(await newElementsFactory.Invoke(sourceElement));
															continue;
														}
														catch (MapNotFoundException) {
															newElementsFactory = null;
														}
													}

													// Try merge map
													if (mergeElementsFactory == null || destinationFactory == null)
														throw new MapNotFoundException(types);

													try {
														elementsToAdd.Add(await mergeElementsFactory.Invoke(sourceElement, destinationFactory.Invoke()));
													}
													catch (MapNotFoundException) {
														throw new MapNotFoundException(types);
													}
												}
											}
										}
										finally {
											semaphore?.Release();
										}
									}
								}

								foreach (var element in elementsToRemove) {
									if (!(bool)removeMethod.Invoke(destination, new object[] { element }))
										throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
								}
								foreach (var element in elementsToAdd) {
									addMethod.Invoke(destination, new object[] { element });
								}

								result = collectionConversion.Invoke(destination);
							}
							catch (MapNotFoundException) {
								throw;
							}
							catch (TaskCanceledException) {
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
						// Check if we can map elements
						try {
							if (await elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken))
								return null;
						}
						catch { }

						try {
							if (await elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken))
								return null;
						}
						catch { }

						throw new MapNotFoundException(types);
					}
					else
						throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
				};
			}

			throw new MapNotFoundException(types);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#pragma warning restore CA1068
#nullable enable
#endif


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

			return CreateMergeFactory(sourceType, destinationType, mappingOptions, cancellationToken, false).Invoke(source, destination);
		}
		#endregion

		#region IAsyncMapperCanMap methods
		public Task<bool> CanMapAsyncNew(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return Task.FromResult(false);
		}

		public Task<bool> CanMapAsyncMerge(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return CanMapMerge(sourceType, destinationType, null, mappingOptions, cancellationToken);
		}
		#endregion

		#region IAsyncMapperFactory methods
		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?, Task<object?>
#else
			object, Task<object>
#endif
			> MapAsyncNewFactory(
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

		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?, object?, Task<object?>
#else
			object, object, Task<object>
#endif
			> MapAsyncMergeFactory(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return CreateMergeFactory(sourceType, destinationType, mappingOptions, cancellationToken, true);
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		async Task<bool> CanMapMerge(
			Type sourceType,
			Type destinationType,
			IEnumerable destination = null,
			MappingOptions mappingOptions = null,
			CancellationToken cancellationToken = default) {

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

				mappingOptions = MergeOrCreateMappingOptions(mappingOptions, false, out _);

				var elementsMapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper ?? _originalElementMapper;

				// If the original element mapper can map the types on its own we succeed
				bool? canMapNew;
				try {
					canMapNew = await elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken);
				}
				catch (InvalidOperationException) {
					canMapNew = null;
				}
				bool? canMapMerge;
				try {
					canMapMerge = await elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken);

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
					canMapNested = await CanMapMerge(elementTypes.From, elementTypes.To, null, mappingOptions, cancellationToken);
				}
				catch (InvalidOperationException) {
					canMapNested = null;
				}

				// If we have a concrete class we already checked that it's not readonly
				// Otherwise if we have a destination check if all its elements can be mapped
				if ((canMapNew == true || canMapMerge == true || canMapNested == true) && !destinationType.IsInterface)
					return true;
				else if(canMapNested == null && destination != null){
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
