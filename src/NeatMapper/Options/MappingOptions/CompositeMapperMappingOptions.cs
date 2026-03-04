namespace NeatMapper {
	/// <summary>
	/// Options applied to <see cref="CompositeMapper"/>.<br/>
	/// These will override any options defined in <see cref="CompositeMapperOptions"/>.
	/// </summary>
	public sealed class CompositeMapperMappingOptions {
		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapperMappingOptions"/>.
		/// </summary>
		/// <param name="mergeMapsHandling">
		/// <inheritdoc cref="MergeMapsHandling" path="/summary"/>
		/// <inheritdoc cref="MergeMapsHandling" path="/remarks"/>
		/// </param>
		public CompositeMapperMappingOptions(MergeMapsHandling? mergeMapsHandling = null) {
			MergeMapsHandling = mergeMapsHandling;
		}


		/// <summary>
		/// <inheritdoc cref="CompositeMapperOptions.MergeMapsHandling" path="/summary" />
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="CompositeMapperOptions"/>.</remarks>
		public MergeMapsHandling? MergeMapsHandling { get; init; }
	}
}
