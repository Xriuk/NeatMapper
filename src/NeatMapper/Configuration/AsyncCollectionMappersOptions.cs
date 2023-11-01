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
		/// <see langword="1"/> means that the mappings will be sequential and not parallel.<br/>
		/// Parallel mappings should be used for long running operations, as they introduce a bit of overhead
		/// with the tasks and interlocking, so you should consider changing this option carefully.<br/>
		/// You can also override this option for single mappings by using <see cref="AsyncCollectionMappersMappingOptions"/>
		/// </summary>
		/// <remarks>Defaults to <see langword="1"/></remarks>
		public int MaxParallelMappings { get; set; } = 1;
	}
}
