using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which creates a new <see cref="IEnumerable{T}"/> (derived from <see cref="ICollection{T}"/>
	/// plus some special types like below) or <see cref="IAsyncEnumerable{T}"/> (even nested and readonly),
	/// from another <see cref="IEnumerable{T}"/> or <see cref="IAsyncEnumerable{T}"/>, asynchronously.<br/>
	/// Elements are then mapped with another <see cref="IAsyncMapper"/> by trying new map first, then merge map.<br/>
	/// Special collections which can be created are:
	/// <list type="bullet">
	/// <item><see cref="Stack{T}"/></item>
	/// <item><see cref="Queue{T}"/></item>
	/// <item><see cref="string"/> (considered as a collection of <see cref="char"/>s)</item>
	/// </list>
	/// </summary>
	/// <remarks>Collections are NOT mapped lazily, all source elements are evaluated during the map.</remarks>
	public sealed class AsyncNewCollectionMapper : AsyncCollectionMapper, IAsyncMapperCanMap, IAsyncMapperFactory {
		/// <summary>
		/// Creates a new instance of <see cref="AsyncNewCollectionMapper"/>.
		/// </summary>
		/// <param name="elementsMapper">
		/// <see cref="IAsyncMapper"/> to use to map collection elements.<br/>
		/// Can be overridden during mapping with <see cref="AsyncMapperOverrideMappingOptions"/>.
		/// </param>
		/// <param name="asyncCollectionMappersOptions">
		/// Additional parallelization options to apply during mapping, null to use default.<br/>
		/// Can be overridden during mapping with <see cref="AsyncCollectionMappersMappingOptions"/>.
		/// </param>
		public AsyncNewCollectionMapper(
			IAsyncMapper elementsMapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			AsyncCollectionMappersOptions?
#else
			AsyncCollectionMappersOptions
#endif
			asyncCollectionMappersOptions = null) :
			base(elementsMapper, asyncCollectionMappersOptions != null ? new AsyncCollectionMappersOptions(asyncCollectionMappersOptions) : null) { }


		#region IAsyncMapper methods
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
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			using (var factory = MapAsyncNewFactory(sourceType, destinationType, mappingOptions)) {
				return await factory.Invoke(source, cancellationToken);
			}
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

			// Not mapping merge
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion

		#region IAsyncMapperCanMap methods
		public async Task<bool> CanMapAsyncNew(
			Type sourceType,
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
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if ((sourceType.IsEnumerable() || sourceType.IsAsyncEnumerable()) &&
				(destinationType.IsEnumerable() || destinationType.IsAsyncEnumerable()) &&
				ObjectFactory.CanCreateCollection(destinationType)) {

				var elementTypes = (From: sourceType.IsEnumerable() ? sourceType.GetEnumerableElementType() : sourceType.GetAsyncEnumerableElementType(),
					To: destinationType.IsEnumerable() ? destinationType.GetEnumerableElementType() : destinationType.GetAsyncEnumerableElementType());

				mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

				var elementsMapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

				bool cannotVerifyNew = false;
				try {
					if(await elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken))
						return true;
				}
				catch(InvalidOperationException) {
					cannotVerifyNew = true;
				}

				if (ObjectFactory.CanCreate(elementTypes.To) && await elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken))
					return true;
				else if(cannotVerifyNew)
					throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
			}

			return false;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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

			return Task.FromResult(false);
		}
		#endregion

		#region IAsyncMapperFactory methods
		public IAsyncNewMapFactory MapAsyncNewFactory(
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
			if ((types.From.IsEnumerable() || types.From.IsAsyncEnumerable()) &&
				(types.To.IsEnumerable() || types.To.IsAsyncEnumerable()) &&
				ObjectFactory.CanCreateCollection(types.To)) {

				var elementTypes = (From: types.From.IsEnumerable() ? types.From.GetEnumerableElementType() : types.From.GetAsyncEnumerableElementType(),
					To: types.To.IsEnumerable() ? types.To.GetEnumerableElementType() : types.To.GetAsyncEnumerableElementType());

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

				mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

				var elementsMapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

				// At least one of New or Merge mapper is required to map elements
				// If both are found they will be used in the following order: NewMap then MergeMap
				IAsyncNewMapFactory newElementsFactory;
				try {
					newElementsFactory = elementsMapper.MapAsyncNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
				}
				catch (MapNotFoundException) {
					newElementsFactory = null;
				}

				try { 
					IAsyncNewMapFactory mergeElementsFactory;
					try {
						mergeElementsFactory = elementsMapper.MapAsyncMergeFactory(elementTypes.From, elementTypes.To, mappingOptions).MapAsyncNewFactory();
					}
					catch (MapNotFoundException) {
						if (newElementsFactory == null)
							throw;
						else
							mergeElementsFactory = null;
					}

					try {
						if (types.From.IsAsyncEnumerable()) {
							var getAsyncEnumeratorDelegate = ObjectFactory.GetAsyncEnumerableGetAsyncEnumerator(elementTypes.From);
							var moveNextAsyncDelegate = ObjectFactory.GetAsyncEnumeratorMoveNextAsync(elementTypes.From);

							return new DisposableAsyncNewMapFactory(
								sourceType, destinationType,
								async (source, cancellationToken) => {
									TypeUtils.CheckObjectType(source, types.From, nameof(source));

									if (source == null) {
										// DEV: maybe remove because we already know that we could map the types,
										// find edge-cases + tests
										// Check if we can map elements
										try {
											if (await elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken))
												return null;
										}
										catch { }

										try {
											if (await elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken) &&
												ObjectFactory.CanCreate(elementTypes.To)) {

												return null;
											}
										}
										catch { }

										throw new MapNotFoundException(types);
									}
									else {
										var canNew = newElementsFactory != null;

										object result;
										try {
											var destination = collectionFactory.Invoke();

											var asyncEnumerator = getAsyncEnumeratorDelegate.Invoke(source, cancellationToken);
											try {
												while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
													var sourceElement = asyncEnumerator.GetType().GetProperty(nameof(IAsyncEnumerator<object>.Current))
														.GetValue(asyncEnumerator);

													// Try new map
													if (canNew) {
														try {
															addDelegate.Invoke(destination, await newElementsFactory.Invoke(sourceElement, cancellationToken));
															continue;
														}
														catch (MapNotFoundException) {
															canNew = false;
														}
													}

													// Try merge map
													if (mergeElementsFactory == null)
														throw new MapNotFoundException(types);
													try {
														addDelegate.Invoke(destination, await mergeElementsFactory.Invoke(sourceElement, cancellationToken));
													}
													catch (MapNotFoundException) {
														throw new MapNotFoundException(types);
													}
												}
											}
											finally {
												await asyncEnumerator.DisposeAsync();
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
								},
								newElementsFactory, mergeElementsFactory);
						}
						else {
							// Create parallel cancellation source and semaphore if needed
							var parallelMappings = mappingOptions.GetOptions<AsyncCollectionMappersMappingOptions>()?.MaxParallelMappings
								?? _asyncCollectionMappersOptions.MaxParallelMappings;
							
							if (parallelMappings > 1) {
								var semaphore = parallelMappings > 1 ? new SemaphoreSlim(parallelMappings) : null;
								
								try {
									return new DisposableAsyncNewMapFactory(
										sourceType, destinationType,
										async (source, cancellationToken) => {
											TypeUtils.CheckObjectType(source, types.From, nameof(source));

											if(source is IEnumerable sourceEnumerable){
												var canNew = newElementsFactory != null ? 1 : 0;

												object result;
												try {
													var destination = collectionFactory.Invoke();

													// Create and await all the tasks
													using(var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) { 
														var tasks = sourceEnumerable
															.Cast<object>()
															.Select(sourceElement => Task.Run(async () => {
																await semaphore.WaitAsync(cancellationSource.Token);
																try {
																	// Try new map
																	if (Interlocked.CompareExchange(ref canNew, 0, 0) == 1) {
																		try {
																			return await newElementsFactory.Invoke(sourceElement, cancellationSource.Token);
																		}
																		catch (MapNotFoundException) {
																			Interlocked.CompareExchange(ref canNew, 0, 1);
																		}
																	}

																	// Try merge map
																	if (mergeElementsFactory == null)
																		throw new MapNotFoundException(types);
																	try {
																		return await mergeElementsFactory.Invoke(sourceElement, cancellationSource.Token);
																	}
																	catch (MapNotFoundException) {
																		throw new MapNotFoundException(types);
																	}
																}
																finally {
																	semaphore.Release();
																}
															}, cancellationSource.Token))
															.ToArray();
														try {
															await TaskUtils.WhenAllFailFast(tasks);
														}
														catch {
															// Cancel all the tasks
															cancellationSource.Cancel();

															throw;
														}

														// Add the results to the destination
														foreach (var task in tasks) {
															addDelegate.Invoke(destination, task.Result);
														}
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
											else if (source == null) {
												// DEV: maybe remove because we already know that we could map the types,
												// find edge-cases + tests
												// Check if we can map elements
												try {
													if (await elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken))
														return null;
												}
												catch { }

												try {
													if (await elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken) &&
														ObjectFactory.CanCreate(elementTypes.To)) {

														return null;
													}
												}
												catch { }

												throw new MapNotFoundException(types);
											}
											else
												throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
										},
										semaphore, newElementsFactory, mergeElementsFactory);
								}
								catch {
									semaphore?.Dispose();
									throw;
								}
							}
							else {
								return new DisposableAsyncNewMapFactory(
									sourceType, destinationType,
									async (source, cancellationToken) => {
										TypeUtils.CheckObjectType(source, types.From, nameof(source));

										if (source is IEnumerable sourceEnumerable) {
											var canNew = newElementsFactory != null;

											object result;
											try {
												var destination = collectionFactory.Invoke();

												foreach (var sourceElement in sourceEnumerable) {
													// Try new map
													if (canNew) {
														try {
															addDelegate.Invoke(destination, await newElementsFactory.Invoke(sourceElement, cancellationToken));
															continue;
														}
														catch (MapNotFoundException) {
															canNew = false;
														}
													}

													// Try merge map
													if (mergeElementsFactory == null)
														throw new MapNotFoundException(types);
													try {
														addDelegate.Invoke(destination, await mergeElementsFactory.Invoke(sourceElement, cancellationToken));
													}
													catch (MapNotFoundException) {
														throw new MapNotFoundException(types);
													}
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
										else if (source == null) {
											// DEV: maybe remove because we already know that we could map the types,
											// find edge-cases + tests
											// Check if we can map elements
											try {
												if (await elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken))
													return null;
											}
											catch { }

											try {
												if (await elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken) &&
													ObjectFactory.CanCreate(elementTypes.To)) {

													return null;
												}
											}
											catch { }

											throw new MapNotFoundException(types);
										}
										else
											throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
									},
									newElementsFactory, mergeElementsFactory);
							}
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
			}

			throw new MapNotFoundException(types);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public IAsyncMergeMapFactory MapAsyncMergeFactory(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			// Not mapping merge
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion
	}
}
