#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Concurrent;
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
		/// Cached input <see cref="MappingOptions"/> and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MappingOptions> _optionsCache =
			new ConcurrentDictionary<MappingOptions, MappingOptions>();

		/// <summary>
		/// Cached output <see cref="MappingOptions"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have a null key).
		/// </summary>
		private readonly MappingOptions _optionsCacheNull;


		internal AsyncCollectionMapper(IAsyncMapper elementsMapper, AsyncCollectionMappersOptions asyncCollectionMappersOptions = null) {
			_elementsMapper = new AsyncCompositeMapper(elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper)), this);
			_asyncCollectionMappersOptions = asyncCollectionMappersOptions ?? new AsyncCollectionMappersOptions();
			_nestedMappingContext = new AsyncNestedMappingContext(this);
			_optionsCacheNull = MergeOrCreateMappingOptions(MappingOptions.Empty, out _);
		}


		public abstract Task<object> MapAsync(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default);
		public abstract Task<object> MapAsync(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default);


		// Will override the mapper if not already overridden
		protected MappingOptions MergeOrCreateMappingOptions(MappingOptions options, out MergeCollectionsMappingOptions mergeCollectionsMappingOptions) {
			if (options == null) {
				mergeCollectionsMappingOptions = null;
				return _optionsCacheNull;
			}
			else {
				mergeCollectionsMappingOptions = options.GetOptions<MergeCollectionsMappingOptions>();
				return _optionsCache.GetOrAdd(options, opts => opts
					.Replace<MergeCollectionsMappingOptions>(m => new MergeCollectionsMappingOptions(m.RemoveNotMatchedDestinationElements, null))
					.ReplaceOrAdd<AsyncMapperOverrideMappingOptions, AsyncNestedMappingContext>(
						m => m?.Mapper != null ? m : new AsyncMapperOverrideMappingOptions(_elementsMapper, m?.ServiceProvider),
						n => n != null ? new AsyncNestedMappingContext(_nestedMappingContext.ParentMapper, n) : _nestedMappingContext));
			}
		}
	}
}
