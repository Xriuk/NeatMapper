using System;

namespace NeatMapper {
	/// <summary>
	/// Contains informations about the current nested mapping operation
	/// </summary>
	public sealed class NestedMappingContext {
		public NestedMappingContext(IMapper parentMapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			NestedMappingContext?
#else
			NestedMappingContext
#endif
			parentContext = null) {

			ParentMapper = parentMapper ?? throw new ArgumentNullException(nameof(parentMapper));
			ParentContext = parentContext;
		}


		/// <summary>
		/// Mapper which initiated the current nested mapping
		/// </summary>
		public IMapper ParentMapper { get; }

		/// <summary>
		/// <see cref="NestedMappingContext"/> of the parent mapper, if it was part of another nested mapping operation too
		/// </summary>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			NestedMappingContext?
#else
			NestedMappingContext
#endif
			ParentContext { get; }
	}
}
