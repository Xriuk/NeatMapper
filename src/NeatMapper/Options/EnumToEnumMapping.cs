namespace NeatMapper {
	/// <summary>
	/// Type of mapping to use when mapping an enum to another enum.
	/// </summary>
	public enum EnumToEnumMapping {
		/// <summary>
		/// Matches underlying numeric values, in case of duplicate destination values, the first member
		/// is used, in the order they are declared in the enum.
		/// </summary>
		Value,
		/// <summary>
		/// Matches enum names, first by checking <see cref="System.Runtime.Serialization.EnumMemberAttribute"/>,
		/// or <see cref="System.ComponentModel.DataAnnotations.DisplayAttribute"/> attributes of the member,
		/// then its declared name. Matching is done in a case-sensitive way.
		/// </summary>
		NameCaseSensitive,
		/// <summary>
		/// Matches enum names, first by checking <see cref="System.Runtime.Serialization.EnumMemberAttribute"/>,
		/// or <see cref="System.ComponentModel.DataAnnotations.DisplayAttribute"/> attributes of the member,
		/// then its declared name. Matching is done in a case-insensitive way.
		/// </summary>
		NameCaseInsensitive
	}
}
