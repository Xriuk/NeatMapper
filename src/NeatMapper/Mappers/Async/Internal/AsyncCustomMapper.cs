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
		/// Cached input <see cref="MappingOptions"/> and output pool of <see cref="AsyncMappingContext"/>.<br/>S
		/// A pool is needed for each <see cref="MappingOptions"/> because <see cref="CancellationToken"/> can change,
		/// so we reuse <see cref="AsyncMappingContext"/>s by changing <see cref="AsyncMappingContext.CancellationToken"/>.
		/// </summary>
		internal readonly ConcurrentDictionary<MappingOptions, ObjectPool<AsyncMappingContext>> _contextsCache
			= new ConcurrentDictionary<MappingOptions, ObjectPool<AsyncMappingContext>>();

		/// <summary>
		/// Cached output pool of <see cref="AsyncMappingContext"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have a null key), also provides faster access since locking isn't needed
		/// for thread-safety.<br/>
		/// A pool is needed for each <see cref="MappingOptions"/> because <see cref="CancellationToken"/> can change,
		/// so we reuse <see cref="AsyncMappingContext"/>s by changing <see cref="AsyncMappingContext.CancellationToken"/>.
		/// </summary>
		internal readonly ObjectPool<AsyncMappingContext> _contextsCacheNull;


		internal AsyncCustomMapper(CustomMapsConfiguration configuration, IServiceProvider serviceProvider = null) {
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
			_contextsCacheNull = new ObjectPool<AsyncMappingContext>(() => GetOrCreateMappingContext(MappingOptions.Empty, default));
		}


		public abstract Task<object> MapAsync(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default);
		public abstract Task<object> MapAsync(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken  = default);


		internal ObjectPool<AsyncMappingContext> GetMappingOptionsPool(MappingOptions options) {
			if (options == null)
				return _contextsCacheNull;
			else {
				return _contextsCache.GetOrAdd(options, opts => new ObjectPool<AsyncMappingContext>(() => {
					var overrideOptions = opts.GetOptions<AsyncMapperOverrideMappingOptions>();
					return new AsyncMappingContext(
						overrideOptions?.ServiceProvider ?? _serviceProvider,
						overrideOptions?.Mapper ?? this,
						this,
						opts,
						default
					);
				}));
			}
		}

		protected AsyncMappingContext GetOrCreateMappingContext(MappingOptions options, CancellationToken cancellationToken) {
			var context = GetMappingOptionsPool(options).Get();
			context.CancellationToken = cancellationToken;
			return context;
		}
	}
}
