namespace NeatMapper {
	/// <summary>
	/// Options applied to automatic asynchronous collections mapping, these will override
	/// any configuration options defined in <see cref="AsyncCollectionMappersOptions"/>.
	/// </summary>
	public sealed class AsyncCollectionMappersMappingOptions {
		public AsyncCollectionMappersMappingOptions(int? maxParallelMappings = null) {
			MaxParallelMappings = maxParallelMappings;
		}


		/// <summary>
		/// Maximum number of parallel mappings inside a collection mapping,
		/// <see langword="1"/> means that the mappings will be sequential and not parallel.<br/>
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting</remarks>
		public int? MaxParallelMappings {
			get;
#if NET5_0_OR_GREATER
			init;
#endif
		}
	}
}
