using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Options applied to <see cref="AsyncCompositeMapper"/>.<br/>
	/// Can be overridden during mapping with <see cref=AsyncCompositeMapperMappingOptions"/>.
	/// </summary>
	public sealed class AsyncCompositeMapperOptions {
		/// <summary>
		/// Creates a new instance of <see cref="AsyncCompositeMapperOptions"/>.
		/// </summary>
		public AsyncCompositeMapperOptions() {
			Mappers = [];
			MergeMapsHandling = MergeMapsHandling.CreateDestination;
		}
		/// <summary>
		/// Creates a new instance of <see cref="AsyncCompositeMapperOptions"/> by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from.</param>
		public AsyncCompositeMapperOptions(AsyncCompositeMapperOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Mappers = new List<IAsyncMapper>(options.Mappers ?? throw new InvalidOperationException("Mappers cannot be null"));
			MergeMapsHandling = options.MergeMapsHandling;
		}


		/// <summary>
		/// Ordered list of mappers, each mapper will be tried and the first one to succeed will map the types.
		/// </summary>
		public IList<IAsyncMapper> Mappers { get; set; }

		/// <summary>
		/// Specifies how to handle merge maps from new maps.
		/// </summary>
		/// <remarks>Defaults to <see cref="MergeMapsHandling.CreateDestination"/>.</remarks>
		public MergeMapsHandling MergeMapsHandling { get; set; }
	}
}
