namespace NeatMapper.Core {
	public sealed class MappingContext {
		internal MappingContext() { }

		public IMapper Mapper { get; internal set; } = null!;

		public IServiceProvider ServiceProvider { get; internal set; } = null!;

		//public CancellationToken CancellationToken { get; internal set; } = default;
	}
}
