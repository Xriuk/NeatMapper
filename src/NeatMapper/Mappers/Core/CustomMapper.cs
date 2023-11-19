#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

namespace NeatMapper {
	/// <summary>
	/// Base class for mappers which use user-defined maps to map types.
	/// Internal class.
	/// </summary>
	public abstract class CustomMapper : IMapper {
		internal readonly CustomMapsConfiguration _configuration;
		protected readonly IServiceProvider _serviceProvider;

		internal CustomMapper(CustomMapsConfiguration configuration, IServiceProvider serviceProvider = null) {
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
		}


		public abstract object Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract object Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null);


		protected MappingContext CreateMappingContext(MappingOptions options) {
			var overrideOptions = options?.GetOptions<MapperOverrideMappingOptions>();
			return new MappingContext(
				overrideOptions?.ServiceProvider ?? _serviceProvider,
				overrideOptions?.Mapper ?? this,
				options ?? MappingOptions.Empty
			);
		}
	}
}
