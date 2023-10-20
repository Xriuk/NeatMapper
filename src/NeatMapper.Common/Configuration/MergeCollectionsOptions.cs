namespace NeatMapper.Configuration {
	/// <summary>
	/// Options applied to automatic collections mapping
	/// </summary>
	public sealed class MergeCollectionsOptions {
		public MergeCollectionsOptions() { }
		public MergeCollectionsOptions(MergeCollectionsOptions options) {
			RemoveNotMatchedDestinationElements = options.RemoveNotMatchedDestinationElements;
		}


		/// <summary>
		/// If true, will remove all the elements from destination which do not have a corresponding element in source,
		/// matched with <see cref="IMatchMap{TSource, TDestination}"/>
		/// </summary>
		/// <remarks>Defaults to true</remarks>
		public bool RemoveNotMatchedDestinationElements { get; set; } = true;
	}
}
