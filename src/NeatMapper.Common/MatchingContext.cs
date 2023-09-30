using NeatMapper.Common.Mapper;

namespace NeatMapper {
	public class MatchingContext : BaseContext {
		internal MatchingContext() { }

		/// <summary>
		/// Matcher which can be used for nested matches
		/// </summary>
		public IMatcher Matcher { get; internal set; } = null!;
	}
}
