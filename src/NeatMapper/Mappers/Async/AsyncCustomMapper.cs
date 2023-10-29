#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Base class for asynchronous mappers which use user-defined maps to map types
	/// </summary>
	public abstract class AsyncCustomMapper : IAsyncMapper {
		internal readonly CustomMapsConfiguration _configuration;
		protected readonly IServiceProvider _serviceProvider;

		internal AsyncCustomMapper(CustomMapsConfiguration configuration, IServiceProvider serviceProvider = null) {
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
		}


		public abstract Task<object> MapAsync(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default);
		public abstract Task<object> MapAsync(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken  = default);


		protected AsyncMappingContext CreateMappingContext(MappingOptions options, CancellationToken cancellationToken) {
			var overrideOptions = options?.GetOptions<AsyncMapperOverrideMappingOptions>();
			return new AsyncMappingContext {
				Mapper = overrideOptions?.Mapper ?? this,
				ServiceProvider = overrideOptions?.ServiceProvider ?? _serviceProvider,
				MappingOptions = options ?? MappingOptions.Empty,
				CancellationToken = cancellationToken
			};
		}
	}
}
