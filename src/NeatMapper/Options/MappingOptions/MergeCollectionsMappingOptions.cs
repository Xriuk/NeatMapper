namespace NeatMapper {
	/// <summary>
	/// Options applied to automatic collections merge mappings (async and normal).<br/
	/// These will override any configuration options defined in <see cref="MergeCollectionsOptions"/>.
	/// </summary>
	public sealed class MergeCollectionsMappingOptions{
		/// <inheritdoc cref="MergeCollectionsMappingOptions(bool?, IMatcher?, bool?)" />
		public MergeCollectionsMappingOptions(bool? removeNotMatchedDestinationElements, IMatcher? matcher) 
			: this(removeNotMatchedDestinationElements, matcher, null) {}
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
		/// <param name="recreateReadonlyDestination">
		/// <inheritdoc cref="RecreateReadonlyDestination" path="/summary"/>
		/// <inheritdoc cref="RecreateReadonlyDestination" path="/remarks"/>
		/// </param>
		public MergeCollectionsMappingOptions(
			bool? removeNotMatchedDestinationElements = null,
			IMatcher? matcher = null,
			bool? recreateReadonlyDestination = null) {

			RemoveNotMatchedDestinationElements = removeNotMatchedDestinationElements;
			Matcher = matcher;
			RecreateReadonlyDestination = recreateReadonlyDestination;
		}


		/// <summary>
		/// <inheritdoc cref="MergeCollectionsOptions.RemoveNotMatchedDestinationElements" path="/summary" />
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="MergeCollectionsOptions"/>.</remarks>
		public bool? RemoveNotMatchedDestinationElements { get; init; }

		/// <summary>
		/// <see cref="IMatcher"/> to be used to match elements of the collections.
		/// </summary>
		/// <remarks><see langword="null"/> to use the default <see cref="IMatcher"/> (if any).</remarks>
		public IMatcher? Matcher { get; init; }

		/// <summary>
		/// <inheritdoc cref="MergeCollectionsOptions.RecreateReadonlyDestination" path="/summary" />
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="MergeCollectionsOptions"/>.</remarks>
		public bool? RecreateReadonlyDestination { get; init; }
	}
}
