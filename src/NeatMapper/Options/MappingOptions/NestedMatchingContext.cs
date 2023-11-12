namespace NeatMapper {
	/// <summary>
	/// Contains informations about the current nested matching operation
	/// </summary>
	public sealed class NestedMatchingContext {
		public NestedMatchingContext(IMatcher parentMatcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			NestedMatchingContext?
#else
			NestedMatchingContext
#endif
			parentContext = null) {

			ParentMatcher = parentMatcher;
			ParentContext = parentContext;
		}


		/// <summary>
		/// Matcher which initiated the current nested matching
		/// </summary>
		public IMatcher ParentMatcher { get; }

		/// <summary>
		/// <see cref="NestedMatchingContext"/> of the parent matcher, if it was part of another nested matching operation too
		/// </summary>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			NestedMatchingContext?
#else
			NestedMatchingContext
#endif
			ParentContext { get; }
	}
}
