#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections;

namespace NeatMapper {
	/// <summary>
	/// Base class for mappers which use user-defined maps to map types
	/// </summary>
	public abstract class CustomMapper : IMapper {
		internal readonly CustomMapsConfiguration _configuration;
		protected readonly IServiceProvider _serviceProvider;

		internal CustomMapper(CustomMapsConfiguration configuration, IServiceProvider serviceProvider = null) {
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
		}


		public abstract object Map(object source, Type sourceType, Type destinationType, IEnumerable mappingOptions = null);
		public abstract object Map(object source, Type sourceType, object destination, Type destinationType, IEnumerable mappingOptions = null);


		protected MappingContext CreateMappingContext(IEnumerable mappingOptions) {
			var options = new MappingOptions(mappingOptions);
			var overrideOptions = options.GetOptions<MapperOverrideMappingOptions>();
			return new MappingContext {
				Mapper = overrideOptions?.Mapper ?? this,
				ServiceProvider = overrideOptions?.ServiceProvider ?? _serviceProvider,
				MappingOptions = options
			};
		}
	}
}
