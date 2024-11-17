using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Options used to define a list of <see cref="IMapper"/>s to use for <see cref="CompositeMapper"/>
	/// </summary>
	public sealed class CompositeMapperOptions {
		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapperOptions"/>.
		/// </summary>
		public CompositeMapperOptions() {
			Mappers = [];
		}
		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapperOptions"/> by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from.</param>
		public CompositeMapperOptions(CompositeMapperOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Mappers = new List<IMapper>(options.Mappers ?? throw new InvalidOperationException("Mappers cannot be null"));
		}


		/// <summary>
		/// Ordered list of mappers, each mapper will be tried and the first one to succeed will map the types.
		/// </summary>
		public IList<IMapper> Mappers { get; set; }
	}
}
