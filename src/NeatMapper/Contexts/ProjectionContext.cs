using System;

namespace NeatMapper {
	/// <summary>
	/// Contains informations and services for the current projection operation.
	/// </summary>
	public sealed class ProjectionContext {
		public ProjectionContext(IServiceProvider serviceProvider, IProjector projector, MappingOptions mappingOptions) :
			this(serviceProvider, new NestedProjector(projector ?? throw new ArgumentNullException(nameof(projector))), mappingOptions) {}
		public ProjectionContext(IServiceProvider serviceProvider, NestedProjector projector, MappingOptions mappingOptions) {
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			Projector = projector ?? throw new ArgumentNullException(nameof(projector));
			MappingOptions = mappingOptions ?? throw new ArgumentNullException(nameof(mappingOptions));
		}


		/// <summary>
		/// Service provider which can be used to retrieve additional services.
		/// </summary>
		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Projector to be used for nested projections.
		/// Can be used as a regular <see cref="IProjector"/>, the invocations will be replaced with the expanded maps.
		/// </summary>
		public NestedProjector Projector { get; }

		/// <summary>
		/// Additional mapping options, contains multiple options of different types,
		/// each projector/map should try to retrieve its options and use them.
		/// </summary>
		public MappingOptions MappingOptions { get; }
	}
}
