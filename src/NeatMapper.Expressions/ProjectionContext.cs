namespace NeatMapper.Expressions {
	public sealed class ProjectionContext {
		public ProjectionContext(IServiceProvider serviceProvider, IProjector mapper) {
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}


		public IServiceProvider ServiceProvider { get; }

		public IProjector Mapper { get; }
	}
}
