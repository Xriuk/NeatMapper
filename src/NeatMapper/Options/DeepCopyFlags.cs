namespace NeatMapper {
	/// <summary>
	/// Specifies how to handle deep objects.
	/// </summary>
	public enum DeepCopyFlags {
		/// <summary>
		/// Will just copy values as they are. Effectively the same as <see cref="IdentityMapper"/>.
		/// </summary>
		None,
		/// <summary>
		/// Will map values deeply by also mapping nested objects with a merge map.
		/// </summary>
		DeepMap,
		/// <summary>
		/// Will map values deeply by also mapping nested objects with a new map.
		/// </summary>
		OverrideInstance // DEV: maybe rename? Together with the whole enum
	}
}
