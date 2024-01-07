using System;

namespace NeatMapper {
	/// <summary>
	/// Options for <see cref="IMapper"/> which allow to override mapper and service provider
	/// inside the created <see cref="MappingContext"/>.
	/// </summary>
	public sealed class MapperOverrideMappingOptions {
		public MapperOverrideMappingOptions(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IMapper?
#else
			IMapper
#endif
			mapper = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) {
			
			Mapper = mapper;
			ServiceProvider = serviceProvider;
		}


		/// <summary>
		/// Mapper to be used for nested maps, null to use the one provided by the mapper.
		/// </summary>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IMapper?
#else
			IMapper
#endif
			Mapper { get;
#if NET5_0_OR_GREATER
				init;
#endif
		}

		/// <summary>
		/// Service provider to use for the maps, null to use the one provided by the mapper.
		/// </summary>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			ServiceProvider { get;
#if NET5_0_OR_GREATER
				init;
#endif
		}
	}
}
