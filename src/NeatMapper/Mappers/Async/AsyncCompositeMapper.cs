using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which delegates mapping to other <see cref="IAsyncMapper"/>s,
	/// this allows to combine different mapping capabilities.<br/>
	/// Each mapper is invoked in order and the first one to succeed in mapping is returned.<br/>
	/// For new maps, if no mapper can map the types a destination object is created and merge maps are tried.
	/// </summary>
	public sealed class AsyncCompositeMapper : IAsyncMapper, IAsyncMapperCanMap {
		/// <summary>
		/// List of <see cref="IMapper"/>s to be tried in order when mapping types.
		/// </summary>
		private readonly IList<IAsyncMapper> _mappers;

		/// <summary>
		/// Cached <see cref="AsyncNestedMappingContext"/> to provide, if not already provided in <see cref="MappingOptions"/>.
		/// </summary>
		private readonly AsyncNestedMappingContext _nestedMappingContext;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MappingOptions> _optionsCache = new ConcurrentDictionary<MappingOptions, MappingOptions>();

		/// <summary>
		/// Cached output <see cref="MappingOptions"/> for the <see langword="null"/> input <see cref="MappingOptions"/>
		/// (since a dictionary can't have a null key), also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		private readonly MappingOptions _optionsCacheNull;


		/// <summary>
		/// Creates the mapper by using the provided mappers list.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public AsyncCompositeMapper(params IAsyncMapper[] mappers) : this((IList<IAsyncMapper>) mappers) { }

		/// <summary>
		/// Creates the mapper by using the provided mappers list.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public AsyncCompositeMapper(IList<IAsyncMapper> mappers) {
			if (mappers == null)
				throw new ArgumentNullException(nameof(mappers));

			_mappers = new List<IAsyncMapper>(mappers);
			_nestedMappingContext = new AsyncNestedMappingContext(this);
			_optionsCacheNull = GetOrCreateMappingOptions(MappingOptions.Empty);
		}


		#region IAsyncMapper methods
		public Task<
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

			return MapInternal(_mappers, source, sourceType, destinationType, GetOrCreateMappingOptions(mappingOptions), cancellationToken);
		}

		public Task<
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

			return MapInternal(_mappers, source, sourceType, destination, destinationType, GetOrCreateMappingOptions(mappingOptions), cancellationToken);
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

			mappingOptions = GetOrCreateMappingOptions(mappingOptions);

			// Check if any mapper implements IAsyncMapperCanMap, if one of them throws it means that the map can be checked only when mapping
			var undeterminateMappers = new List<IAsyncMapper>();
			foreach (var mapper in _mappers.OfType<IAsyncMapperCanMap>()) {
				try { 
					if(await mapper.CanMapAsyncNew(sourceType, destinationType, mappingOptions, cancellationToken))
						return true;
				}
				catch (InvalidOperationException) {
					undeterminateMappers.Add(mapper);
				}
			}

			// Try creating a default source object and try mapping it
			var mappersLeft = _mappers.Where(m => !(m is IAsyncMapperCanMap) || undeterminateMappers.IndexOf(m) != -1);
			if (mappersLeft.Any()) {
				object source;
				try {
					source = ObjectFactory.GetOrCreateCached(sourceType) ?? throw new Exception(); // Just in case
				}
				catch {
					throw new InvalidOperationException("Cannot verify if the mapper supports the given map because unable to create an object to test it");
				}

				foreach (var mapper in mappersLeft) {
					try {
						await mapper.MapAsync(source, sourceType, destinationType, mappingOptions, cancellationToken);
						return true;
					}
					catch (MapNotFoundException) {}
				}
			}

			if(undeterminateMappers.Count > 0)
				throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
			else
				return false;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public async Task<bool> CanMapAsyncMerge(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = GetOrCreateMappingOptions(mappingOptions);

			// Check if any mapper implements IAsyncMapperCanMap, if one of them throws it means that the map can be checked only when mapping
			var undeterminateMappers = new List<IAsyncMapper>();
			foreach (var mapper in _mappers.OfType<IAsyncMapperCanMap>()) {
				try {
					if (await mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions, cancellationToken))
						return true;
				}
				catch (InvalidOperationException) {
					undeterminateMappers.Add(mapper);
				}
			}

			// Try creating two default source and destination objects and try mapping them
			var mappersLeft = _mappers.Where(m => !(m is IAsyncMapperCanMap) || undeterminateMappers.IndexOf(m) != -1);
			if (mappersLeft.Any()) {
				object source;
				object destination;
				try {
					source = ObjectFactory.GetOrCreateCached(sourceType) ?? throw new Exception(); // Just in case
					destination = ObjectFactory.Create(destinationType) ?? throw new Exception(); // Just in case
				}
				catch {
					throw new InvalidOperationException("Cannot verify if the mapper supports the given map because unable to create the objects to test it");
				}

				foreach (var mapper in mappersLeft) {
					try {
						await mapper.MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken);
						return true;
					}
					catch (MapNotFoundException) { }
				}
			}

			if (undeterminateMappers.Count > 0)
				throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
			else
				return false;
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
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = GetOrCreateMappingOptions(mappingOptions);

			var unavailableMappers = new ConcurrentBag<IAsyncMapper>();
			var factoriesCache = new ConcurrentBag<IAsyncNewMapFactory>();
			var enumerator = _mappers.OfType<IAsyncMapperFactory>().GetEnumerator();
			var enumeratorSemaphore = new SemaphoreSlim(1);

			// DEV: maybe check with CanMapAsync and if returns false throw instead of creating the factory?

			return new DisposableAsyncNewMapFactory(
				sourceType, destinationType,
				async source => {
					// Try using cached factories, if any
					foreach (var factory in factoriesCache) {
						try {
							return await factory.Invoke(source);
						}
						catch (MapNotFoundException) { }
					}

					// Retrieve and cache new factories, if the mappers throw while retrieving the factory
					// they can never map the given types (we assume that if it cannot provide a factory for two types,
					// it cannot even map them), otherwise they might map them and fail (and we'll retry later)
					await enumeratorSemaphore.WaitAsync(cancellationToken);
					try { 
						if (enumerator.MoveNext()) {
							while (true) {
								IAsyncNewMapFactory factory;
								try {
									factory = enumerator.Current.MapAsyncNewFactory(sourceType, destinationType, mappingOptions, cancellationToken);
								}
								catch (MapNotFoundException) {
									unavailableMappers.Add(enumerator.Current);
									if (enumerator.MoveNext())
										continue;
									else
										break;
								}

								factoriesCache.Add(factory);

								try {
									return await factory.Invoke(source);
								}
								catch (MapNotFoundException) { }

								// Since we finished the mappers, we check if any mapper left can map the types
								if (!enumerator.MoveNext()) {
									foreach (var mapper in _mappers.OfType<IAsyncMapperCanMap>()) {
										if (mapper is IAsyncMapperFactory)
											continue;

										try {
											if (!await mapper.CanMapAsyncNew(sourceType, destinationType, mappingOptions, cancellationToken))
												unavailableMappers.Add(mapper);
										}
										catch { }
									}

									break;
								}
							}
						}
					}
					finally {
						enumeratorSemaphore.Release();
					}

					// Invoke the default map if there are any mappers left
					if (unavailableMappers.Count != _mappers.Count)
						return await MapInternal(_mappers.Except(unavailableMappers), source, sourceType, destinationType, mappingOptions, cancellationToken);
					else
						throw new MapNotFoundException((sourceType, destinationType));
				},
				enumerator, enumeratorSemaphore, new LambdaDisposable(() => {
					foreach (var factory in factoriesCache) {
						factory.Dispose();
					}
				}));

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
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = GetOrCreateMappingOptions(mappingOptions);

			var unavailableMappers = new ConcurrentBag<IAsyncMapper>();
			var factoriesCache = new ConcurrentBag<IAsyncMergeMapFactory>();
			var enumerator = _mappers.OfType<IAsyncMapperFactory>().GetEnumerator();
			var enumeratorSemaphore = new SemaphoreSlim(1);

			// DEV: maybe check with CanMapAsync and if returns false throw instead of creating the factory?

			return new DisposableAsyncMergeMapFactory(
				sourceType, destinationType,
				async (source, destination) => {
					// Try using cached factories, if any
					foreach (var factory in factoriesCache) {
						try {
							return await factory.Invoke(source, destination);
						}
						catch (MapNotFoundException) { }
					}

					// Retrieve and cache new factories, if the mappers throw while retrieving the factory
					// they can never map the given types (we assume that if it cannot provide a factory for two types,
					// it cannot even map them), otherwise they might map them and fail (and we'll retry later)
					await enumeratorSemaphore.WaitAsync(cancellationToken);
					try {
						if (enumerator.MoveNext()) {
							while (true) {
								IAsyncMergeMapFactory factory;
								try {
									factory = enumerator.Current.MapAsyncMergeFactory(sourceType, destinationType, mappingOptions, cancellationToken);
								}
								catch (MapNotFoundException) {
									unavailableMappers.Add(enumerator.Current);
									if (enumerator.MoveNext())
										continue;
									else
										break;
								}

								factoriesCache.Add(factory);

								try {
									return await factory.Invoke(source, destination);
								}
								catch (MapNotFoundException) { }

								// Since we finished the mappers, we check if any mapper left can map the types
								if (!enumerator.MoveNext()) {
									foreach (var mapper in _mappers.OfType<IAsyncMapperCanMap>()) {
										if (mapper is IAsyncMapperFactory)
											continue;

										try {
											if (!await mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions, cancellationToken))
												unavailableMappers.Add(mapper);
										}
										catch { }
									}

									break;
								}
							}
						}
					}
					finally {
						enumeratorSemaphore.Release();
					}

					// Invoke the default map if there are any mappers left
					if (unavailableMappers.Count != _mappers.Count)
						return await MapInternal(_mappers.Except(unavailableMappers), source, sourceType, destination, destinationType, mappingOptions, cancellationToken);
					else
						throw new MapNotFoundException((sourceType, destinationType));
				},
				enumerator, enumeratorSemaphore, new LambdaDisposable(() => {
					foreach (var factory in factoriesCache) {
						factory.Dispose();
					}
				}));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		// Will override the mapper if not already overridden
		MappingOptions GetOrCreateMappingOptions(MappingOptions options) {
			if (options == null)
				return _optionsCacheNull;
			else {
				return _optionsCache.GetOrAdd(options, opts => opts.ReplaceOrAdd<AsyncMapperOverrideMappingOptions, AsyncNestedMappingContext>(
					m => m?.Mapper != null ? m : new AsyncMapperOverrideMappingOptions(this, m?.ServiceProvider),
					n => n != null ? new AsyncNestedMappingContext(this, n) : _nestedMappingContext));
			}
		}

		private static async Task<object> MapInternal(IEnumerable<IAsyncMapper> mappers,
			object source, Type sourceType, Type destinationType,
			MappingOptions mappingOptions, CancellationToken cancellationToken) {

			// Try new map
			foreach (var mapper in mappers) {
				try {
					return await mapper.MapAsync(source, sourceType, destinationType, mappingOptions, cancellationToken);
				}
				catch (MapNotFoundException) { }
			}

			// Try creating a destination and forward to merge map
			object destination;
			try {
				destination = ObjectFactory.Create(destinationType);
			}
			catch (ObjectCreationException) {
				throw new MapNotFoundException((sourceType, destinationType));
			}

			return await MapInternal(mappers, source, sourceType, destination, destinationType, mappingOptions, cancellationToken);
		}

		private static async Task<object> MapInternal(IEnumerable<IAsyncMapper> mappers,
			object source, Type sourceType, object destination, Type destinationType,
			MappingOptions mappingOptions, CancellationToken cancellationToken) {

			foreach (var mapper in mappers) {
				try {
					return await mapper.MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken);
				}
				catch (MapNotFoundException) { }
			}

			throw new MapNotFoundException((sourceType, destinationType));
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
