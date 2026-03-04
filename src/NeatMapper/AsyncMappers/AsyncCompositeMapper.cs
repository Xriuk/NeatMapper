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
		/// Composite mapper options.
		/// </summary>
		private readonly AsyncCompositeMapperOptions _compositeMapperOptions;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		/// <summary>
		/// Creates the mapper by using the provided mappers list.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public AsyncCompositeMapper(params IAsyncMapper[] mappers) :
			this(new AsyncCompositeMapperOptions { Mappers = mappers ?? throw new ArgumentNullException(nameof(mappers)) }) { }
		/// <summary>
		/// Creates the mapper by using the provided mappers list.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public AsyncCompositeMapper(IList<IAsyncMapper> mappers) :
			this(new AsyncCompositeMapperOptions { Mappers = mappers ?? throw new ArgumentNullException(nameof(mappers)) }) { }
		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapper"/>.
		/// </summary>
		/// <param name="options">Options to create the mapper with.<br/>
		/// Can be overridden during mapping with <see cref="CompositeMapper"/>.</param>
		public AsyncCompositeMapper(AsyncCompositeMapperOptions options) {
			_compositeMapperOptions = new AsyncCompositeMapperOptions(options ?? throw new ArgumentNullException(nameof(options)));
			var nestedMappingContext = new AsyncNestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<AsyncMapperOverrideMappingOptions, AsyncNestedMappingContext>(
				m => m?.Mapper != null ? m : new AsyncMapperOverrideMappingOptions(this, m?.ServiceProvider),
				n => n != null ? new AsyncNestedMappingContext(this, n) : nestedMappingContext, options.Cached));
		}


		#region IAsyncMapper methods
		public bool CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			if (_compositeMapperOptions.Mappers.Any(m => m.CanMapAsyncNew(sourceType, destinationType, mappingOptions)))
				return true;

			var mergeMapsHandling = mappingOptions.GetOptions<AsyncCompositeMapperMappingOptions>()?.MergeMapsHandling
				?? _compositeMapperOptions.MergeMapsHandling;

			return mergeMapsHandling != MergeMapsHandling.DoNotMap &&
				(mergeMapsHandling != MergeMapsHandling.CreateDestination || ObjectFactory.CanCreate(destinationType)) &&
				_compositeMapperOptions.Mappers.Any(m => m.CanMapAsyncMerge(sourceType, destinationType, mappingOptions));
		}

		public bool CanMapAsyncMerge(Type sourceType, Type destinationType,MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			return _compositeMapperOptions.Mappers.Any(m => m.CanMapAsyncMerge(sourceType, destinationType, mappingOptions));
		}

		public Task<object?> MapAsync(
			object? source,
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			var newMappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var mapper = _compositeMapperOptions.Mappers.FirstOrDefault(m => m.CanMapAsyncNew(sourceType, destinationType, newMappingOptions));
			if (mapper != null)
				return mapper.MapAsync(source, sourceType, destinationType, newMappingOptions, cancellationToken);

			var mergeMapsHandling = newMappingOptions.GetOptions<AsyncCompositeMapperMappingOptions>()?.MergeMapsHandling
				?? _compositeMapperOptions.MergeMapsHandling;

			if (mergeMapsHandling != MergeMapsHandling.DoNotMap &&
				(mergeMapsHandling != MergeMapsHandling.CreateDestination || ObjectFactory.CanCreate(destinationType))) {

				return MapAsync(
					source,
					sourceType,
					(mergeMapsHandling == MergeMapsHandling.CreateDestination ?
						ObjectFactory.Create(destinationType) :
						destinationType.GetDefault()),
					destinationType,
					mappingOptions,
					cancellationToken);
			}

			throw new MapNotFoundException((sourceType, destinationType));
		}

		public Task<object?> MapAsync(
			object? source,
			Type sourceType,
			object? destination,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var mapper = _compositeMapperOptions.Mappers.FirstOrDefault(m => m.CanMapAsyncMerge(sourceType, destinationType, mappingOptions));
			if (mapper != null)
				return mapper.MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken);
			else
				throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion

		#region IAsyncMapperFactory methods
		public IAsyncNewMapFactory MapAsyncNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			// Try retrieving a new factory
			IAsyncMapper? validMapper = null;
			foreach (var mapper in _compositeMapperOptions.Mappers) {
				if (mapper.CanMapAsyncNew(sourceType, destinationType, mappingOptions)) {
					if (mapper is IAsyncMapperFactory factory)
						return factory.MapAsyncNewFactory(sourceType, destinationType, mappingOptions);
					else
						validMapper ??= mapper;
				}
			}

			// If we can map the types we return MapAsync wrapped in a delegate
			if (validMapper != null) {
				return new DefaultAsyncNewMapFactory(
					sourceType, destinationType,
					(source, cancellationToken) => validMapper.MapAsync(source, sourceType, destinationType, mappingOptions, cancellationToken));
			}

			// Check if we can forward to merge map
			var mergeMapsHandling = mappingOptions.GetOptions<AsyncCompositeMapperMappingOptions>()?.MergeMapsHandling
				?? _compositeMapperOptions.MergeMapsHandling;

			// Check if we can forward to merge map by creating a destination object
			if (mergeMapsHandling != MergeMapsHandling.DoNotMap &&
				(mergeMapsHandling != MergeMapsHandling.CreateDestination || ObjectFactory.CanCreate(destinationType))) {

				// Try retrieving a merge factory
				foreach (var mapper in _compositeMapperOptions.Mappers) {
					if (mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions)) {
						if (mapper is IAsyncMapperFactory factory)
							return factory.MapAsyncMergeFactory(sourceType, destinationType, mappingOptions)
								.MapAsyncNewFactory(mergeMapsHandling == MergeMapsHandling.CreateDestination, true);
						else
							validMapper ??= mapper;
					}
				}

				// If we can map the types we return MapAsync wrapped in a delegate
				if (validMapper != null) {
					var destinationFactory = (mergeMapsHandling == MergeMapsHandling.CreateDestination ?
						(Func<object?>)ObjectFactory.CreateFactory(destinationType) :
						destinationType.GetDefault);

					return new DefaultAsyncNewMapFactory(
						sourceType, destinationType,
						(source, cancellationToken) => validMapper.MapAsync(source, sourceType, destinationFactory.Invoke(), destinationType, mappingOptions, cancellationToken));
				}
			}

			throw new MapNotFoundException((sourceType, destinationType));
		}

		public IAsyncMergeMapFactory MapAsyncMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			// Try retrieving a merge factory
			IAsyncMapper? validMapper = null;
			foreach (var mapper in _compositeMapperOptions.Mappers) {
				if (mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions)) {
					if (mapper is IAsyncMapperFactory factory)
						return factory.MapAsyncMergeFactory(sourceType, destinationType, mappingOptions);
					else
						validMapper ??= mapper;
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
		}
		#endregion

		#region IAsyncMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetAsyncNewMaps(MappingOptions? mappingOptions = null) {
			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var mergeMapsHandling = mappingOptions.GetOptions<AsyncCompositeMapperMappingOptions>()?.MergeMapsHandling
				?? _compositeMapperOptions.MergeMapsHandling;

			// Supports both new and merge maps
			IEnumerable<(Type From, Type To)> mergeMaps;
			if (mergeMapsHandling == MergeMapsHandling.DoNotMap)
				mergeMaps = [];
			else {
				mergeMaps = _compositeMapperOptions.Mappers.SelectMany(m => m.GetAsyncMergeMaps(mappingOptions));
				if (mergeMapsHandling == MergeMapsHandling.CreateDestination)
					mergeMaps = mergeMaps.Where(m => ObjectFactory.CanCreate(m.To));
			}

			return _compositeMapperOptions.Mappers.SelectMany(m => m.GetAsyncNewMaps(mappingOptions))
				.Concat(mergeMaps);
		}

		public IEnumerable<(Type From, Type To)> GetAsyncMergeMaps(MappingOptions? mappingOptions = null) {
			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			return _compositeMapperOptions.Mappers.SelectMany(m => m.GetAsyncMergeMaps(mappingOptions));
		}
		#endregion
	}
}
