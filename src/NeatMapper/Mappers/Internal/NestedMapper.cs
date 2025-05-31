using System;
using System.Collections.Generic;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which wraps another <see cref="IMapper"/> and overrides <see cref="MappingOptions"/>
	/// (and caches them).
	/// </summary>
	internal sealed class NestedMapper : IMapper, IMapperFactory, IMapperMaps {
		/// <summary>
		/// <see cref="IMapper"/> to wrap.
		/// </summary>
		private readonly IMapper _mapper;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		/// <summary>
		/// Creates a new instance of <see cref="NestedMapper"/>.
		/// </summary>
		/// <param name="mapper">Mapper to forward the actual mapping to.</param>
		/// <param name="optionsFactory">
		/// Method to invoke to alter the <see cref="MappingOptions"/> passed to the mapper.
		/// </param>
		public NestedMapper(IMapper mapper, Func<MappingOptions, MappingOptions> optionsFactory) {
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(
				optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory)));
		}


		#region IMapper methods
		public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return _mapper.CanMapNew(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions));
		}

		public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return _mapper.CanMapMerge(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions));
		}

		public object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return _mapper.Map(source, sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions));
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			return _mapper.Map(source, sourceType, destination, destinationType, _optionsCache.GetOrCreate(mappingOptions));
		}
		#endregion

		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return _mapper.MapNewFactory(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions));
		}

		public IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return _mapper.MapMergeFactory(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions));
		}
		#endregion

		#region IMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetNewMaps(MappingOptions? mappingOptions = null) {
			return _mapper.GetNewMaps(_optionsCache.GetOrCreate(mappingOptions));
		}

		public IEnumerable<(Type From, Type To)> GetMergeMaps(MappingOptions? mappingOptions = null) {
			return _mapper.GetMergeMaps(_optionsCache.GetOrCreate(mappingOptions));
		}
		#endregion
	}
}
