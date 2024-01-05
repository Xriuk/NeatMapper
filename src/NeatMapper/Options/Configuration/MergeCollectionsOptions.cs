namespace NeatMapper {
	/// <summary>
	/// Options applied to automatic collections mapping.<br/>
	/// Can be overridden during mapping with <see cref="MergeCollectionsMappingOptions"/>.
	/// </summary>
	public sealed class MergeCollectionsOptions {
		public MergeCollectionsOptions() { }
		public MergeCollectionsOptions(MergeCollectionsOptions options) {
			RemoveNotMatchedDestinationElements = options.RemoveNotMatchedDestinationElements;
		}


		/// <summary>
		/// If <see langword="true"/>, will remove all the elements from destination which do not have
		/// a corresponding element in source, matched with <see cref="IMatchMap{TSource, TDestination}"/>.
		/// </summary>
		/// <remarks>Defaults to <see langword="true"/>.</remarks>
		public bool RemoveNotMatchedDestinationElements { get; set; } = true;
	}
}
