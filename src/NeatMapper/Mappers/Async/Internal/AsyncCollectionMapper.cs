#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
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
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		protected readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		internal AsyncCollectionMapper(IAsyncMapper elementsMapper, AsyncCollectionMappersOptions asyncCollectionMappersOptions = null) {
			_elementsMapper = new AsyncCompositeMapper(elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper)), this);
			_asyncCollectionMappersOptions = asyncCollectionMappersOptions ?? new AsyncCollectionMappersOptions();
			var nestedMappingContext = new AsyncNestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<AsyncMapperOverrideMappingOptions, AsyncNestedMappingContext>(
				m => m?.Mapper != null ? m : new AsyncMapperOverrideMappingOptions(_elementsMapper, m?.ServiceProvider),
				n => n != null ? new AsyncNestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext, options.Cached));
		}


		public abstract Task<object> MapAsync(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default);
		public abstract Task<object> MapAsync(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default);
	}
}
