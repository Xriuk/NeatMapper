namespace NeatMapper.Core {
	public sealed class AsyncMappingContext {
		internal AsyncMappingContext() { }


		public IAsyncMapper Mapper { get; internal set; } = null!;

		public IServiceProvider ServiceProvider { get; internal set; } = null!;

		public CancellationToken CancellationToken { get; internal set; } = default;
	}
}
