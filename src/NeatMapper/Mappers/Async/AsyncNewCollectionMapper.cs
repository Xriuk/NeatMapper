using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which creates a new <see cref="System.Collections.Generic.IEnumerable{T}"/>
	/// (even nested) from a <see cref="System.Collections.Generic.IEnumerable{T}"/> asynchronously
	/// and maps elements with another <see cref="IAsyncMapper"/> by trying new map first, then merge map.
	/// </summary>
	public sealed class AsyncNewCollectionMapper : AsyncCollectionMapper, IAsyncMapperCanMap, IAsyncMapperFactory {
		/// <inheritdoc cref="AsyncNewCollectionMapper(IAsyncMapper, AsyncCollectionMappersOptions)"/>
		[Obsolete("serviceProvider parameter is no longer used and will be removed in future versions, use other overloads.")]
		public AsyncNewCollectionMapper(
			IAsyncMapper elementsMapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			AsyncCollectionMappersOptions?
#else
			AsyncCollectionMappersOptions
#endif
			asyncCollectionMappersOptions,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider) :
			base(elementsMapper, asyncCollectionMappersOptions) { }

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
			base(elementsMapper, asyncCollectionMappersOptions) { }


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#pragma warning disable CA1068
#endif

		private Func<object, Task<object>> CreateNewFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions, CancellationToken cancellationToken, bool isRealFactory) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			// If both types are collections try mapping the element types
			if (types.From.IsEnumerable() && types.To.IsEnumerable() && ObjectFactory.CanCreateCollection(types.To)) {
				var elementTypes = (From: types.From.GetEnumerableElementType(), To: types.To.GetEnumerableElementType());

				mappingOptions = MergeOrCreateMappingOptions(mappingOptions, isRealFactory, out _);

				// Create parallel cancellation source and semaphore
				var parallelMappings = mappingOptions.GetOptions<AsyncCollectionMappersMappingOptions>()?.MaxParallelMappings
					?? _asyncCollectionMappersOptions.MaxParallelMappings;
				CancellationTokenSource cancellationSource;
				SemaphoreSlim semaphore;
				if(parallelMappings > 1) {
					cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
					semaphore = new SemaphoreSlim(parallelMappings);
					cancellationToken = cancellationSource.Token;
				}
				else { 
					cancellationSource = null;
					semaphore = null;
				}

				var elementsMapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

				// Try new map
				Func<object, Task<object>> elementsFactory;
				try {
					elementsFactory = elementsMapper.MapAsyncNewFactory(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken);
				}
				catch (MapNotFoundException) {
					if (!ObjectFactory.CanCreate(elementTypes.To))
						throw;

					// Try merge map
					Func<object, object, Task<object>> mergeFactory;
					try {
						mergeFactory = elementsMapper.MapAsyncMergeFactory(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken);
					}
					catch (MapNotFoundException) {
						throw new MapNotFoundException(types);
					}
					Func<object> destinationFactory;
					try {
						destinationFactory = ObjectFactory.CreateFactory(elementTypes.To);
					}
					catch (ObjectCreationException) {
						throw new MapNotFoundException(types);
					}
					elementsFactory = source => mergeFactory.Invoke(source, destinationFactory.Invoke());
				}

				Func<object> collectionFactory;
				Type actualCollectionType;
				try {
					collectionFactory = ObjectFactory.CreateCollectionFactory(types.To, out actualCollectionType);
				}
				catch (ObjectCreationException) {
					throw new MapNotFoundException(types);
				}
				var addMethod = ObjectFactory.GetCollectionAddMethod(actualCollectionType);
				var collectionConversion = ObjectFactory.CreateCollectionConversionFactory(types.To);

				return async source => {
					TypeUtils.CheckObjectType(source, types.From, nameof(source));

					if (source is IEnumerable sourceEnumerable) {
						object result;
						try {
							var destination = collectionFactory.Invoke();

							// Check if we need to enable parallel mapping or not
							if (parallelMappings > 1 && sourceEnumerable.Cast<object>().Any()) {
								// Create and await all the tasks
								var tasks = sourceEnumerable
									.Cast<object>()
									.Select(sourceElement => Task.Run(async () => {
										await semaphore.WaitAsync(cancellationToken);
										try {
											return await elementsFactory.Invoke(sourceElement);
										}
										catch (MapNotFoundException) {
											throw new MapNotFoundException(types);
										}
										finally {
											semaphore.Release();
										}
									}, cancellationToken))
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
									addMethod.Invoke(destination, new object[] { task.Result });
								}
							}
							else { 
								foreach (var sourceElement in sourceEnumerable) {
									try {
										var destinationElement = await elementsFactory.Invoke(sourceElement);
										addMethod.Invoke(destination, new object[] { destinationElement });
									}
									catch (MapNotFoundException) {
										throw new MapNotFoundException(types);
									}
								}
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
					else if (source == null) {
						// Check if we can map elements
						try {
							if (await elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions))
								return null;
						}
						catch { }

						try {
							if (await elementsMapper.CanMapAsyncMerge(elementTypes.From, elementTypes.To, mappingOptions))
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

			return CreateNewFactory(sourceType, destinationType, mappingOptions, cancellationToken, false).Invoke(source);
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

			if (sourceType.IsEnumerable() && destinationType.IsEnumerable() && ObjectFactory.CanCreateCollection(destinationType)) {
				var elementTypes = (From: sourceType.GetEnumerableElementType(), To: destinationType.GetEnumerableElementType());

				mappingOptions = MergeOrCreateMappingOptions(mappingOptions, false, out _);

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

			return CreateNewFactory(sourceType, destinationType, mappingOptions, cancellationToken, true);
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

			// Not mapping merge
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion
	}
}
