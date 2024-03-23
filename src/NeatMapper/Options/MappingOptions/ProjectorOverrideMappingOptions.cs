using System;

namespace NeatMapper {
	/// <summary>
	/// Options for <see cref="IProjector"/> which allow to override projector and service provider
	/// inside the created <see cref="ProjectionContext"/>.
	/// </summary>
	public sealed class ProjectorOverrideMappingOptions {
		/// <summary>
		/// Creates a new instance of <see cref="ProjectorOverrideMappingOptions"/>.
		/// </summary>
		/// <param name="projector">
		/// <inheritdoc cref="Projector" path="/summary"/><inheritdoc cref="Projector" path="/remarks"/>
		/// </param>
		/// <param name="serviceProvider">
		/// <inheritdoc cref="ServiceProvider" path="/summary"/><inheritdoc cref="ServiceProvider" path="/remarks"/>
		/// </param>
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
		/// Projector to be used for nested projections.
		/// </summary>
		/// <remarks><see langword="null"/> to use the one provided by the parent projector.</remarks>
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
		/// Service provider to use for the maps.
		/// </summary>
		/// <remarks><see langword="null"/> to use the one provided by the parent projector.</remarks>
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
