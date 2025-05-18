using System;

namespace NeatMapper {
	/// <summary>
	/// Contains informations about the current nested projection operation.
	/// </summary>
	public sealed class NestedProjectionContext {
		/// <summary>
		/// Creates a new instance of <see cref="NestedProjectionContext"/>.
		/// </summary>
		/// <param name="parentProjector"><inheritdoc cref="ParentProjector" path="/summary"/></param>
		/// <param name="parentContext"><inheritdoc cref="ParentContext" path="/summary"/></param>
		/// <exception cref="ArgumentNullException"><paramref name="parentProjector"/> was null.</exception>
		public NestedProjectionContext(IProjector parentProjector, NestedProjectionContext? parentContext = null) {
			ParentProjector = parentProjector ?? throw new ArgumentNullException(nameof(parentProjector));
			ParentContext = parentContext;
		}


		/// <summary>
		/// Projector which initiated the current nested projection.
		/// </summary>
		public IProjector ParentProjector { get; }

		/// <summary>
		/// <see cref="NestedProjectionContext"/> of the <see cref="ParentProjector"/>, if it itself
		/// was part of another nested projection operation too, or <see langword="null"/>
		/// if this is the first nested projection operation.
		/// </summary>
		public NestedProjectionContext? ParentContext { get; }


		/// <summary>
		/// Checks if this context or any of its parents matches a given predicate.
		/// </summary>
		/// <param name="predicate">Condition to check on the context(s).</param>
		/// <returns>True if this context or any of its parents matches the given predicate.</returns>
		public bool CheckRecursive(Func<NestedProjectionContext, bool> predicate) {
			return predicate.Invoke(this) || ParentContext?.CheckRecursive(predicate) == true;
		}
	}
}
