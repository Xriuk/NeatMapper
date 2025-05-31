using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which wraps another <see cref="IAsyncMapper"/> and overrides some <see cref="MappingOptions"/>.
	/// </summary>
	internal sealed class AsyncNestedMapper : IAsyncMapper, IAsyncMapperFactory, IAsyncMapperMaps {
		/// <summary>
		/// <see cref="IMapper"/> to wrap.
		/// </summary>
		private readonly IAsyncMapper _mapper;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		/// <summary>
		/// Creates a new instance of <see cref="AsyncNestedMapper"/>.
		/// </summary>
		/// <param name="mapper">Mapper to forward the actual mapping to.</param>
		/// <param name="optionsFactory">
		/// Method to invoke to alter the <see cref="MappingOptions"/> passed to the mapper.
		/// </param>
		public AsyncNestedMapper(IAsyncMapper mapper, Func<MappingOptions, MappingOptions> optionsFactory) {
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(
				optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory)));
		}


		#region IAsyncMapper methods
		public bool CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return _mapper.CanMapAsyncNew(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions));
		}

		public bool CanMapAsyncMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return _mapper.CanMapAsyncMerge(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions));
		}

		public Task<object?> MapAsync(
			object? source,
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return _mapper.MapAsync(source, sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions), cancellationToken);
		}

		public Task<object?> MapAsync(
			object? source,
			Type sourceType,
			object? destination,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return _mapper.MapAsync(source, sourceType, destination, destinationType, _optionsCache.GetOrCreate(mappingOptions), cancellationToken);
		}
		#endregion

		#region IAsyncMapperFactory methods
		public IAsyncNewMapFactory MapAsyncNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return _mapper.MapAsyncNewFactory(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions));
		}

		public IAsyncMergeMapFactory MapAsyncMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return _mapper.MapAsyncMergeFactory(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions));
		}
		#endregion

		#region IAsyncMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetAsyncNewMaps(MappingOptions? mappingOptions = null) {
			return _mapper.GetAsyncNewMaps(_optionsCache.GetOrCreate(mappingOptions));
		}

		public IEnumerable<(Type From, Type To)> GetAsyncMergeMaps(MappingOptions? mappingOptions = null) {
			return _mapper.GetAsyncMergeMaps(_optionsCache.GetOrCreate(mappingOptions));
		}
		#endregion
	}
}
