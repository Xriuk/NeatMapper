namespace NeatMapper.Expressions {
	public sealed class ProjectionContext {
		public ProjectionContext(IServiceProvider serviceProvider, IProjector projector) :
			this(serviceProvider, new NestedProjector(projector ?? throw new ArgumentNullException(nameof(projector)))) {}
		public ProjectionContext(IServiceProvider serviceProvider, NestedProjector projector) {
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			Projector = projector ?? throw new ArgumentNullException(nameof(projector));
		}



		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Projector to be used for nested projections.
		/// </summary>
		public NestedProjector Projector { get; }
	}
}
