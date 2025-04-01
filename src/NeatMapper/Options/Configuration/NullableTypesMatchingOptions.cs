using System;

namespace NeatMapper {
	/// <summary>
	/// Options to handle <see cref="Nullable{T}"/> types matching.<br/>
	/// Can be overridden during matching with <see cref="NullableTypesMatchingMappingOptions"/>.
	/// </summary>
	public sealed class NullableTypesMatchingOptions {
		/// <summary>
		/// Creates a new instance of <see cref="NullableTypesMatchingOptions"/>.
		/// </summary>
		public NullableTypesMatchingOptions() {
			SupportNullableTypes = true;
		}
		/// <summary>
		/// Creates a new instance of <see cref="NullableTypesMatchingOptions"/> by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from.</param>
		public NullableTypesMatchingOptions(NullableTypesMatchingOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			SupportNullableTypes = options.SupportNullableTypes;
		}


		/// <summary>
		/// Allows matching <see cref="Nullable{T}"/> types automatically if the corresponding wrapped type
		/// is supported:
		/// <list type="bullet">
		/// <item>
		/// If both source and destination are <see langword="null"/> the matcher will return
		/// <see langword="true"/>.
		/// </item>
		/// <item>
		/// If one of source or destination is <see langword="null"/> the matcher will return
		/// <see langword="false"/>.
		/// </item>
		/// <item>
		/// Otherwise the non-nullable <see cref="IEquatable{T}"/> or
		/// <see cref="System.Collections.Generic.IEqualityComparer{T}"/> will be invoked, with the unwrapped
		/// <see cref="Nullable{T}.Value"/>.
		/// </item>
		/// </list>
		/// If an explicit match for the <see cref="Nullable{T}"/> is available, it is used instead.
		/// </summary>
		/// <remarks>Defaults to <see langword="true"/>.</remarks>
		public bool SupportNullableTypes { get; set; }
	}
}
