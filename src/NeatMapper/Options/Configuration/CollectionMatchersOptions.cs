using System;

namespace NeatMapper {
	/// <summary>
	/// Options applied to <see cref="CollectionMatcher"/>.<br/>
	/// Can be overridden during mapping with <see cref="CollectionMatchersMappingOptions"/>.
	/// </summary>
	public sealed class CollectionMatchersOptions {
		/// <summary>
		/// Creates a new instance of <see cref="CollectionMatchersOptions"/>.
		/// </summary>
		public CollectionMatchersOptions() {
			NullEqualsEmpty = true;
			CollectionMatchingOrder = CollectionMatchingOrder.Default;
		}
		/// <summary>
		/// Creates a new instance of <see cref="CollectionMatchersOptions"/> by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from.</param>
		public CollectionMatchersOptions(CollectionMatchersOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			NullEqualsEmpty = options.NullEqualsEmpty;
			CollectionMatchingOrder = options.CollectionMatchingOrder;
		}


		/// <summary>
		/// <see langword="true"/> if <see langword="null"/> collections should match empty ones, false to distinguish them.
		/// </summary>
		/// <remarks>Defaults to <see langword="true"/>.</remarks>
		public bool NullEqualsEmpty { get; set; }

		/// <inheritdoc cref="NeatMapper.CollectionMatchingOrder"/>
		/// <remarks>Defaults to <see cref="CollectionMatchingOrder.Default"/>.</remarks>
		public CollectionMatchingOrder CollectionMatchingOrder { get; set; }
	}
}
