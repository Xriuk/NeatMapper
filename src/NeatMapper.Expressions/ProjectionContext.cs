namespace NeatMapper.Expressions {
	public sealed class ProjectionContext {
		internal ProjectionContext() { }


		public INestedProjectionMapper Mapper { get; internal set; } = null!;

		public IServiceProvider ServiceProvider { get; internal set; } = null!;
	}
}
