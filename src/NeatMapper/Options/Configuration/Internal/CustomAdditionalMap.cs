namespace NeatMapper {
	/// <summary>
	/// Additional maps defined outside classes (delegates, compiled expressions, ...)
	/// </summary>
	internal sealed class CustomAdditionalMap : CustomMap {
		/// <summary>
		/// If <see langword="true"/> and the map was already defined will throw an exception, otherwise will simply ignore it
		/// </summary>
		public bool ThrowOnDuplicate { get; set; }
	}
}
