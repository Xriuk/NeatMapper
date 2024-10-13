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
		/// <summary>
		/// Configuration for class and additional maps for the mapper.
		/// </summary>
		internal readonly CustomMapsConfiguration _configuration;

		/// <summary>
		/// Service provider available in the created <see cref="MappingContext"/>s.
		/// </summary>
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> and output <see cref="MappingContext"/>.
		/// </summary>
		protected readonly MappingOptionsFactoryCache<MappingContext> _contextsCache;


		internal CustomMapper(CustomMapsConfiguration configuration, IServiceProvider serviceProvider = null) {
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
			_contextsCache = new MappingOptionsFactoryCache<MappingContext>(options => {
				var overrideOptions = options.GetOptions<MapperOverrideMappingOptions>();
				return new MappingContext(
					overrideOptions?.ServiceProvider ?? _serviceProvider,
					overrideOptions?.Mapper ?? this,
					this,
					options
				);
			});
		}


		public abstract bool CanMapNew(Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract object Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract object Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null);
	}
}
