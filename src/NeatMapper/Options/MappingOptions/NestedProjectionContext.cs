using System;

namespace NeatMapper {
	/// <summary>
	/// Contains informations about the current nested projection operation
	/// </summary>
	public sealed class NestedProjectionContext {
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
		/// Projector which initiated the current nested projection
		/// </summary>
		public IProjector ParentProjector { get; }

		/// <summary>
		/// <see cref="NestedProjectionContext"/> of the parent projector, if it was part of another nested projection operation too
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
