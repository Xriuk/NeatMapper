using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which delegates mapping to other <see cref="IMapper"/>s,
	/// this allows to combine different mapping capabilities.<br/>
	/// Each mapper is invoked in order and the first one to succeed in mapping is returned.<br/>
	/// For new maps, if no mapper can map the types a destination object is created and merge maps are tried.
	/// </summary>
	public sealed class CompositeMapper : IMapper, IMapperCanMap, IMapperFactory, IMapperMaps {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		/// <summary>
		/// Singleton array used for Linq queries.
		/// </summary>
		private static readonly object[] _singleElementArray = new object[] { null };


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


		/// <summary>
		/// List of <see cref="IMapper"/>s to be tried in order when mapping types.
		/// </summary>
		private readonly IReadOnlyList<IMapper> _mappers;

		/// <summary>
		/// Cached <see cref="NestedMappingContext"/> to provide, if not already provided in <see cref="MappingOptions"/>.
		/// </summary>
		private readonly NestedMappingContext _nestedMappingContext;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output <see cref="MappingOptions"/> (with 
		///	<see cref="MappingOptions.Cached"/> also set to <see langword="true"/>).
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MappingOptions> _optionsCache =
			new ConcurrentDictionary<MappingOptions, MappingOptions>();

		/// <summary>
		/// Cached output <see cref="MappingOptions"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have null keys) and <see cref="MappingOptions.Empty"/> inputs,
		/// also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		private readonly MappingOptions _optionsCacheNull;


		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapper"/>.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public CompositeMapper(params IMapper[] mappers) :
			this((IList<IMapper>)mappers ?? throw new ArgumentNullException(nameof(mappers))) { }

		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapper"/>.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public CompositeMapper(IList<IMapper> mappers) {
			_mappers = new List<IMapper>(mappers ?? throw new ArgumentNullException(nameof(mappers)));
			_nestedMappingContext = new NestedMappingContext(this);
			_optionsCacheNull = MergeMappingOptions(MappingOptions.Empty);
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

			return MapInternal(_mappers, source, sourceType, destinationType, MergeOrCreateMappingOptions(mappingOptions));
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

			return MapInternal(_mappers, source, sourceType, destination, destinationType, MergeOrCreateMappingOptions(mappingOptions));
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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			var unavailableMappersNew = new HashSet<IMapper>();
			var factoriesNew = new CachedLazyEnumerable<INewMapFactory>(
				_mappers.OfType<IMapperFactory>()
				.Select(mapper => {
					try {
						return mapper.MapNewFactory(sourceType, destinationType, mappingOptions);
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
					foreach (var mapper in _mappers.OfType<IMapperCanMap>()) {
						if (mapper is IMapperFactory)
							continue;

						try {
							if (!mapper.CanMapNew(sourceType, destinationType, mappingOptions)) {
								lock (unavailableMappersNew) {
									unavailableMappersNew.Add(mapper);
								}
							}
							else
								break;
						}
						catch { }
					}

					return (INewMapFactory)e;
				}))
				.Where(factory => factory != null));

			var unavailableMappersMerge = new HashSet<IMapper>();
			var factoriesMerge = new CachedLazyEnumerable<INewMapFactory>(
				_mappers.OfType<IMapperFactory>()
				.Select(mapper => {
					try {
						return mapper.MapMergeFactory(sourceType, destinationType, mappingOptions).MapNewFactory();
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
					foreach (var mapper in _mappers.OfType<IMapperCanMap>()) {
						if (mapper is IMapperFactory)
							continue;

						try {
							if (!mapper.CanMapMerge(sourceType, destinationType, mappingOptions) || !ObjectFactory.CanCreate(destinationType)) {
								lock (unavailableMappersMerge) {
									unavailableMappersMerge.Add(mapper);
								}
							}
							else
								break;
						}
						catch { }
					}

					return (INewMapFactory)e;
				}))
				.Where(factory => factory != null));

			// DEV: maybe check with CanMap and if returns false throw instead of creating the factory?

			return new DisposableNewMapFactory(
				sourceType, destinationType, 
				source => {
					// Try using the new factories, if any
					foreach (var factory in factoriesNew) {
						try {
							return factory.Invoke(source);
						}
						catch (MapNotFoundException) { }
					}

					// Try using the merge factories, if any
					foreach (var factory in factoriesMerge) {
						try {
							return factory.Invoke(source);
						}
						catch (MapNotFoundException) { }
					}

					// Invoke the default map if there are any mappers left (no locking needed on unavailableMappers
					// because factories is already fully enumerated)
					if (unavailableMappersNew.Count != _mappers.Count ||
						unavailableMappersMerge.Count != _mappers.Count) { 

						return MapInternal(_mappers.Except(unavailableMappersNew.Intersect(unavailableMappersMerge)), source, sourceType, destinationType, mappingOptions);
					}
					else
						throw new MapNotFoundException((sourceType, destinationType));
				},
				factoriesNew, factoriesMerge, new LambdaDisposable(() => {
					foreach(var factory in factoriesNew.Cached) {
						factory.Dispose();
					}
					foreach (var factory in factoriesMerge.Cached) {
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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			var unavailableMappers = new HashSet<IMapper>();
			var factories = new CachedLazyEnumerable<IMergeMapFactory>(
				_mappers.OfType<IMapperFactory>()
				.Select(mapper => {
					try {
						return mapper.MapMergeFactory(sourceType, destinationType, mappingOptions);
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
					foreach (var mapper in _mappers.OfType<IMapperCanMap>()) {
						if (mapper is IMapperFactory)
							continue;

						try {
							if (!mapper.CanMapMerge(sourceType, destinationType, mappingOptions)) {
								lock (unavailableMappers) {
									unavailableMappers.Add(mapper);
								}
							}
							else
								break;
						}
						catch { }
					}

					return (IMergeMapFactory)null;
				}))
				.Where(factory => factory != null));

			// DEV: maybe check with CanMap and if returns false throw instead of creating the factory?

			return new DisposableMergeMapFactory(
				sourceType, destinationType,
				(source, destination) => {
					// Try using the factories, if any
					foreach (var factory in factories) {
						try {
							return factory.Invoke(source, destination);
						}
						catch (MapNotFoundException) { }
					}

					// Invoke the default map if there are any mappers left (no locking needed on unavailableMappers
					// because factories is already fully enumerated)
					if (unavailableMappers.Count != _mappers.Count)
						return MapInternal(_mappers.Except(unavailableMappers), source, sourceType, destination, destinationType, mappingOptions);
					else
						throw new MapNotFoundException((sourceType, destinationType));
				},
				factories, new LambdaDisposable(() => {
					foreach (var factory in factories.Cached) {
						factory.Dispose();
					}
				}));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region IMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetNewMaps(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			// Supports both new and merge maps (where destination object can be created)
			return _mappers.SelectMany(m => m.GetNewMaps(mappingOptions))
				.Concat(GetMergeMaps(mappingOptions).Where(m => ObjectFactory.CanCreate(m.To)));
		}

		public IEnumerable<(Type From, Type To)> GetMergeMaps(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return _mappers.SelectMany(m => m.GetMergeMaps(mappingOptions));
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		// Will override the mapper if not already overridden
		private MappingOptions MergeOrCreateMappingOptions(MappingOptions options) {
			if (options == null || options == MappingOptions.Empty)
				return _optionsCacheNull;
			else if(options.Cached)
				return _optionsCache.GetOrAdd(options, MergeMappingOptions);
			else
				return MergeMappingOptions(options);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MappingOptions MergeMappingOptions(MappingOptions options) {
			// Caching (if options are cached aswell)
			return options.ReplaceOrAdd<MapperOverrideMappingOptions, NestedMappingContext>(
				m => m?.Mapper != null ? m : new MapperOverrideMappingOptions(this, m?.ServiceProvider),
				n => n != null ? new NestedMappingContext(this, n) : _nestedMappingContext, options.Cached);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
