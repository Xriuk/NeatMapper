using System;
using System.Collections;
using System.Collections.Generic;
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
	/// Also supports parallel mapping via <see cref="AsyncCollectionMappersOptions"/> (and overrides).
	/// </summary>
	/// <remarks>Collections are NOT mapped lazily, all source elements are evaluated during the map.</remarks>
	public sealed class AsyncNewCollectionMapper : AsyncCollectionMapper, IAsyncMapperFactory {
		// Parallel tasks:
		// https://stackoverflow.com/a/63937542/2672235


		/// <summary>
		/// Default async options.
		/// </summary>
		private readonly AsyncCollectionMappersOptions _asyncCollectionMappersOptions;


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
				base(elementsMapper) {

			_asyncCollectionMappersOptions = asyncCollectionMappersOptions ?? new AsyncCollectionMappersOptions();
		}


		#region IAsyncMapper methods
		override public bool CanMapAsyncNew(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return CanMapAsyncNewInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
		}

		override public bool CanMapAsyncMerge(
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapAsyncNewInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper))
				throw new MapNotFoundException(types);

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			// At least one of New or Merge mapper is required to map elements
			IAsyncNewMapFactory elementsFactory;
			try {
				elementsFactory = elementsMapper.MapAsyncNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				try {
					elementsFactory = elementsMapper.MapAsyncMergeFactory(elementTypes.From, elementTypes.To, mappingOptions).MapAsyncNewFactory();
				}
				catch (MapNotFoundException) {
					throw new MapNotFoundException(types);
				}
			}

			using (elementsFactory) {
				if(source == null)
					return null;

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
				Action<object, object> addDelegate;
				try {
					addDelegate = ObjectFactory.GetCollectionCustomAddDelegate(actualCollectionType);
				}
				catch (InvalidOperationException) {
					throw new MapNotFoundException(types);
				}

				// Create parallel cancellation source and semaphore if needed
				var parallelMappings = mappingOptions.GetOptions<AsyncCollectionMappersMappingOptions>()?.MaxParallelMappings
					?? _asyncCollectionMappersOptions.MaxParallelMappings;

				using (var semaphore = parallelMappings > 1 ? new SemaphoreSlim(parallelMappings) : null) {
					if (types.From.IsAsyncEnumerable()) {
						var getAsyncEnumeratorDelegate = ObjectFactory.GetAsyncEnumerableGetAsyncEnumerator(elementTypes.From);
						var moveNextAsyncDelegate = ObjectFactory.GetAsyncEnumeratorMoveNextAsync(elementTypes.From);
						var currentDelegate = ObjectFactory.GetAsyncEnumeratorCurrent(elementTypes.From);

						if (parallelMappings > 1) {
							try {
								// Create and await all the tasks
								var tasks = new List<Task<object>>();
								using (var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) {
									var asyncEnumerator = getAsyncEnumeratorDelegate.Invoke(source, cancellationSource.Token);
									try {
										while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
											var sourceElement = currentDelegate.Invoke(asyncEnumerator);

											await semaphore.WaitAsync(cancellationSource.Token);

											tasks.Add(Task.Run(async () => {
												try {
													return await elementsFactory.Invoke(sourceElement, cancellationSource.Token);
												}
												finally {
													semaphore.Release();
												}
											}, cancellationSource.Token));
										}
									}
									finally {
										await asyncEnumerator.DisposeAsync();
									}
									try {
										await TaskUtils.WhenAllFailFast(tasks);
									}
									catch (Exception e) {
										// Cancel all the tasks
										cancellationSource.Cancel();

										if (!(e is AggregateException a) || a.InnerExceptions.Count > 1)
											throw;
										else
											throw a.InnerException;
									}
								}

								// Add the results to the destination
								foreach (var task in tasks) {
									addDelegate.Invoke(destination, task.Result);
								}
							}
							catch (OperationCanceledException) {
								throw;
							}
							catch (Exception e) {
								throw new MappingException(e, types);
							}
						}
						else {
							var asyncEnumerator = getAsyncEnumeratorDelegate.Invoke(source, cancellationToken);
							try {
								while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
									var sourceElement = currentDelegate.Invoke(asyncEnumerator);
									addDelegate.Invoke(destination, await elementsFactory.Invoke(sourceElement, cancellationToken));
								}
							}
							finally {
								await asyncEnumerator.DisposeAsync();
							}
						}
					}
					else {
						if (source is IEnumerable sourceEnumerable) {
							if(parallelMappings > 1) {
								try { 
									// Create and await all the tasks
									var tasks = new List<Task<object>>();
									using (var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) {
										foreach (var sourceElement in sourceEnumerable) {
											await semaphore.WaitAsync(cancellationSource.Token);

											tasks.Add(Task.Run(async () => {
												try {
													return await elementsFactory.Invoke(sourceElement, cancellationSource.Token);
												}
												finally {
													semaphore.Release();
												}
											}, cancellationSource.Token));
										}
										try {
											await TaskUtils.WhenAllFailFast(tasks);
										}
										catch (Exception e) {
											// Cancel all the tasks
											cancellationSource.Cancel();

											if (!(e is AggregateException a) || a.InnerExceptions.Count > 1)
												throw;
											else
												throw a.InnerException;
										}
									}

									// Add the results to the destination
									foreach (var task in tasks) {
										addDelegate.Invoke(destination, task.Result);
									}
								}
								catch (OperationCanceledException) {
									throw;
								}
								catch (Exception e) {
									throw new MappingException(e, types);
								}
							}
							else {
								try {
									foreach (var sourceElement in sourceEnumerable) {
										addDelegate.Invoke(destination, await elementsFactory.Invoke(sourceElement, cancellationToken));
									}
								}
								catch (OperationCanceledException) {
									throw;
								}
								catch (Exception e) {
									throw new MappingException(e, types);
								}
							}
						}
						else
							throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
					}
				}

				var result = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType, types.To).Invoke(destination);

				// Should not happen
				TypeUtils.CheckObjectType(result, types.To);

				return result;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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

			if(!CanMapAsyncNewInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper))
				throw new MapNotFoundException(types);

			// At least one of New or Merge mapper is required to map elements
			IAsyncNewMapFactory elementsFactory;
			try {
				elementsFactory = elementsMapper.MapAsyncNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				try {
					elementsFactory = elementsMapper.MapAsyncMergeFactory(elementTypes.From, elementTypes.To, mappingOptions).MapAsyncNewFactory();
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
				Action<object, object> addDelegate;
				try {
					addDelegate = ObjectFactory.GetCollectionCustomAddDelegate(actualCollectionType);
				}
				catch (InvalidOperationException) {
					throw new MapNotFoundException(types);
				}

				var collectionConversionDelegate = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType, types.To);

				// Create parallel cancellation source and semaphore if needed
				var parallelMappings = mappingOptions.GetOptions<AsyncCollectionMappersMappingOptions>()?.MaxParallelMappings
					?? _asyncCollectionMappersOptions.MaxParallelMappings;

				if (types.From.IsAsyncEnumerable()) {
					var getAsyncEnumeratorDelegate = ObjectFactory.GetAsyncEnumerableGetAsyncEnumerator(elementTypes.From);
					var moveNextAsyncDelegate = ObjectFactory.GetAsyncEnumeratorMoveNextAsync(elementTypes.From);
					var currentDelegate = ObjectFactory.GetAsyncEnumeratorCurrent(elementTypes.From);

					if(parallelMappings > 1) {
						var semaphore = new SemaphoreSlim(parallelMappings);

						try { 
							return new DisposableAsyncNewMapFactory(
								sourceType, destinationType,
								async (source, cancellationToken) => {
									TypeUtils.CheckObjectType(source, types.From, nameof(source));

									if (source == null)
										return null;
									else {
										object result;
										try {
											// Create and await all the tasks
											var tasks = new List<Task<object>>();
											using (var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) {
												var asyncEnumerator = getAsyncEnumeratorDelegate.Invoke(source, cancellationSource.Token);
												try {
													while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
														var sourceElement = currentDelegate.Invoke(asyncEnumerator);

														await semaphore.WaitAsync(cancellationSource.Token);

														tasks.Add(Task.Run(async () => {
															try {
																return await elementsFactory.Invoke(sourceElement, cancellationSource.Token);
															}
															finally {
																semaphore.Release();
															}
														}, cancellationSource.Token));
													}
												}
												finally {
													await asyncEnumerator.DisposeAsync();
												}
												try {
													await TaskUtils.WhenAllFailFast(tasks);
												}
												catch (Exception e) {
													// Cancel all the tasks
													cancellationSource.Cancel();

													if (!(e is AggregateException a) || a.InnerExceptions.Count > 1)
														throw;
													else
														throw a.InnerException;
												}
											}

											var destination = collectionFactory.Invoke();

											// Add the results to the destination
											foreach (var task in tasks) {
												addDelegate.Invoke(destination, task.Result);
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
								},
								elementsFactory, semaphore);
						}
						catch {
							semaphore.Dispose();
							throw;
						}
					}
					else { 
						return new DisposableAsyncNewMapFactory(
							sourceType, destinationType,
							async (source, cancellationToken) => {
								TypeUtils.CheckObjectType(source, types.From, nameof(source));

								if (source == null) 
									return null;
								else {
									object result;
									try {
										var destination = collectionFactory.Invoke();

										var asyncEnumerator = getAsyncEnumeratorDelegate.Invoke(source, cancellationToken);
										try {
											while (await moveNextAsyncDelegate.Invoke(asyncEnumerator)) {
												var sourceElement = currentDelegate.Invoke(asyncEnumerator);
												addDelegate.Invoke(destination, await elementsFactory.Invoke(sourceElement, cancellationToken));
											}
										}
										finally {
											await asyncEnumerator.DisposeAsync();
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
							},
							elementsFactory);
					}
				}
				else {
					if (parallelMappings > 1) {
						var semaphore = new SemaphoreSlim(parallelMappings);

						try {
							return new DisposableAsyncNewMapFactory(
								sourceType, destinationType,
								async (source, cancellationToken) => {
									TypeUtils.CheckObjectType(source, types.From, nameof(source));

									if (source is IEnumerable sourceEnumerable) {
										object result;
										try {
											// Create and await all the tasks
											var tasks = new List<Task<object>>();
											using (var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) {
												foreach(var sourceElement in sourceEnumerable) {
													await semaphore.WaitAsync(cancellationSource.Token);

													tasks.Add(Task.Run(async () => {
														try {
															return await elementsFactory.Invoke(sourceElement, cancellationSource.Token);
														}
														finally {
															semaphore.Release();
														}
													}, cancellationSource.Token));
												}
												try {
													await TaskUtils.WhenAllFailFast(tasks);
												}
												catch (Exception e) {
													// Cancel all the tasks
													cancellationSource.Cancel();

													if (!(e is AggregateException a) || a.InnerExceptions.Count > 1)
														throw;
													else
														throw a.InnerException;
												}
											}

											var destination = collectionFactory.Invoke();

											// Add the results to the destination
											foreach (var task in tasks) {
												addDelegate.Invoke(destination, task.Result);
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
								elementsFactory, semaphore);
						}
						catch {
							semaphore.Dispose();
							throw;
						}
					}
					else {
						return new DisposableAsyncNewMapFactory(
							sourceType, destinationType,
							async (source, cancellationToken) => {
								TypeUtils.CheckObjectType(source, types.From, nameof(source));

								if (source is IEnumerable sourceEnumerable) {
									object result;
									try {
										var destination = collectionFactory.Invoke();

										foreach (var sourceElement in sourceEnumerable) {
											addDelegate.Invoke(destination, await elementsFactory.Invoke(sourceElement, cancellationToken));
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
				}
			}
			catch {
				elementsFactory?.Dispose();
				throw;
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


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private bool CanMapAsyncNewInternal(
			Type sourceType,
			Type destinationType,
			ref MappingOptions mappingOptions,
			out (Type From, Type To) elementTypes,
			out IAsyncMapper elementsMapper) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if ((sourceType.IsEnumerable() || sourceType.IsAsyncEnumerable()) &&
				(destinationType.IsEnumerable() || destinationType.IsAsyncEnumerable()) &&
				ObjectFactory.CanCreateCollection(destinationType)) {

				elementTypes = (From: sourceType.IsEnumerable() ? sourceType.GetEnumerableElementType() : sourceType.GetAsyncEnumerableElementType(),
					To: destinationType.IsEnumerable() ? destinationType.GetEnumerableElementType() : destinationType.GetAsyncEnumerableElementType());
				mappingOptions = _optionsCache.GetOrCreate(mappingOptions);
				elementsMapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

				return elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions) ||
					(ObjectFactory.CanCreate(elementTypes.To) && elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions));
			}
			else {
				elementTypes = default;
				elementsMapper = null;

				return false;
			}
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
