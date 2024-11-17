using System;

namespace NeatMapper {
	/// <summary>
	/// Options applied to automatic asynchronous collections mapping.<br/>
	/// Can be overridden during mapping with <see cref="AsyncCollectionMappersMappingOptions"/>.
	/// </summary>
	public sealed class AsyncCollectionMappersOptions {
		/// <summary>
		/// Creates a new instance of <see cref="AsyncCollectionMappersOptions"/>.
		/// </summary>
		public AsyncCollectionMappersOptions() { }
		/// <summary>
		/// Creates a new instance of <see cref="AsyncCollectionMappersOptions"/> by copying options from another instance.
		/// </summary>
		/// <param name="other">Options to copy from.</param>
		public AsyncCollectionMappersOptions(AsyncCollectionMappersOptions other) {
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			MaxParallelMappings = other.MaxParallelMappings;
		}


		/// <summary>
		/// <para>
		/// Maximum number of parallel mappings inside a collection mapping,
		/// <see langword="1"/> means that the mappings will be sequential and not parallel.
		/// </para>
		/// <para>
		/// Parallel mappings should be used for long running operations, as they introduce a bit of overhead
		/// with the tasks and interlocking, so you should consider changing this option carefully.<br/>
		/// You may want to define explicit async maps to map collections instead.
		/// </para>
		/// <para>
		/// You can also override this option for single mappings by using
		/// <see cref="AsyncCollectionMappersMappingOptions.MaxParallelMappings"/>.
		/// </para>
		/// </summary>
		/// <remarks>Defaults to <see langword="1"/>.</remarks>
		public int MaxParallelMappings { get; set; } = 1;
	}
}
