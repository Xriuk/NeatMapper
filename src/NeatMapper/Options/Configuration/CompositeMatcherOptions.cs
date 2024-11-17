using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Options applied on creation to <see cref="CompositeMatcher"/>.<br/>
	/// Can be overridden during mapping with <see cref="CompositeMatcherMappingOptions"/>.
	/// </summary>
	public sealed class CompositeMatcherOptions {
		/// <summary>
		/// Creates a new instance of <see cref="CompositeMatcherOptions"/>.
		/// </summary>
		public CompositeMatcherOptions() {
			Matchers = [];
			ReverseTypes = true;
		}
		/// <summary>
		/// Creates a new instance of <see cref="CompositeMatcherOptions"/> by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from.</param>
		public CompositeMatcherOptions(CompositeMatcherOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Matchers = new List<IMatcher>(options.Matchers ?? throw new InvalidOperationException("Matchers cannot be null"));
			ReverseTypes = options.ReverseTypes;
		}


		/// <summary>
		/// Ordered list of matchers, each matcher will be tried and the first one to succeed will match the types.
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
