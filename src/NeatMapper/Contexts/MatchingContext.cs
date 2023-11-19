#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

namespace NeatMapper {
	/// <summary>
	/// Contains informations and services for the current matching operation
	/// </summary>
	public sealed class MatchingContext {
		public MatchingContext(IServiceProvider serviceProvider, IMatcher matcher, MappingOptions mappingOptions) {
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			Matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
			MappingOptions = mappingOptions ?? throw new ArgumentNullException(nameof(mappingOptions));
		}


		/// <summary>
		/// Service provider which can be used to retrieve additional services
		/// </summary>
		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Matcher which can be used for nested matches
		/// </summary>
		public IMatcher Matcher { get; }

		/// <summary>
		/// Additional mapping options, contains multiple options of different types,
		/// each mapper/map should try to retrieve its options and use them
		/// </summary>
		public MappingOptions MappingOptions { get; }
	}
}
