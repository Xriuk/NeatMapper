using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Options used to define a list of <see cref="IAsyncMapper"/>s to use for <see cref="AsyncCompositeMapper"/>
	/// </summary>
	public sealed class AsyncCompositeMapperOptions {
		/// <summary>
		/// Creates a new instance
		/// </summary>
		public AsyncCompositeMapperOptions() {
			Mappers = new List<IAsyncMapper>();
		}
		/// <summary>
		/// Creates a new instance by copying options from another instance
		/// </summary>
		/// <param name="options">Options to copy from</param>
		public AsyncCompositeMapperOptions(AsyncCompositeMapperOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Mappers = new List<IAsyncMapper>(options.Mappers);
		}


		public IList<IAsyncMapper> Mappers { get; set; }
	}
}
