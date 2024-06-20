namespace NeatMapper {
	/// <summary>
	/// Options applied to automatic collections mapping (async and normal).<br/>
	/// Can be overridden during mapping with <see cref="MergeCollectionsMappingOptions"/>.
	/// </summary>
	public sealed class MergeCollectionsOptions {
		/// <summary>
		/// Creates a new instance of <see cref="MergeCollectionsOptions"/>.
		/// </summary>
		public MergeCollectionsOptions() {}
		/// <summary>
		/// Creates a new instance of <see cref="MergeCollectionsOptions"/> by copying options from another instance.
		/// </summary>
		public MergeCollectionsOptions(MergeCollectionsOptions options) {
			RemoveNotMatchedDestinationElements = options.RemoveNotMatchedDestinationElements;
		}


		/// <summary>
		/// If <see langword="true"/>, will remove all the elements from destination which do not have
		/// a corresponding element in source, matched with an <see cref="IMatcher"/>
		/// (or <see cref="MergeCollectionsMappingOptions.Matcher"/>).
		/// </summary>
		/// <remarks>Defaults to <see langword="true"/>.</remarks>
		public bool RemoveNotMatchedDestinationElements { get; set; } = true;
	}
}
