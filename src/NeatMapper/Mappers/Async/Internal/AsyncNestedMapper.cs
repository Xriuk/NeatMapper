using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Concurrent;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which wraps another <see cref="IAsyncMapper"/> and overrides some <see cref="MappingOptions"/>.
	/// </summary>
	internal sealed class AsyncNestedMapper : IAsyncMapper, IAsyncMapperCanMap, IAsyncMapperFactory {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		/// <summary>
		/// <see cref="IMapper"/> to wrap.
		/// </summary>
		private readonly IAsyncMapper _mapper;

		/// <summary>
		/// Factory used to edit (or create) <see cref="MappingOptions"/> and apply them to <see cref="_mapper"/>.
		/// </summary>
		private readonly Func<MappingOptions, MappingOptions> _optionsFactory;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/> from <see cref="_optionsFactory"/>.
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MappingOptions> _optionsCache = new ConcurrentDictionary<MappingOptions, MappingOptions>();

		/// <summary>
		/// Cached output <see cref="MappingOptions"/> for the <see langword="null"/> input <see cref="MappingOptions"/>
		/// (since a dictionary can't have a null key), also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		private readonly MappingOptions _optionsCacheNull;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


		/// <summary>
		/// Creates a new instance of <see cref="AsyncNestedMapper"/>.
		/// </summary>
		/// <param name="mapper">Mapper to forward the actual mapping to.</param>
		/// <param name="optionsFactory">
		/// Method to invoke to alter the <see cref="MappingOptions"/> passed to the mapper,
		/// both the passed parameter and the returned value may be null.
		/// </param>
		public AsyncNestedMapper(
			IAsyncMapper mapper,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				MappingOptions?, MappingOptions?
#else
				MappingOptions, MappingOptions
#endif
			> optionsFactory) {

			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
			_optionsCacheNull = _optionsFactory.Invoke(null);
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

			return _mapper.MapAsync(source, sourceType, destinationType, GetOrCreateOptions(mappingOptions), cancellationToken);
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

			return _mapper.MapAsync(source, sourceType, destination, destinationType, GetOrCreateOptions(mappingOptions), cancellationToken);
		}
		#endregion

		#region IAsyncMapperCanMap methods
		public Task<bool> CanMapAsyncNew(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return _mapper.CanMapAsyncNew(sourceType, destinationType, GetOrCreateOptions(mappingOptions), cancellationToken);
		}

		public Task<bool> CanMapAsyncMerge(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return _mapper.CanMapAsyncMerge(sourceType, destinationType, GetOrCreateOptions(mappingOptions), cancellationToken);
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

			return _mapper.MapAsyncNewFactory(sourceType, destinationType, GetOrCreateOptions(mappingOptions), cancellationToken);
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

			return _mapper.MapAsyncMergeFactory(sourceType, destinationType, GetOrCreateOptions(mappingOptions), cancellationToken);
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		/// <summary>
		/// Retrieves cached cached options or apples <see cref="_optionsFactory"/> on them.
		/// </summary>
		/// <param name="mappingOptions">Input options to check.</param>
		/// <returns>Cached or created and cached resulting options.</returns>
		private MappingOptions GetOrCreateOptions(MappingOptions mappingOptions) {
			if (mappingOptions == null)
				return _optionsCacheNull;
			else
				return _optionsCache.GetOrAdd(mappingOptions, _optionsFactory);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
