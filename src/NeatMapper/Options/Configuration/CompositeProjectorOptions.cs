using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Options used to define a list of <see cref="IProjector"/>s to use for <see cref="CompositeProjector"/>
	/// </summary>
	public sealed class CompositeProjectorOptions {
		/// <summary>
		/// Creates a new instance
		/// </summary>
		public CompositeProjectorOptions() {
			Projectors = new List<IProjector>();
		}
		/// <summary>
		/// Creates a new instance by copying options from another instance
		/// </summary>
		/// <param name="options">Options to copy from</param>
		public CompositeProjectorOptions(CompositeProjectorOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Projectors = new List<IProjector>(options.Projectors);
		}


		public IList<IProjector> Projectors { get; set; }
	}
}
