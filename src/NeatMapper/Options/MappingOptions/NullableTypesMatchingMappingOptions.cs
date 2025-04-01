namespace NeatMapper {
	public sealed class NullableTypesMatchingMappingOptions {
		/// <summary>
		/// Creates a new instance of <see cref="NullableTypesMatchingMappingOptions"/>.
		/// </summary>
		/// <param name="supportNullableTypes">
		/// <inheritdoc cref="SupportNullableTypes" path="/summary"/>
		/// <inheritdoc cref="SupportNullableTypes" path="/remarks"/>
		/// </param>
		public NullableTypesMatchingMappingOptions(bool? supportNullableTypes = null) {
			SupportNullableTypes = supportNullableTypes;
		}


		/// <summary>
		/// <inheritdoc cref="NullableTypesMatchingOptions.SupportNullableTypes" path="/summary" />
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="NullableTypesMatchingOptions"/>.</remarks>
		public bool? SupportNullableTypes { get; init; }
	}
}
