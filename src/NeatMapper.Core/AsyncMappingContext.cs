namespace NeatMapper.Core {
	public sealed class AsyncMappingContext {
		internal AsyncMappingContext() { }

		/// <summary>
		/// Mapper which can be used for nested mappings
		/// </summary>
		public IAsyncMapper Mapper { get; internal set; } = null!;

		/// <summary>
		/// Scoped service provider which can be used to retrieve additional services
		/// </summary>
		public IServiceProvider ServiceProvider { get; internal set; } = null!;

		/// <summary>
		/// Cancellation token of the mapping which should be passed to all the async methods inside the maps
		/// </summary>
		public CancellationToken CancellationToken { get; internal set; } = default;
	}
}
