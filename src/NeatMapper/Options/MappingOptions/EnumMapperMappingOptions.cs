namespace NeatMapper {
	/// <summary>
	/// Options applied to <see cref="EnumMapper"/>.<br/>
	/// These will override any options defined in <see cref="EnumMapperOptions"/>.
	/// </summary>
	public sealed class EnumMapperMappingOptions {
		/// <summary>
		/// Creates a new instance of <see cref="EnumMapperMappingOptions"/>.
		/// </summary>
		/// <param name="stringToEnumCaseInsensitive">
		/// <inheritdoc cref="StringToEnumCaseInsensitive" path="/summary"/>
		/// <inheritdoc cref="StringToEnumCaseInsensitive" path="/remarks"/>
		/// </param>
		/// <param name="enumToNumberMapping">
		/// <inheritdoc cref="EnumToNumberMapping" path="/summary"/>
		/// <inheritdoc cref="EnumToNumberMapping" path="/remarks"/>
		/// </param>
		/// <param name="enumToEnumMapping">
		/// <inheritdoc cref="EnumToEnumMapping" path="/summary"/>
		/// <inheritdoc cref="EnumToEnumMapping" path="/remarks"/>
		/// </param>
		public EnumMapperMappingOptions(
			bool? stringToEnumCaseInsensitive = null,
			EnumToNumberMapping? enumToNumberMapping = null,
			EnumToEnumMapping? enumToEnumMapping = null) {

			StringToEnumCaseInsensitive = stringToEnumCaseInsensitive;
			EnumToNumberMapping = enumToNumberMapping;
			EnumToEnumMapping = enumToEnumMapping;
		}


		/// <summary>
		/// <inheritdoc cref="EnumMapperOptions.StringToEnumCaseInsensitive" path="/summary" />
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="EnumMapperOptions"/>.</remarks>
		public bool? StringToEnumCaseInsensitive { get; init; }

		/// <summary>
		/// <inheritdoc cref="EnumMapperOptions.EnumToNumberMapping" path="/summary" />
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="EnumMapperOptions"/>.</remarks>
		public EnumToNumberMapping? EnumToNumberMapping { get; init; }

		/// <summary>
		/// <inheritdoc cref="EnumMapperOptions.EnumToEnumMapping" path="/summary" />
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="EnumMapperOptions"/>.</remarks>
		public EnumToEnumMapping? EnumToEnumMapping { get; init; }
	}
}
