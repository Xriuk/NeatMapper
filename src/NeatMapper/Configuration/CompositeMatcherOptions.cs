using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Options used to define a list of <see cref="IMatcher"/>s to use for <see cref="CompositeMapper"/>
	/// </summary>
	public sealed class CompositeMatcherOptions {
		/// <summary>
		/// Creates a new instance
		/// </summary>
		public CompositeMatcherOptions() {
			Matchers = new List<IMatcher>();
		}
		/// <summary>
		/// Creates a new instance by copying options from another instance
		/// </summary>
		/// <param name="options">Options to copy from</param>
		public CompositeMatcherOptions(CompositeMatcherOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Matchers = new List<IMatcher>(options.Matchers);
		}


		public IList<IMatcher> Matchers { get; set; }
	}
}
