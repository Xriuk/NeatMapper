#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Base class for asynchronous mappers which map collections by mapping elements with another
	/// <see cref="IAsyncMapper"/>.
	/// Internal class.
	/// </summary>
	public abstract class AsyncCollectionMapper : IAsyncMapper {
		/// <summary>
		/// <see cref="IMapper"/> which is used to map the elements of the collections, will be also provided
		/// as a nested mapper in <see cref="AsyncMapperOverrideMappingOptions"/> (if not already present).
		/// </summary>
		protected readonly IAsyncMapper _elementsMapper;

		/// <summary>
		/// Default async options.
		/// </summary>
		protected readonly AsyncCollectionMappersOptions _asyncCollectionMappersOptions;

		/// <summary>
		/// Cached nested context with no parents.
		/// </summary>
		private readonly AsyncNestedMappingContext _nestedMappingContext;

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


		internal AsyncCollectionMapper(IAsyncMapper elementsMapper, AsyncCollectionMappersOptions asyncCollectionMappersOptions = null) {
			_elementsMapper = new AsyncCompositeMapper(elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper)), this);
			_asyncCollectionMappersOptions = asyncCollectionMappersOptions ?? new AsyncCollectionMappersOptions();
			_nestedMappingContext = new AsyncNestedMappingContext(this);
			_optionsCacheNull = MergeMappingOptions(MappingOptions.Empty);
		}


		public abstract Task<object> MapAsync(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default);
		public abstract Task<object> MapAsync(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default);


		// Will override the mapper if not already overridden
		protected MappingOptions MergeOrCreateMappingOptions(MappingOptions options) {
			if (options == null || options == MappingOptions.Empty) 
				return _optionsCacheNull;
			else if (options.Cached)
				return _optionsCache.GetOrAdd(options, MergeMappingOptions);
			else
				return MergeMappingOptions(options);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MappingOptions MergeMappingOptions(MappingOptions options) {
			return options.ReplaceOrAdd<AsyncMapperOverrideMappingOptions, AsyncNestedMappingContext>(
					m => m?.Mapper != null ? m : new AsyncMapperOverrideMappingOptions(_elementsMapper, m?.ServiceProvider),
					n => n != null ? new AsyncNestedMappingContext(_nestedMappingContext.ParentMapper, n) : _nestedMappingContext, options.Cached);
		}
	}
}
