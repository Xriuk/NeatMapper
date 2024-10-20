using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which delegates mapping to other <see cref="IAsyncMapper"/>s,
	/// this allows to combine different mapping capabilities.<br/>
	/// For new maps, if no mapper can map the types a destination object is created and merge maps are tried.
	/// </summary>
	public sealed class AsyncCompositeMapper : IAsyncMapper, IAsyncMapperFactory, IAsyncMapperMaps {
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
		public bool CanMapAsyncNew(
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

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			return _mappers.Any(m => m.CanMapAsyncNew(sourceType, destinationType, mappingOptions)) ||
				(ObjectFactory.CanCreate(destinationType) && _mappers.Any(m => m.CanMapAsyncMerge(sourceType, destinationType, mappingOptions)));
		}

		public bool CanMapAsyncMerge(
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

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			return _mappers.Any(m => m.CanMapAsyncMerge(sourceType, destinationType, mappingOptions));
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

			var newMappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var mapper = _mappers.FirstOrDefault(m => m.CanMapAsyncNew(sourceType, destinationType, newMappingOptions));
			if (mapper != null)
				return mapper.MapAsync(source, sourceType, destinationType, newMappingOptions, cancellationToken);

			if (!ObjectFactory.CanCreate(destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			return MapAsync(source, sourceType, ObjectFactory.Create(destinationType), destinationType, mappingOptions, cancellationToken);
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			var mapper = _mappers.FirstOrDefault(m => m.CanMapAsyncMerge(sourceType, destinationType, mappingOptions));
			if (mapper != null)
				return mapper.MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken);
			else
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

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			// Try retrieving a new factory
			IAsyncMapper validMapper = null;
			foreach (var mapper in _mappers) {
				if (mapper.CanMapAsyncNew(sourceType, destinationType, mappingOptions)) {
					if (mapper is IAsyncMapperFactory factory)
						return factory.MapAsyncNewFactory(sourceType, destinationType, mappingOptions);
					else if (validMapper == null)
						validMapper = mapper;
				}
			}

			// If we can map the types we return MapAsync wrapped in a delegate
			if (validMapper != null) {
				return new DefaultAsyncNewMapFactory(
					sourceType, destinationType,
					(source, cancellationToken) => validMapper.MapAsync(source, sourceType, destinationType, mappingOptions, cancellationToken));
			}

			// Check if we can forward to merge map by creating a destination object
			if (ObjectFactory.CanCreate(destinationType)) {
				// Try retrieving a merge factory
				foreach (var mapper in _mappers) {
					if (mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions)) {
						if (mapper is IAsyncMapperFactory factory)
							return factory.MapAsyncMergeFactory(sourceType, destinationType, mappingOptions).MapAsyncNewFactory();
						else if (validMapper == null)
							validMapper = mapper;
					}
				}

				// If we can map the types we return MapAsync wrapped in a delegate
				if (validMapper != null) {
					var destinationFactory = ObjectFactory.CreateFactory(destinationType);

					return new DefaultAsyncNewMapFactory(
						sourceType, destinationType,
						(source, cancellationToken) => validMapper.MapAsync(source, sourceType, destinationFactory.Invoke(), destinationType, mappingOptions, cancellationToken));
				}
			}

			throw new MapNotFoundException((sourceType, destinationType));

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

			// Try retrieving a merge factory
			IAsyncMapper validMapper = null;
			foreach (var mapper in _mappers) {
				if (mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions)) {
					if (mapper is IAsyncMapperFactory factory)
						return factory.MapAsyncMergeFactory(sourceType, destinationType, mappingOptions);
					else if (validMapper == null)
						validMapper = mapper;
				}
			}

			// If we can map the types we return Map wrapped in a delegate
			if (validMapper != null) {
				return new DefaultAsyncMergeMapFactory(
					sourceType, destinationType,
					(source, destination, cancellationToken) => validMapper.MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken));
			}
			else
				throw new MapNotFoundException((sourceType, destinationType));

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
