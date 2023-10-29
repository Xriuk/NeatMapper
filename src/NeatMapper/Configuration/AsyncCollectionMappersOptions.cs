namespace NeatMapper {
	/// <summary>
	/// Options applied to automatic asynchronous collections mapping
	/// </summary>
	public sealed class AsyncCollectionMappersOptions {
		public AsyncCollectionMappersOptions() { }
		public AsyncCollectionMappersOptions(AsyncCollectionMappersOptions other) {
			MaxParallelMappings = other.MaxParallelMappings;
		}


		/// <summary>
		/// Maximum number of parallel mappings inside a collection mapping,
		/// 1 means that the mappings will be sequential and not parallel.<br/>
		/// Unless you need to map a lot of elements it is recommended to keep it at 1,
		/// because there is some overhead with semaphores and locks to map elements in parallel.<br/>
		/// Consider also using <see cref="AsyncCollectionMappersMappingOptions"/> to override this setting for specific mappings
		/// </summary>
		/// <remarks>Defaults to <see langword="1"/></remarks>
		public int MaxParallelMappings { get; set; } = 1;
	}
}
