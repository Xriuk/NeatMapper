#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;

namespace NeatMapper {
	/// <summary>
	/// Contains informations and services for the current asynchronous mapping operation
	/// </summary>
	public sealed class AsyncMappingContext {
		internal AsyncMappingContext() { }


		/// <summary>
		/// Service provider which can be used to retrieve additional services
		/// </summary>
		public IServiceProvider ServiceProvider { get; internal set; }

		/// <summary>
		/// Mapper which can be used for nested mappings
		/// </summary>
		public IAsyncMapper Mapper { get; internal set; }

		/// <summary>
		/// Additional mapping options, contains multiple options of different types,
		/// each mapper/map should try to retrieve its options and use them
		/// </summary>
		public MappingOptions MappingOptions { get; internal set; }

		/// <summary>
		/// Cancellation token of the mapping which should be passed to all the async methods inside the maps
		/// </summary>
		public CancellationToken CancellationToken { get; internal set; }
	}
}
