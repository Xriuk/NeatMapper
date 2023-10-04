using System.Threading;

namespace NeatMapper.Async {
	/// <summary>
	/// Contains informations and services for the current asynchronous mapping operation
	/// </summary>
	public sealed class AsyncMappingContext : MatchingContext {
		internal AsyncMappingContext() { }


		/// <summary>
		/// Mapper which can be used for nested mappings
		/// </summary>
		public IAsyncMapper Mapper { get; internal set; }
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			= null!;
#endif

		/// <summary>
		/// Cancellation token of the mapping which should be passed to all the async methods inside the maps
		/// </summary>
		public CancellationToken CancellationToken { get; internal set; }
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			= default;
#endif
	}
}
