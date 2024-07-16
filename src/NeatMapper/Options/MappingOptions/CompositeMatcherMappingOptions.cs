namespace NeatMapper {
	public sealed class CompositeMatcherMappingOptions {
		/// <summary>
		/// Creates a new instance of <see cref="CompositeMatcherMappingOptions"/>.
		/// </summary>
		/// <param name="reverseTypes">
		/// <inheritdoc cref="ReverseTypes" path="/summary"/>
		/// <inheritdoc cref="ReverseTypes" path="/remarks"/>
		/// </param>
		public CompositeMatcherMappingOptions(bool? reverseTypes = null) {
			ReverseTypes = reverseTypes;
		}


		/// <summary>
		/// If <see langword="true"/>, if none of the matchers will succeed in mapping the given
		/// types, the types will be inverted and a new search will be performed.
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="CompositeMatcherOptions"/>.</remarks>
		public bool? ReverseTypes {
			get;
#if NET5_0_OR_GREATER
			init;
#endif
		}
	}
}
