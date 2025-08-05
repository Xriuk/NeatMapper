namespace NeatMapper {
	/// <summary>
	/// Specifies how to handle deep object.
	/// </summary>
	public enum DeepCopyFlags {
		/// <summary>
		/// Will just copy values as they are. Effectively the same as <see cref="IdentityMapper"/>.
		/// </summary>
		None = 0,
		/// <summary>
		/// Will map values deeply by also mapping nested objects.
		/// </summary>
		DeepMap = 1,
		/// <summary>
		/// Will always override the destination value by using new maps, if not set will try to merge the values.
		/// </summary>
		OverrideInstance = 2
	}
}
