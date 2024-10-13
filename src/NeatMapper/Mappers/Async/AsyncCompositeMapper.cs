using System;
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
	public sealed class AsyncCompositeMapper : IAsyncMapper, IAsyncMapperFactory, IAsyncMapperMaps {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		/// <summary>
		/// Singleton array used for Linq queries.
		/// </summary>
		private static readonly object[] _singleElementArray = new object[] { null };


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


		/// <summary>
		/// List of <see cref="IMapper"/>s to be tried in order when mapping types.
		/// </summary>
		private readonly IList<IAsyncMapper> _mappers;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		/// <summary>
		/// Creates the mapper by using the provided mappers list.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public AsyncCompositeMapper(params IAsyncMapper[] mappers) :
			this((IList<IAsyncMapper>)mappers ?? throw new ArgumentNullException(nameof(mappers))) { }

		/// <summary>
		/// Creates the mapper by using the provided mappers list.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public AsyncCompositeMapper(IList<IAsyncMapper> mappers) {
			_mappers = new List<IAsyncMapper>(mappers ?? throw new ArgumentNullException(nameof(mappers)));
			var nestedMappingContext = new AsyncNestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<AsyncMapperOverrideMappingOptions, AsyncNestedMappingContext>(
				m => m?.Mapper != null ? m : new AsyncMapperOverrideMappingOptions(this, m?.ServiceProvider),
				n => n != null ? new AsyncNestedMappingContext(this, n) : nestedMappingContext, options.Cached));
		}


		#region IAsyncMapper methods
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

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			// Check if any mapper implements IAsyncMapperCanMap, if one of them throws it means that the map can be checked only when mapping
			var undeterminateMappers = new List<IAsyncMapper>();
			foreach (var mapper in _mappers) {
				try {
					if (await mapper.CanMapAsyncNew(sourceType, destinationType, mappingOptions, cancellationToken))
						return true;
				}
				catch (InvalidOperationException) {
					undeterminateMappers.Add(mapper);
				}
			}

			// Try creating a default source object and try mapping it
			if (undeterminateMappers.Count > 0) {
				object source;
				try {
					source = ObjectFactory.GetOrCreateCached(sourceType) ?? throw new Exception(); // Just in case
				}
				catch {
					throw new InvalidOperationException("Cannot verify if the mapper supports the given map because unable to create an object to test it");
				}

				foreach (var mapper in undeterminateMappers) {
					try {
						await mapper.MapAsync(source, sourceType, destinationType, mappingOptions, cancellationToken);
						return true;
					}
					catch (MapNotFoundException) { }
				}

				throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
			}
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

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			// Check if any mapper implements IAsyncMapperCanMap, if one of them throws it means that the map can be checked only when mapping
			var undeterminateMappers = new List<IAsyncMapper>();
			foreach (var mapper in _mappers) {
				try {
					if (await mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions, cancellationToken))
						return true;
				}
				catch (InvalidOperationException) {
					undeterminateMappers.Add(mapper);
				}
			}

			// Try creating two default source and destination objects and try mapping them
			if (undeterminateMappers.Count > 0) {
				object source;
				object destination;
				try {
					source = ObjectFactory.GetOrCreateCached(sourceType) ?? throw new Exception(); // Just in case
					destination = ObjectFactory.Create(destinationType) ?? throw new Exception(); // Just in case
				}
				catch {
					throw new InvalidOperationException("Cannot verify if the mapper supports the given map because unable to create the objects to test it");
				}

				foreach (var mapper in undeterminateMappers) {
					try {
						await mapper.MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken);
						return true;
					}
					catch (MapNotFoundException) { }
				}

				throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
			}
			else
				return false;
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
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return MapInternal(_mappers, source, sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions), cancellationToken);
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

			return MapInternal(_mappers, source, sourceType, destination, destinationType, _optionsCache.GetOrCreate(mappingOptions), cancellationToken);
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

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			// DEV: maybe check with CanMapAsync and if returns false throw instead of creating the factory?

			var unavailableMappersNew = new HashSet<IAsyncMapper>();
			var factoriesNew = new CachedLazyEnumerable<IAsyncNewMapFactory>(
				_mappers.OfType<IAsyncMapperFactory>()
				.Select(mapper => {
					try {
						return mapper.MapAsyncNewFactory(sourceType, destinationType, mappingOptions);
					}
					catch (MapNotFoundException) {
						lock (unavailableMappersNew) {
							unavailableMappersNew.Add(mapper);
						}
						return null;
					}
				})
				.Concat(_singleElementArray.Select(e => {
					// Since we finished the mappers, we check if any mapper left can map the types
					foreach (var mapper in _mappers) {
						if (mapper is IAsyncMapperFactory)
							continue;

						try {
							if (!mapper.CanMapAsyncNew(sourceType, destinationType, mappingOptions).Result) {
								lock (unavailableMappersNew) {
									unavailableMappersNew.Add(mapper);
								}
							}
							else
								break;
						}
						catch { }
					}

					return (IAsyncNewMapFactory)e;
				}))
				.Where(factory => factory != null));

			try { 
				var unavailableMappersMerge = new HashSet<IAsyncMapper>();
				var factoriesMerge = new CachedLazyEnumerable<IAsyncNewMapFactory>(
					_mappers.OfType<IAsyncMapperFactory>()
					.Select(mapper => {
						try {
							return mapper.MapAsyncMergeFactory(sourceType, destinationType, mappingOptions).MapAsyncNewFactory();
						}
						catch (MapNotFoundException) {
							lock (unavailableMappersMerge) {
								unavailableMappersMerge.Add(mapper);
							}
							return null;
						}
					})
					.Concat(_singleElementArray.Select(e => {
						// Since we finished the mappers, we check if any mapper left can map the types
						foreach (var mapper in _mappers) {
							if (mapper is IAsyncMapperFactory)
								continue;

							try {
								if (!mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions).Result || !ObjectFactory.CanCreate(destinationType)) {
									lock (unavailableMappersMerge) {
										unavailableMappersMerge.Add(mapper);
									}
								}
								else
									break;
							}
							catch { }
						}

						return (IAsyncNewMapFactory)e;
					}))
					.Where(factory => factory != null));

				try { 
					return new DisposableAsyncNewMapFactory(
						sourceType, destinationType,
						async (source, cancellationToken) => {
							// Try using the new factories, if any
							foreach (var factory in factoriesNew) {
								try {
									return await factory.Invoke(source, cancellationToken);
								}
								catch (MapNotFoundException) { }
							}

							// Try using the merge factories, if any
							foreach (var factory in factoriesMerge) {
								try {
									return await factory.Invoke(source, cancellationToken);
								}
								catch (MapNotFoundException) { }
							}

							// Invoke the default map if there are any mappers left (no locking needed on unavailableMappers
							// because factories is already fully enumerated)
							if (unavailableMappersNew.Count != _mappers.Count ||
								unavailableMappersMerge.Count != _mappers.Count) { 

								return await MapInternal(_mappers.Except(unavailableMappersNew.Intersect(unavailableMappersMerge)), source, sourceType, destinationType, mappingOptions, cancellationToken);
							}
							else
								throw new MapNotFoundException((sourceType, destinationType));
						},
						factoriesNew, factoriesMerge);
				}
				catch {
					factoriesMerge.Dispose();
					throw;
				}
			}
			catch {
				factoriesNew.Dispose();
				throw;
			}

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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			// DEV: maybe check with CanMapAsync and if returns false throw instead of creating the factory?

			var unavailableMappers = new HashSet<IAsyncMapper>();
			var factories = new CachedLazyEnumerable<IAsyncMergeMapFactory>(
				_mappers.OfType<IAsyncMapperFactory>()
				.Select(mapper => {
					try {
						return mapper.MapAsyncMergeFactory(sourceType, destinationType, mappingOptions);
					}
					catch (MapNotFoundException) {
						lock (unavailableMappers) {
							unavailableMappers.Add(mapper);
						}
						return null;
					}
				})
				.Concat(_singleElementArray.Select(_ => {
					// Since we finished the mappers, we check if any mapper left can map the types
					foreach (var mapper in _mappers) {
						if (mapper is IAsyncMapperFactory)
							continue;

						try {
							if (!mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions).Result) {
								lock (unavailableMappers) {
									unavailableMappers.Add(mapper);
								}
							}
							else
								break;
						}
						catch { }
					}

					return (IAsyncMergeMapFactory)null;
				}))
				.Where(factory => factory != null));

			try { 
				return new DisposableAsyncMergeMapFactory(
					sourceType, destinationType,
					async (source, destination, cancellationToken) => {
						// Try using the factories, if any
						foreach (var factory in factories) {
							try {
								return await factory.Invoke(source, destination, cancellationToken);
							}
							catch (MapNotFoundException) { }
						}

						// Invoke the default map if there are any mappers left (no locking needed on unavailableMappers
						// because factories is already fully enumerated)
						if (unavailableMappers.Count != _mappers.Count)
							return await MapInternal(_mappers.Except(unavailableMappers), source, sourceType, destination, destinationType, mappingOptions, cancellationToken);
						else
							throw new MapNotFoundException((sourceType, destinationType));
					},
					factories);
			}
			catch {
				factories.Dispose();
				throw;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region IAsyncMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetAsyncNewMaps(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			// Supports both new and merge maps (where destination object can be created)
			return _mappers.SelectMany(m => m.GetAsyncNewMaps(mappingOptions))
				.Concat(GetAsyncMergeMaps(mappingOptions).Where(m => ObjectFactory.CanCreate(m.To)));
		}

		public IEnumerable<(Type From, Type To)> GetAsyncMergeMaps(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return _mappers.SelectMany(m => m.GetAsyncMergeMaps(mappingOptions));
		}
		#endregion
	}
}
