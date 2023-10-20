using System;

namespace NeatMapper {
	/// <summary>
	/// Options which allow to override mapper for nested maps and service provider
	/// </summary>
	public sealed class MapperOverrideOptions {
		/// <summary>
		/// Mapper to be used for nested maps, null to use the one provided by the mapper
		/// </summary>
		public
#if NET5_0_OR_GREATER
			IMapper?
#else
			IMapper
#endif
			Mapper { get; set; }

		/// <summary>
		/// Service provider to use for the maps, null to use the one provided by the mapper
		/// </summary>
		public
#if NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			ServiceProvider { get; set; }
	}
}
