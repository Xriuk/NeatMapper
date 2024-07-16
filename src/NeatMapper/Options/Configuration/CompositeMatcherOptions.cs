using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Options applied on creation to <see cref="CompositeMatcher"/>.<br/>
	/// Can be overridden during mapping with <see cref="CompositeMatcherMappingOptions"/>.
	/// </summary>
	public sealed class CompositeMatcherOptions {
		/// <summary>
		/// Creates a new instance
		/// </summary>
		public CompositeMatcherOptions() {
			Matchers = new List<IMatcher>();
			ReverseTypes = true;
		}
		/// <summary>
		/// Creates a new instance by copying options from another instance
		/// </summary>
		/// <param name="options">Options to copy from</param>
		public CompositeMatcherOptions(CompositeMatcherOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Matchers = new List<IMatcher>(options.Matchers ?? throw new InvalidOperationException("Matchers cannot be null"));
			ReverseTypes = options.ReverseTypes;
		}


		/// <summary>
		/// <see cref="IMatcher"/>s to use to match types. Each matcher is invoked in order and the first one
		/// to succeed in matching is returned.
		/// </summary>
		public IList<IMatcher> Matchers { get; set; }

		/// <summary>
		/// If <see langword="true"/>, if none of the <see cref="Matchers"/> will succeed in mapping the given
		/// types, the types will be inverted and a new search will be performed.
		/// </summary>
		/// <remarks>Defaults to <see langword="true"/>.</remarks>
		public bool ReverseTypes { get; set; }
	}
}
