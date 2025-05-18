using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Options used to define a list of <see cref="IAsyncMapper"/>s to use for <see cref="AsyncCompositeMapper"/>.
	/// </summary>
	public sealed class AsyncCompositeMapperOptions {
		/// <summary>
		/// Creates a new instance of <see cref="AsyncCompositeMapperOptions"/>.
		/// </summary>
		public AsyncCompositeMapperOptions() {
			Mappers = [];
		}
		/// <summary>
		/// Creates a new instance of <see cref="AsyncCompositeMapperOptions"/> by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from.</param>
		public AsyncCompositeMapperOptions(AsyncCompositeMapperOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Mappers = new List<IAsyncMapper>(options.Mappers ?? throw new InvalidOperationException("Mappers cannot be null"));
		}


		/// <summary>
		/// Ordered list of mappers, each mapper will be tried and the first one to succeed will map the types.
		/// </summary>
		public IList<IAsyncMapper> Mappers { get; set; }
	}
}
