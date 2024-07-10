#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Concurrent;
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
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output <see cref="AsyncMappingContextOptions"/>.
		/// </summary>
		internal readonly ConcurrentDictionary<MappingOptions, AsyncMappingContextOptions> _contextsCache
			= new ConcurrentDictionary<MappingOptions, AsyncMappingContextOptions>();

		/// <summary>
		/// Cached output <see cref="AsyncMappingContextOptions"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have null keys) and <see cref="MappingOptions.Empty"/>,
		/// also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		internal readonly AsyncMappingContextOptions _contextsCacheNull;


		internal AsyncCustomMapper(CustomMapsConfiguration configuration, IServiceProvider serviceProvider = null) {
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
			_contextsCacheNull = GetOrCreateMappingContextOptions(MappingOptions.Empty);
		}


		public abstract Task<object> MapAsync(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default);
		public abstract Task<object> MapAsync(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken  = default);


		protected AsyncMappingContextOptions GetOrCreateMappingContextOptions(MappingOptions options) {
			if (options == null || options == MappingOptions.Empty)
				return _contextsCacheNull;
			else if(options.Cached)
				return _contextsCache.GetOrAdd(options, CreateMappingContextOptions);
			else
				return CreateMappingContextOptions(options);
		}

		private AsyncMappingContextOptions CreateMappingContextOptions(MappingOptions options) {
			var overrideOptions = options.GetOptions<AsyncMapperOverrideMappingOptions>();
			return new AsyncMappingContextOptions(
				overrideOptions?.ServiceProvider ?? _serviceProvider,
				overrideOptions?.Mapper ?? this,
				this,
				options
			);
		}
	}
}
