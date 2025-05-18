namespace NeatMapper {
	/// <summary>
	/// Type of the ordering to use during matching. In every mode, the number of elements of the source
	/// and destination collections must be the same.
	/// </summary>
	public enum CollectionMatchingOrder {
		/// <summary>
		/// If at least one of the collections involved in the match is ordered (ie: it implements
		/// <see cref="System.Collections.Generic.IList{T}"/> or
		/// <see cref="System.Collections.Generic.IReadOnlyList{T}"/>), then the matching is
		/// <see cref="Ordered"/>, otherwise <see cref="NotOrdered"/>.
		/// </summary>
		Default,
		/// <summary>
		/// Both collections are treated as ordered, with the order returned by 
		/// <see cref="System.Collections.IEnumerable"/>: elements of the source must have a corresponding element
		/// in the same index in the destination.
		/// </summary>
		Ordered,
		/// <summary>
		/// Both collections are treated as unordered: elements of the source must have a corresponding element
		/// anywhere in the destination, also elements of the destination must have a corresponding element
		/// anywhere in the source.
		/// </summary>
		NotOrdered
	}
}
