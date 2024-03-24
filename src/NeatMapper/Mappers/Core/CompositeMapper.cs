using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which delegates mapping to other <see cref="IMapper"/>s,
	/// this allows to combine different mapping capabilities.<br/>
	/// Each mapper is invoked in order and the first one to succeed in mapping is returned.<br/>
	/// For new maps, if no mapper can map the types a destination object is created and merge maps are tried.
	/// </summary>
	public sealed class CompositeMapper : IMapper, IMapperCanMap, IMapperFactory {
		/// <summary>
		/// List of <see cref="IMapper"/>s to be tried in order when mapping types.
		/// </summary>
		private readonly IReadOnlyList<IMapper> _mappers;

		/// <summary>
		/// Cached <see cref="NestedMappingContext"/> to provide, if not already provided in <see cref="MappingOptions"/>.
		/// </summary>
		private readonly NestedMappingContext _nestedMappingContext;

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
		/// Creates a new instance of <see cref="CompositeMapper"/>.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public CompositeMapper(params IMapper[] mappers) : this((IList<IMapper>) mappers) { }

		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapper"/>.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public CompositeMapper(IList<IMapper> mappers) {
			if (mappers == null)
				throw new ArgumentNullException(nameof(mappers));

			_mappers = new List<IMapper>(mappers);
			_nestedMappingContext = new NestedMappingContext(this);
			_optionsCacheNull = GetOrCreateMappingOptions(MappingOptions.Empty);
		}


		#region IMapper methods
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
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return MapInternal(_mappers, source, sourceType, destinationType, GetOrCreateMappingOptions(mappingOptions));
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
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return MapInternal(_mappers, source, sourceType, destination, destinationType, GetOrCreateMappingOptions(mappingOptions));
		}
		#endregion

		#region IMapperCanMap methods
		public bool CanMapNew(
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

			mappingOptions = GetOrCreateMappingOptions(mappingOptions);

			// Check if any mapper implements IMapperCanMap, if one of them throws it means that the map can be checked only when mapping
			var undeterminateMappers = new List<IMapper>();
			foreach (var mapper in _mappers.OfType<IMapperCanMap>()) {
				try { 
					if(mapper.CanMapNew(sourceType, destinationType, mappingOptions))
						return true;
				}
				catch (InvalidOperationException) {
					undeterminateMappers.Add(mapper);
				}
			}

			// Try creating a default source object and try mapping it
			var mappersLeft = _mappers.Where(m => !(m is IMapperCanMap) || undeterminateMappers.IndexOf(m) != -1);
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
						mapper.Map(source, sourceType, destinationType, mappingOptions);
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

		public bool CanMapMerge(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = GetOrCreateMappingOptions(mappingOptions);

			// Check if any mapper implements IMapperCanMap, if one of them throws it means that the map can be checked only when mapping
			var undeterminateMappers = new List<IMapper>();
			foreach (var mapper in _mappers.OfType<IMapperCanMap>()) {
				try {
					if (mapper.CanMapMerge(sourceType, destinationType, mappingOptions))
						return true;
				}
				catch (InvalidOperationException) {
					undeterminateMappers.Add(mapper);
				}
			}

			// Try creating two default source and destination objects and try mapping them
			var mappersLeft = _mappers.Where(m => !(m is IMapperCanMap) || undeterminateMappers.IndexOf(m) != -1);
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
						mapper.Map(source, sourceType, destination, destinationType, mappingOptions);
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

		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(
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

			mappingOptions = GetOrCreateMappingOptions(mappingOptions);

			var unavailableMappers = new ConcurrentBag<IMapper>();
			var factoriesCache = new ConcurrentBag<INewMapFactory>();
			var enumerator = _mappers.OfType<IMapperFactory>().GetEnumerator();

			// DEV: maybe check with CanMap and if returns false throw instead of creating the factory?

			return new DisposableNewMapFactory(
				sourceType, destinationType,
				source => {
					// Try using cached factories, if any
					foreach(var factory in factoriesCache) {
						try {
							return factory.Invoke(source);
						}
						catch (MapNotFoundException) { }
					}

					// Retrieve and cache new factories, if the mappers throw while retrieving the factory
					// they can never map the given types (we assume that if it cannot provide a factory for two types,
					// it cannot even map them), otherwise they might map them and fail (and we'll retry later)
					lock (enumerator) { 
						if (enumerator.MoveNext()) { 
							while (true) {
								INewMapFactory factory;
								try {
									factory = enumerator.Current.MapNewFactory(sourceType, destinationType, mappingOptions);
								}
								catch (MapNotFoundException) {
									unavailableMappers.Add(enumerator.Current);
									if(enumerator.MoveNext())
										continue;
									else
										break;
								}

								factoriesCache.Add(factory);

								try {
									return factory.Invoke(source);
								}
								catch (MapNotFoundException) { }

								// Since we finished the mappers, we check if any mapper left can map the types
								if (!enumerator.MoveNext()) {
									foreach (var mapper in _mappers.OfType<IMapperCanMap>()) {
										if(mapper is IMapperFactory)
											continue;

										try {
											if (!mapper.CanMapNew(sourceType, destinationType, mappingOptions))
												unavailableMappers.Add(mapper);
										}
										catch { }
									}

									break;
								}
							}
						}
					}

					// Invoke the default map if there are any mappers left
					if (unavailableMappers.Count != _mappers.Count)
						return MapInternal(_mappers.Except(unavailableMappers), source, sourceType, destinationType, mappingOptions);
					else
						throw new MapNotFoundException((sourceType, destinationType));
				},
				enumerator, new LambdaDisposable(() => {
					foreach(var factory in factoriesCache) {
						factory.Dispose();
					}
				}));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public IMergeMapFactory MapMergeFactory(
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

			mappingOptions = GetOrCreateMappingOptions(mappingOptions);

			var unavailableMappers = new ConcurrentBag<IMapper>();
			var factoriesCache = new ConcurrentBag<IMergeMapFactory>();
			var enumerator = _mappers.OfType<IMapperFactory>().GetEnumerator();

			// DEV: maybe check with CanMap and if returns false throw instead of creating the factory?

			return new DisposableMergeMapFactory(sourceType, destinationType, (source, destination) => {
				// Try using cached factories, if any
				foreach (var factory in factoriesCache) {
					try {
						return factory.Invoke(source, destination);
					}
					catch (MapNotFoundException) { }
				}

				// Retrieve and cache new factories, if the mappers throw while retrieving the factory
				// they can never map the given types (we assume that if it cannot provide a factory for two types,
				// it cannot even map them), otherwise they might map them and fail (and we'll retry later)
				lock (enumerator) {
					if (enumerator.MoveNext()) {
						while (true) {
							IMergeMapFactory factory;
							try {
								factory = enumerator.Current.MapMergeFactory(sourceType, destinationType, mappingOptions);
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
								return factory.Invoke(source, destination);
							}
							catch (MapNotFoundException) { }

							// Since we finished the mappers, we check if any mapper left can map the types
							if (!enumerator.MoveNext()) {
								foreach (var mapper in _mappers.OfType<IMapperCanMap>()) {
									if (mapper is IMapperFactory)
										continue;

									try {
										if (!mapper.CanMapMerge(sourceType, destinationType, mappingOptions))
											unavailableMappers.Add(mapper);
									}
									catch { }
								}

								break;
							}
						}
					}
				}

				// Invoke the default map if there are any mappers left
				if (unavailableMappers.Count != _mappers.Count)
					return MapInternal(_mappers.Except(unavailableMappers), source, sourceType, destination, destinationType, mappingOptions);
				else
					throw new MapNotFoundException((sourceType, destinationType));
			}, enumerator, new LambdaDisposable(() => {
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

		MappingOptions GetOrCreateMappingOptions(MappingOptions options) {
			if (options == null)
				return _optionsCacheNull;
			else {
				return _optionsCache.GetOrAdd(options, opts => opts.ReplaceOrAdd<MapperOverrideMappingOptions, NestedMappingContext>(
					m => m?.Mapper != null ? m : new MapperOverrideMappingOptions(this, m?.ServiceProvider),
					n => n != null ? new NestedMappingContext(this, n) : _nestedMappingContext));
			}
		}

		private static object MapInternal(IEnumerable<IMapper> mappers,
			object source, Type sourceType, Type destinationType,
			MappingOptions mappingOptions) {

			// Try new map
			foreach (var mapper in mappers) {
				try {
					return mapper.Map(source, sourceType, destinationType, mappingOptions);
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

			return MapInternal(mappers, source, sourceType, destination, destinationType, mappingOptions);
		}

		private static object MapInternal(IEnumerable<IMapper> mappers,
			object source, Type sourceType, object destination, Type destinationType,
			MappingOptions mappingOptions) {

			foreach (var mapper in mappers) {
				try {
					return mapper.Map(source, sourceType, destination, destinationType, mappingOptions);
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
