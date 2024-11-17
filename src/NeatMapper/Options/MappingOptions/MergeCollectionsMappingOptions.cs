namespace NeatMapper {
	/// <summary>
	/// Options applied to automatic collections merge mappings (async and normal).<br/
	/// These will override any configuration options defined in <see cref="MergeCollectionsOptions"/>.
	/// </summary>
	public sealed class MergeCollectionsMappingOptions{
		/// <summary>
		/// Creates a new instance of <see cref="MergeCollectionsMappingOptions"/>.
		/// </summary>
		/// <param name="removeNotMatchedDestinationElements">
		/// <inheritdoc cref="RemoveNotMatchedDestinationElements" path="/summary"/>
		/// <inheritdoc cref="RemoveNotMatchedDestinationElements" path="/remarks"/>
		/// </param>
		/// <param name="matcher">
		/// <inheritdoc cref="Matcher" path="/summary"/>
		/// <inheritdoc cref="Matcher" path="/remarks"/>
		/// </param>
		public MergeCollectionsMappingOptions(bool? removeNotMatchedDestinationElements = null, IMatcher? matcher = null) {
			RemoveNotMatchedDestinationElements = removeNotMatchedDestinationElements;
			Matcher = matcher;
		}


		/// <summary>
		/// If <see langword="true"/>, will remove all the elements from destination which do not have a corresponding element in source,
		/// matched with <see cref="IMatchMap{TSource, TDestination}"/> (or <see cref="Matcher"/>).
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="MergeCollectionsOptions"/>.</remarks>
		public bool? RemoveNotMatchedDestinationElements {
			get;
#if NET5_0_OR_GREATER
			init;
#endif
		}

		/// <summary>
		/// <see cref="IMatcher"/> to be used to match elements of the collections.
		/// </summary>
		/// <remarks><see langword="null"/> to use the default <see cref="IMatcher"/> (if any).</remarks>
		public IMatcher? Matcher {
			get;
#if NET5_0_OR_GREATER
			init;
#endif
		}
	}
}
