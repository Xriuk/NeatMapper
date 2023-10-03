using NeatMapper.Common.Mapper;

namespace NeatMapper {
	/// <summary>
	/// Contains informations and services for the current matching operation
	/// </summary>
	public class MatchingContext : BaseContext {
		internal MatchingContext() { }

		/// <summary>
		/// Matcher which can be used for nested matches
		/// </summary>
		public IMatcher Matcher { get; internal set; }
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			= null!;
#endif
	}
}
