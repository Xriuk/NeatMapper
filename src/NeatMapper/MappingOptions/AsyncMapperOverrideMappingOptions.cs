using System;

namespace NeatMapper {
	/// <summary>
	/// Options for <see cref="IAsyncMapper"/> which allow to override mapper and service provider
	/// inside the created <see cref="AsyncMappingContext"/>
	/// </summary>
	public sealed class AsyncMapperOverrideMappingOptions {
		/// <summary>
		/// Mapper to be used for nested maps, null to use the one provided by the mapper
		/// </summary>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IAsyncMapper?
#else
			IAsyncMapper
#endif
			Mapper { get; set; }

		/// <summary>
		/// Service provider to use for the maps, null to use the one provided by the mapper
		/// </summary>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			ServiceProvider { get; set; }
	}
}
