using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Options used to define a list of <see cref="IProjector"/>s to use for <see cref="CompositeProjector"/>
	/// </summary>
	public sealed class CompositeProjectorOptions {
		/// <summary>
		/// Creates a new instance of <see cref="CompositeProjectorOptions"/>.
		/// </summary>
		public CompositeProjectorOptions() {
			Projectors = [];
		}
		/// <summary>
		/// Creates a new instance of <see cref="CompositeProjectorOptions"/> by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from</param>
		public CompositeProjectorOptions(CompositeProjectorOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Projectors = new List<IProjector>(options.Projectors ?? throw new InvalidOperationException("Projectors cannot be null"));
		}


		/// <summary>
		/// Ordered list of projectors, each projector will be tried and the first one to succeed will project the types.
		/// </summary>
		public IList<IProjector> Projectors { get; set; }
	}
}
