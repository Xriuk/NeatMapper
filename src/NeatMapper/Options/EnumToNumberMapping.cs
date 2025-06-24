namespace NeatMapper {
	/// <summary>
	/// Type of mapping to use when mapping an enum to its underlying numeric type.
	/// </summary>
	public enum EnumToNumberMapping {
		/// <summary>
		/// Uses the value of the enum member.
		/// </summary>
		Value,
		/// <summary>
		/// Hashes the name of the enum member and resizes the hash to fit the underlying numeric type.
		/// </summary>
		HashedName
	}
}
