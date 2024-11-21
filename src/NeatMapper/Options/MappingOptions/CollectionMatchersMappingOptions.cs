namespace NeatMapper {
	/// <summary>
	/// Options applied to <see cref="CollectionMatcher"/>.
	/// </summary>
	public sealed class CollectionMatchersMappingOptions {
		/// <summary>
		/// Creates a new instance of <see cref="CollectionMatchersMappingOptions"/>.
		/// </summary>
		/// <param name="nullEqualsEmpty">
		/// <inheritdoc cref="NullEqualsEmpty" path="/summary"/>
		/// <inheritdoc cref="NullEqualsEmpty" path="/remarks"/>
		/// </param>
		/// <param name="collectionMatchingOrder">
		/// <inheritdoc cref="CollectionMatchingOrder" path="/summary"/>
		/// <inheritdoc cref="CollectionMatchingOrder" path="/remarks"/>
		/// </param>
		public CollectionMatchersMappingOptions(bool? nullEqualsEmpty = null, CollectionMatchingOrder? collectionMatchingOrder = null) {
			NullEqualsEmpty = nullEqualsEmpty;
			CollectionMatchingOrder = collectionMatchingOrder;
		}


		/// <summary>
		/// <see langword="true"/> if <see langword="null"/> collections should match empty ones, false to distinguish them.
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting.</remarks>
		public bool? NullEqualsEmpty { get; init; }

		/// <inheritdoc cref="NeatMapper.CollectionMatchingOrder"/>
		/// <remarks><see langword="null"/> to use global setting.</remarks>
		public CollectionMatchingOrder? CollectionMatchingOrder { get; init; }
	}
}
