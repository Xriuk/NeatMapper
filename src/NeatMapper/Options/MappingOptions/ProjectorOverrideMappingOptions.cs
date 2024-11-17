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
		public ProjectorOverrideMappingOptions(IProjector? projector = null, IServiceProvider? serviceProvider = null) {
			Projector = projector;
			ServiceProvider = serviceProvider;
		}


		/// <summary>
		/// Projector to be used for nested projections.
		/// </summary>
		/// <remarks><see langword="null"/> to use the one provided by the parent projector.</remarks>
		public IProjector? Projector {
			get;
#if NET5_0_OR_GREATER
			init;
#endif
		}

		/// <summary>
		/// Service provider to use for the maps.
		/// </summary>
		/// <remarks><see langword="null"/> to use the one provided by the parent projector.</remarks>
		public IServiceProvider? ServiceProvider {
			get;
#if NET5_0_OR_GREATER
			init;
#endif
		}
	}
}
