namespace NeatMapper {
	/// <summary>
	/// Options applied to <see cref="AsyncCompositeMapper"/>.<br/>
	/// These will override any options defined in <see cref="AsyncCompositeMapperOptions"/>.
	/// </summary>
	public sealed class AsyncCompositeMapperMappingOptions {
		/// <summary>
		/// Creates a new instance of <see cref="AsyncCompositeMapperMappingOptions"/>.
		/// </summary>
		/// <param name="mergeMapsHandling">
		/// <inheritdoc cref="MergeMapsHandling" path="/summary"/>
		/// <inheritdoc cref="MergeMapsHandling" path="/remarks"/>
		/// </param>
		public AsyncCompositeMapperMappingOptions(MergeMapsHandling? mergeMapsHandling = null) {
			MergeMapsHandling = mergeMapsHandling;
		}


		/// <summary>
		/// <inheritdoc cref="AsyncCompositeMapperOptions.MergeMapsHandling" path="/summary" />
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="AsyncCompositeMapperOptions"/>.</remarks>
		public MergeMapsHandling? MergeMapsHandling { get; init; }
	}
}
