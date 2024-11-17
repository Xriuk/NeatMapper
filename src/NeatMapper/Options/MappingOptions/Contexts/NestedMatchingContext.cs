using System;

namespace NeatMapper {
	/// <summary>
	/// Contains informations about the current nested matching operation.
	/// </summary>
	public sealed class NestedMatchingContext {
		/// <summary>
		/// Creates a new instance of <see cref="NestedMatchingContext"/>.
		/// </summary>
		/// <param name="parentMatcher"><inheritdoc cref="ParentMatcher" path="/summary"/></param>
		/// <param name="parentContext"><inheritdoc cref="ParentContext" path="/summary"/></param>
		/// <exception cref="ArgumentNullException"><paramref name="parentMatcher"/> was null.</exception>
		public NestedMatchingContext(IMatcher parentMatcher, NestedMatchingContext? parentContext = null) {
			ParentMatcher = parentMatcher ?? throw new ArgumentNullException(nameof(parentMatcher));
			ParentContext = parentContext;
		}


		/// <summary>
		/// Matcher which initiated the current nested matching.
		/// </summary>
		public IMatcher ParentMatcher { get; }

		/// <summary>
		/// <see cref="NestedMatchingContext"/> of the <see cref="ParentMatcher"/>, if it itself
		/// was part of another nested matching operation too, or <see langword="null"/>
		/// if this is the first nested matching operation.
		/// </summary>
		public NestedMatchingContext? ParentContext { get; }
	}
}
