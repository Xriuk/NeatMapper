namespace NeatMapper {
	/// <summary>
	/// Specifies how to forward new maps to merge maps.
	/// </summary>
	public enum MergeMapsHandling {
		/// <summary>
		/// Create a destination instance (destination type must have a parameterless constructor)
		/// and use that in the merge map.
		/// </summary>
		CreateDestination,
		/// <summary>
		/// Use the default value for the destination type in the map.
		/// </summary>
		DefaultDestination,
		/// <summary>
		/// Do not forward to the merge map.
		/// </summary>
		DoNotMap
	}
}
