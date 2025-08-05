using System;

namespace NeatMapper {
	/// <summary>
	/// Specifies the member(s) to retrieve.
	/// </summary>
	[Flags]
	public enum MemberVisibilityFlags {
		/// <summary>
		/// No member of the given type will be retrieved.
		/// </summary>
		None = 0,
		/// <summary>
		/// Only public members will be retrieved. Can be biwise OR'd with <see cref="NonPublic"/> to retrieve them all.
		/// </summary>
		Public = 1,
		/// <summary>
		/// Only non-public members will be retrieved (private, protected, internal).
		/// Can be biwise OR'd with <see cref="Public"/> to retrieve them all.
		/// </summary>
		NonPublic = 2
	}
}
