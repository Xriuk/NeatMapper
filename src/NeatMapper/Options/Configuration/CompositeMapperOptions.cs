using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Options used to define a list of <see cref="IMapper"/>s to use for <see cref="CompositeMapper"/>
	/// </summary>
	public sealed class CompositeMapperOptions {
		[Obsolete("This constant is no longer used and will be removed in future versions.")]
		public const string Base = "Base";


		/// <summary>
		/// Creates a new instance
		/// </summary>
		public CompositeMapperOptions() {
			Mappers = new List<IMapper>();
		}
		/// <summary>
		/// Creates a new instance by copying options from another instance
		/// </summary>
		/// <param name="options">Options to copy from</param>
		public CompositeMapperOptions(CompositeMapperOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Mappers = new List<IMapper>(options.Mappers);
		}


		public IList<IMapper> Mappers { get; set; }
	}
}
