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
		public AsyncMappingContext(IServiceProvider serviceProvider, IAsyncMapper mapper, MappingOptions mappingOptions, CancellationToken cancellationToken) {
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			MappingOptions = mappingOptions ?? throw new ArgumentNullException(nameof(mappingOptions));
			CancellationToken = cancellationToken;
		}


		/// <summary>
		/// Service provider which can be used to retrieve additional services
		/// </summary>
		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Mapper which can be used for nested mappings
		/// </summary>
		public IAsyncMapper Mapper { get; }

		/// <summary>
		/// Additional mapping options, contains multiple options of different types,
		/// each mapper/map should try to retrieve its options and use them
		/// </summary>
		public MappingOptions MappingOptions { get; }

		/// <summary>
		/// Cancellation token of the mapping which should be passed to all the async methods inside the maps
		/// </summary>
		public CancellationToken CancellationToken { get; }
	}
}
