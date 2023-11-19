#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

namespace NeatMapper {
	/// <summary>
	/// Contains informations and services for the current mapping operation.
	/// </summary>
	public sealed class MappingContext {
		public MappingContext(IServiceProvider serviceProvider, IMapper mapper, MappingOptions mappingOptions) {
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			MappingOptions = mappingOptions ?? throw new ArgumentNullException(nameof(mappingOptions));
		}


		/// <summary>
		/// Service provider which can be used to retrieve additional services.
		/// </summary>
		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Mapper which can be used for nested mappings.
		/// </summary>
		public IMapper Mapper { get; }

		/// <summary>
		/// Additional mapping options, contains multiple options of different types,
		/// each mapper/map should try to retrieve its options and use them.
		/// </summary>
		public MappingOptions MappingOptions { get; }
	}
}
