using System;

namespace NeatMapper {
	/// <summary>
	/// Options applied to automatic collections merge mappings (async and normal).<br/>
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
		/// <param name="options">Options to copy from.</param>
		public MergeCollectionsOptions(MergeCollectionsOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			RemoveNotMatchedDestinationElements = options.RemoveNotMatchedDestinationElements;
		}


		/// <summary>
		/// If <see langword="true"/>, will remove all the elements from the destination collection
		/// which do not have a corresponding element in the source collection.
		/// Matched with an <see cref="IMatcher"/> (or <see cref="MergeCollectionsMappingOptions.Matcher"/>).
		/// </summary>
		/// <remarks>Defaults to <see langword="true"/>.</remarks>
		public bool RemoveNotMatchedDestinationElements { get; set; } = true;
	}
}
