using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which creates a new <see cref="System.Collections.Generic.IEnumerable{T}"/>
	/// (even nested) from a <see cref="System.Collections.Generic.IEnumerable{T}"/> asynchronously
	/// and maps elements with another <see cref="IAsyncMapper"/> by trying new map first, then merge map
	/// </summary>
	public sealed class AsyncNewCollectionMapper : AsyncCollectionMapper, IAsyncMapperCanMap {
		public AsyncNewCollectionMapper(
			IAsyncMapper elementsMapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			AsyncCollectionMappersOptions?
#else
			AsyncCollectionMappersOptions
#endif
			asyncCollectionMappersOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) :
			base(elementsMapper, asyncCollectionMappersOptions, serviceProvider) { }


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
			if (types.From.IsEnumerable() && types.To.IsEnumerable()) {
				var elementTypes = (From: types.From.GetEnumerableElementType(), To: types.To.GetEnumerableElementType());

				// Check if collection can be created
				if (ObjectFactory.CanCreateCollection(types.To)) {
					try {
						if (source is IEnumerable sourceEnumerable) {
							var destination = ObjectFactory.CreateCollection(types.To);
							
							var enumerable = sourceEnumerable.Cast<object>();
							if(enumerable.Any()) {
								var addMethod = ObjectFactory.GetCollectionAddMethod(destination);

								mappingOptions = MergeOrCreateMappingOptions(mappingOptions, out _);

								var elementsMapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

								var parallelMappings = mappingOptions.GetOptions<AsyncCollectionMappersMappingOptions>()?.MaxParallelMappings
									?? _asyncCollectionMappersOption.MaxParallelMappings;

								// Check if we need to enable parallel mapping or not
								if(parallelMappings > 1) { 
									// We do an initial check for the mapping capabilities, to avoid firing up many tasks
									// which could potentially fail while causing side-effects

									// Check if new map could be used
									// -1: unknown (must be checked at runtime), 0: false, 1: true
									int canMapNew;
									try{
										canMapNew = (await elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken)) ? 1 : 0;
									}
									catch (InvalidOperationException) {
										canMapNew = -1;
									}

									// Check if merge map could be used
									if(canMapNew != 1 && ObjectFactory.CanCreate(elementTypes.To)) { 
										try {
											if(await elementsMapper.CanMapAsyncNew(elementTypes.From, elementTypes.To, mappingOptions, cancellationToken))
												canMapNew = 0;
											else if(canMapNew == 0)
												throw new MapNotFoundException(types);
										}
										catch (InvalidOperationException) {
											if (canMapNew == 0)
												throw new MapNotFoundException(types);
										}
									}

									// Create and await all the tasks
									var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
									var semaphore = new SemaphoreSlim(parallelMappings);
									var tasks = enumerable
										.Select(sourceElement => Task.Run(async () => {
											await semaphore.WaitAsync(cancellationSource.Token);
											try {
												object destinationElement;

												// Try new map
												if (Interlocked.CompareExchange(ref canMapNew, 0, 0) != 0) {
													try {
														return await elementsMapper.MapAsync(sourceElement, elementTypes.From, elementTypes.To, mappingOptions, cancellationSource.Token);
													}
													catch (MapNotFoundException) {
														Interlocked.CompareExchange(ref canMapNew, 0, -1);
													}
												}

												// Try merge map
												try {
													destinationElement = ObjectFactory.Create(elementTypes.To);
												}
												catch (ObjectCreationException) {
													throw new MapNotFoundException(types);
												}
												return await elementsMapper.MapAsync(sourceElement, elementTypes.From, destinationElement, elementTypes.To, mappingOptions, cancellationSource.Token);
											}
											finally {
												semaphore.Release();
											}
										}, cancellationSource.Token))
										.ToArray();
									try {
										await TaskUtils.WhenAllFailFast(tasks);
									}
									catch{
										// Cancel all the tasks
										cancellationSource.Cancel();

										throw;
									}

									// Add the results to the destination
									foreach(var task in tasks) {
										addMethod.Invoke(destination, new object[] { task.Result });
									}
								}
								else {
									bool canCreateNew = true;
									foreach (var element in sourceEnumerable) {
										object destinationElement;

										// Try new map
										if (canCreateNew) {
											try {
												destinationElement = await elementsMapper.MapAsync(element, elementTypes.From, elementTypes.To, mappingOptions, cancellationToken);
												addMethod.Invoke(destination, new object[] { destinationElement });
												continue;
											}
											catch (MapNotFoundException) {
												canCreateNew = false;
											}
										}

										// Try merge map
										try {
											destinationElement = ObjectFactory.Create(elementTypes.To);
										}
										catch (ObjectCreationException) {
											throw new MapNotFoundException(types);
										}
										destinationElement = await elementsMapper.MapAsync(element, elementTypes.From, destinationElement, elementTypes.To, mappingOptions, cancellationToken);
										addMethod.Invoke(destination, new object[] { destinationElement });
									}
								}
							}

							var result = ObjectFactory.ConvertCollectionToType(destination, types.To);

							// Should not happen
							if (result != null && !destinationType.IsAssignableFrom(result.GetType()))
								throw new InvalidOperationException($"Object of type {result.GetType().FullName ?? result.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}");

							return result;
						}
						else if (source == null) {
							var elementsMapper = mappingOptions?.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

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
						}
						else
							throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
					}
					catch (MapNotFoundException) {
						throw;
					}
					catch (Exception e) {
						throw new MappingException(e, types);
					}
				}
			}

			throw new MapNotFoundException(types);

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

				mappingOptions = MergeOrCreateMappingOptions(mappingOptions, out _);

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
	}
}
