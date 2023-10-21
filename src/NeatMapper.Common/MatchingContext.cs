#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

namespace NeatMapper {
	/// <summary>
	/// Contains informations and services for the current matching operation
	/// </summary>
	public class MatchingContext {
		internal MatchingContext() { }


		/// <summary>
		/// Service provider which can be used to retrieve additional services
		/// </summary>
		public IServiceProvider ServiceProvider { get; internal set; }

		/// <summary>
		/// Matcher which can be used for nested matches
		/// </summary>
		public IMatcher Matcher { get; internal set; }

		/// <summary>
		/// Additional mapping options, contains multiple options of different types,
		/// each mapper/map should try to retrieve its options and use them
		/// </summary>
		public MappingOptions MappingOptions { get; internal set; }
	}
}
