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
		public NestedProjectionContext(IProjector parentProjector,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			NestedProjectionContext?
#else
			NestedProjectionContext
#endif
			parentContext = null) {

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
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			NestedProjectionContext?
#else
			NestedProjectionContext
#endif
			ParentContext { get; }
	}
}
