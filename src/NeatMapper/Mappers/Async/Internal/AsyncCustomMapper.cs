#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Base class for asynchronous mappers which use user-defined maps to map types.
	/// Internal class.
	/// </summary>
	public abstract class AsyncCustomMapper : IAsyncMapper {
		/// <summary>
		/// Configuration for class and additional maps for the mapper.
		/// </summary>
		internal readonly CustomMapsConfiguration _configuration;

		/// <summary>
		/// Service provider available in the created <see cref="AsyncMappingContext"/>s.
		/// </summary>
		protected readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> and output <see cref="AsyncMappingContextOptions"/>.
		/// </summary>
		protected readonly MappingOptionsFactoryCache<AsyncMappingContextOptions> _contextsCache;


		internal AsyncCustomMapper(CustomMapsConfiguration configuration, IServiceProvider serviceProvider = null) {
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
			_contextsCache = new MappingOptionsFactoryCache<AsyncMappingContextOptions>(options => {
				var overrideOptions = options.GetOptions<AsyncMapperOverrideMappingOptions>();
				return new AsyncMappingContextOptions(
					overrideOptions?.ServiceProvider ?? _serviceProvider,
					overrideOptions?.Mapper ?? this,
					this,
					options
				);
			});
		}


		public abstract Task<object> MapAsync(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default);
		public abstract Task<object> MapAsync(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken  = default);
	}
}
