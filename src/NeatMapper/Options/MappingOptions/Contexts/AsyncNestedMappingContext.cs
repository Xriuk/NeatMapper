﻿using System;

namespace NeatMapper {
	/// <summary>
	/// Contains informations about the current nested asynchronous mapping operation.
	/// </summary>
	public sealed class AsyncNestedMappingContext {
		/// <summary>
		/// Creates a new instance of <see cref="AsyncNestedMappingContext"/>.
		/// </summary>
		/// <param name="parentMapper"><inheritdoc cref="ParentMapper" path="/summary"/></param>
		/// <param name="parentContext"><inheritdoc cref="ParentContext" path="/summary"/></param>
		/// <exception cref="ArgumentNullException"><paramref name="parentMapper"/> was null.</exception>
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
		/// Mapper which initiated the current nested asynchronous mapping.
		/// </summary>
		public IAsyncMapper ParentMapper { get; }

		/// <summary>
		/// <see cref="AsyncNestedMappingContext"/> of the <see cref="ParentMapper"/>, if it itself
		/// was part of another nested asynchronous mapping operation too, or <see langword="null"/>
		/// if this is the first nested asynchronous mapping operation.
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
