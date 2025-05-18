namespace NeatMapper {
	/// <summary>
	/// Options applied to automatic asynchronous collections mapping, these will override
	/// any configuration options defined in <see cref="AsyncCollectionMappersOptions"/>.
	/// </summary>
	public sealed class AsyncCollectionMappersMappingOptions {
		/// <summary>
		/// Creates a new instance of <see cref="AsyncCollectionMappersMappingOptions"/>.
		/// </summary>
		/// <param name="maxParallelMappings">
		/// <inheritdoc cref="MaxParallelMappings" path="/summary"/><inheritdoc cref="MaxParallelMappings" path="/remarks"/>
		/// </param>
		public AsyncCollectionMappersMappingOptions(int? maxParallelMappings = null) {
			MaxParallelMappings = maxParallelMappings;
		}


		/// <summary>
		/// Maximum number of parallel mappings inside a collection mapping,
		/// <see langword="1"/> means that the mappings will be sequential and not parallel.<br/>
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="AsyncCollectionMappersMappingOptions"/>.</remarks>
		public int? MaxParallelMappings { get; init; }
	}
}
