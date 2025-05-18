using System;

namespace NeatMapper {
	/// <summary>
	/// Contains informations about the current nested mapping operation.
	/// </summary>
	public sealed class NestedMappingContext {
		/// <summary>
		/// Creates a new instance of <see cref="NestedMappingContext"/>.
		/// </summary>
		/// <param name="parentMapper"><inheritdoc cref="ParentMapper" path="/summary"/></param>
		/// <param name="parentContext"><inheritdoc cref="ParentContext" path="/summary"/></param>
		/// <exception cref="ArgumentNullException"><paramref name="parentMapper"/> was null.</exception>
		public NestedMappingContext(IMapper parentMapper, NestedMappingContext? parentContext = null) {
			ParentMapper = parentMapper ?? throw new ArgumentNullException(nameof(parentMapper));
			ParentContext = parentContext;
		}


		/// <summary>
		/// Mapper which initiated the current nested mapping.
		/// </summary>
		public IMapper ParentMapper { get; }

		/// <summary>
		/// <see cref="NestedMappingContext"/> of the parent mapper, <see cref="ParentMapper"/>, if it itself
		/// was part of another nested mapping operation too, or <see langword="null"/>
		/// if this is the first nested mapping operation.
		/// </summary>
		public NestedMappingContext? ParentContext { get; }


		/// <summary>
		/// Checks if this context or any of its parents matches a given predicate.
		/// </summary>
		/// <param name="predicate">Condition to check on the context(s).</param>
		/// <returns>True if this context or any of its parents matches the given predicate.</returns>
		public bool CheckRecursive(Func<NestedMappingContext, bool> predicate) {
			return predicate.Invoke(this) || ParentContext?.CheckRecursive(predicate) == true;
		}
	}
}
