﻿namespace NeatMapper {
	/// <summary>
	/// Options applied to automatic asynchronous collections mapping, these will override any configuration options defined in <see cref="AsyncCollectionMappersOptions"/>
	/// </summary>
	public sealed class AsyncCollectionMappersMappingOptions {
		public AsyncCollectionMappersMappingOptions() { }
		public AsyncCollectionMappersMappingOptions(AsyncCollectionMappersMappingOptions other) {
			MaxParallelMappings = other.MaxParallelMappings;
		}


		/// <summary>
		/// Maximum number of parallel mappings inside a collection mapping
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting</remarks>
		public int? MaxParallelMappings { get; set; }
	}
}
