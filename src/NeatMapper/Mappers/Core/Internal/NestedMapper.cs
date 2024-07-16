using System;
using System.Collections.Concurrent;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which wraps another <see cref="IMapper"/> and overrides <see cref="MappingOptions"/>
	/// (and caches them).
	/// </summary>
	internal sealed class NestedMapper : IMapper, IMapperCanMap, IMapperFactory {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		/// <summary>
		/// <see cref="IMapper"/> to wrap.
		/// </summary>
		private readonly IMapper _mapper;

		/// <summary>
		/// Factory used to edit (or create) <see cref="MappingOptions"/> and apply them to <see cref="_mapper"/>.
		/// </summary>
		private readonly Func<MappingOptions, MappingOptions> _optionsFactory;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output <see cref="MappingOptions"/> from <see cref="_optionsFactory"/>.
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MappingOptions> _optionsCache =
			new ConcurrentDictionary<MappingOptions, MappingOptions>();

		/// <summary>
		/// Cached output <see cref="MappingOptions"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have null keys) and <see cref="MappingOptions.Empty"/> inputs,
		/// also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		private readonly MappingOptions _optionsCacheNull;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


		/// <summary>
		/// Creates a new instance of <see cref="NestedMapper"/>.
		/// </summary>
		/// <param name="mapper">Mapper to forward the actual mapping to.</param>
		/// <param name="optionsFactory">
		/// Method to invoke to alter the <see cref="MappingOptions"/> passed to the mapper,
		/// both the passed parameter and the returned value may be null.
		/// </param>
		public NestedMapper(
			IMapper mapper,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				MappingOptions?, MappingOptions?
#else
				MappingOptions, MappingOptions
#endif
			> optionsFactory) {

			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
			_optionsCacheNull = _optionsFactory.Invoke(MappingOptions.Empty);
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

			return _mapper.Map(source, sourceType, destinationType, GetOrCreateOptions(mappingOptions));
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

			return _mapper.Map(source, sourceType, destination, destinationType, GetOrCreateOptions(mappingOptions));
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

			return _mapper.CanMapNew(sourceType, destinationType, GetOrCreateOptions(mappingOptions));
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

			return _mapper.CanMapMerge(sourceType, destinationType, GetOrCreateOptions(mappingOptions));
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

			return _mapper.MapNewFactory(sourceType, destinationType, GetOrCreateOptions(mappingOptions));
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

			return _mapper.MapMergeFactory(sourceType, destinationType, GetOrCreateOptions(mappingOptions));
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private MappingOptions GetOrCreateOptions(MappingOptions options) {
			if(options == null || options == MappingOptions.Empty)
				return _optionsCacheNull;
			else if(options.Cached)
				return _optionsCache.GetOrAdd(options, _optionsFactory);
			else
				return _optionsFactory.Invoke(options);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
