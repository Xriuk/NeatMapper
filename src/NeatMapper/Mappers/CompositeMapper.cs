using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which delegates mapping to other <see cref="IMapper"/>s,
	/// this allows to combine different mapping capabilities.<br/>
	/// For new maps, if no mapper can map the types a destination object is created and merge maps are tried.
	/// </summary>
	public sealed class CompositeMapper : IMapper, IMapperFactory, IMapperMaps {
		/// <summary>
		/// Composite mapper options.
		/// </summary>
		private readonly CompositeMapperOptions _compositeMapperOptions;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapper"/>.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public CompositeMapper(params IMapper[] mappers) :
			this(new CompositeMapperOptions { Mappers = new List<IMapper>(mappers ?? throw new ArgumentNullException(nameof(mappers))) }) { }
		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapper"/>.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public CompositeMapper(IList<IMapper> mappers) :
			this(new CompositeMapperOptions { Mappers = new List<IMapper>(mappers ?? throw new ArgumentNullException(nameof(mappers))) }) { }
		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapper"/>.
		/// </summary>
		/// <param name="options">Options to create the mapper with.<br/>
		/// Can be overridden during mapping with <see cref="CompositeMapper"/>.</param>
		public CompositeMapper(CompositeMapperOptions options) {
			_compositeMapperOptions = new CompositeMapperOptions(options ?? throw new ArgumentNullException(nameof(options)));
			var nestedMappingContext = new NestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<MapperOverrideMappingOptions, NestedMappingContext>(
				m => m?.Mapper != null ? m : new MapperOverrideMappingOptions(this, m?.ServiceProvider),
				n => n != null ? new NestedMappingContext(this, n) : nestedMappingContext, options.Cached));
		}


		#region IMapper methods
		public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			if (_compositeMapperOptions.Mappers.Any(m => m.CanMapNew(sourceType, destinationType, mappingOptions)))
				return true;

			var mergeMapsHandling = mappingOptions.GetOptions<CompositeMapperMappingOptions>()?.MergeMapsHandling
				?? _compositeMapperOptions.MergeMapsHandling;

			return mergeMapsHandling != MergeMapsHandling.DoNotMap &&
				(mergeMapsHandling != MergeMapsHandling.CreateDestination || ObjectFactory.CanCreate(destinationType)) &&
				_compositeMapperOptions.Mappers.Any(m => m.CanMapMerge(sourceType, destinationType, mappingOptions));
		}

		public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			return _compositeMapperOptions.Mappers.Any(m => m.CanMapMerge(sourceType, destinationType, mappingOptions));
		}

		public object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if(sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			var newMappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var mapper = _compositeMapperOptions.Mappers.FirstOrDefault(m => m.CanMapNew(sourceType, destinationType, newMappingOptions));
			if (mapper != null)
				return mapper.Map(source, sourceType, destinationType, newMappingOptions);

			var mergeMapsHandling = newMappingOptions.GetOptions<CompositeMapperMappingOptions>()?.MergeMapsHandling
				?? _compositeMapperOptions.MergeMapsHandling;

			if (mergeMapsHandling != MergeMapsHandling.DoNotMap &&
				(mergeMapsHandling != MergeMapsHandling.CreateDestination || ObjectFactory.CanCreate(destinationType))) {

				return Map(
					source,
					sourceType,
					(mergeMapsHandling == MergeMapsHandling.CreateDestination ?
						ObjectFactory.Create(destinationType) :
						destinationType.GetDefault()),
					destinationType,
					mappingOptions);
			}

			throw new MapNotFoundException((sourceType, destinationType));
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var mapper = _compositeMapperOptions.Mappers.FirstOrDefault(m => m.CanMapMerge(sourceType, destinationType, mappingOptions));
			if (mapper != null)
				return mapper.Map(source, sourceType, destination, destinationType, mappingOptions);
			else
				throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion

		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			// Try retrieving a new factory
			IMapper? validMapper = null;
			foreach(var mapper in _compositeMapperOptions.Mappers) {
				if(mapper.CanMapNew(sourceType, destinationType, mappingOptions)) {
					if(mapper is IMapperFactory factory)
						return factory.MapNewFactory(sourceType, destinationType, mappingOptions);
					else
						validMapper ??= mapper;
				}
			}

			// If we can map the types we return Map wrapped in a delegate
			if (validMapper != null) { 
				return new DefaultNewMapFactory(
					sourceType, destinationType,
					source => validMapper.Map(source, sourceType, destinationType, mappingOptions));
			}

			// Check if we can forward to merge map
			var mergeMapsHandling = mappingOptions.GetOptions<CompositeMapperMappingOptions>()?.MergeMapsHandling
				?? _compositeMapperOptions.MergeMapsHandling;

			if (mergeMapsHandling != MergeMapsHandling.DoNotMap &&
				(mergeMapsHandling != MergeMapsHandling.CreateDestination || ObjectFactory.CanCreate(destinationType))) {

				// Try retrieving a merge factory
				foreach (var mapper in _compositeMapperOptions.Mappers) {
					if (mapper.CanMapMerge(sourceType, destinationType, mappingOptions)) {
						if (mapper is IMapperFactory factory) {
							return factory.MapMergeFactory(sourceType, destinationType, mappingOptions)
								.MapNewFactory(mergeMapsHandling == MergeMapsHandling.CreateDestination, true);
						}
						else
							validMapper ??= mapper;
					}
				}

				// If we can map the types we return Map wrapped in a delegate
				if (validMapper != null) { 
					var destinationFactory = (mergeMapsHandling == MergeMapsHandling.CreateDestination ?
						(Func<object?>)ObjectFactory.CreateFactory(destinationType) :
						destinationType.GetDefault);

					return new DefaultNewMapFactory(
						sourceType, destinationType,
						source => validMapper.Map(source, sourceType, destinationFactory.Invoke(), destinationType, mappingOptions));
				}
			}

			throw new MapNotFoundException((sourceType, destinationType));
		}

		public IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			// Try retrieving a merge factory
			IMapper? validMapper = null;
			foreach (var mapper in _compositeMapperOptions.Mappers) {
				if (mapper.CanMapMerge(sourceType, destinationType, mappingOptions)) {
					if (mapper is IMapperFactory factory)
						return factory.MapMergeFactory(sourceType, destinationType, mappingOptions);
					else
						validMapper ??= mapper;
				}
			}

			// If we can map the types we return Map wrapped in a delegate
			if (validMapper != null) { 
				return new DefaultMergeMapFactory(
					sourceType, destinationType,
					(source, destination) => validMapper.Map(source, sourceType, destination, destinationType, mappingOptions));
			}
			else
				throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion

		#region IMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetNewMaps(MappingOptions? mappingOptions = null) {
			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var mergeMapsHandling = mappingOptions.GetOptions<CompositeMapperMappingOptions>()?.MergeMapsHandling
				?? _compositeMapperOptions.MergeMapsHandling;

			// Supports both new and merge maps
			IEnumerable<(Type From, Type To)> mergeMaps;
			if (mergeMapsHandling == MergeMapsHandling.DoNotMap)
				mergeMaps = [];
			else {
				mergeMaps = _compositeMapperOptions.Mappers.SelectMany(m => m.GetMergeMaps(mappingOptions));
				if (mergeMapsHandling == MergeMapsHandling.CreateDestination)
					mergeMaps = mergeMaps.Where(m => ObjectFactory.CanCreate(m.To));
			}

			return _compositeMapperOptions.Mappers.SelectMany(m => m.GetNewMaps(mappingOptions))
				.Concat(mergeMaps);
		}

		public IEnumerable<(Type From, Type To)> GetMergeMaps(MappingOptions? mappingOptions = null) {
			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			return _compositeMapperOptions.Mappers.SelectMany(m => m.GetMergeMaps(mappingOptions));
		}
		#endregion
	}
}
