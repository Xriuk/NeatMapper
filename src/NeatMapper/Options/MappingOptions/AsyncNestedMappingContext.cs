using System;

namespace NeatMapper {
	/// <summary>
	/// Contains informations about the current nested asynchronous mapping operation
	/// </summary>
	public sealed class AsyncNestedMappingContext {
		public AsyncNestedMappingContext(IAsyncMapper parentMapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			AsyncNestedMappingContext?
#else
			AsyncNestedMappingContext
#endif
			parentContext = null) {

			ParentMapper = parentMapper ?? throw new ArgumentNullException(nameof(parentMapper));
			ParentContext = parentContext;
		}


		/// <summary>
		/// Mapper which initiated the current nested asynchronous mapping
		/// </summary>
		public IAsyncMapper ParentMapper { get; }

		/// <summary>
		/// <see cref="AsyncNestedMappingContext"/> of the parent mapper, if it was part of another nested asynchronous mapping operation too
		/// </summary>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			AsyncNestedMappingContext?
#else
			AsyncNestedMappingContext
#endif
			ParentContext { get; }
	}
}
