using System;

namespace NeatMapper {
	/// <summary>
	/// Options for <see cref="IProjector"/> which allow to override projector and service provider
	/// inside the created <see cref="ProjectionContext"/>
	/// </summary>
	public sealed class ProjectorOverrideMappingOptions {
		public ProjectorOverrideMappingOptions(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IProjector?
#else
			IProjector
#endif
			projector = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) {

			Projector = projector;
			ServiceProvider = serviceProvider;
		}


		/// <summary>
		/// Projector to be used for nested projections, null to use the one provided by the projector
		/// </summary>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IProjector?
#else
			IProjector
#endif
			Projector { get;
#if NET5_0_OR_GREATER
				init;
#endif
		}

		/// <summary>
		/// Service provider to use for the maps, null to use the one provided by the projector
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
