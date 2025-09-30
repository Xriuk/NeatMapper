namespace NeatMapper {
	/// <summary>
	/// Options applied to <see cref="CopyMapper"/>.<br/>
	/// These will override any options defined in <see cref="CopyMapperOptions"/>.
	/// </summary>
	public sealed class CopyMapperMappingOptions {
		/// <summary>
		/// Creates a new instance of <see cref="CopyMapperMappingOptions"/>.
		/// </summary>
		/// <param name="propertiesToMap">
		/// <inheritdoc cref="PropertiesToMap" path="/summary"/>
		/// <inheritdoc cref="PropertiesToMap" path="/remarks"/>
		/// </param>
		/// <param name="fieldsToMap">
		/// <inheritdoc cref="FieldsToMap" path="/summary"/>
		/// <inheritdoc cref="FieldsToMap" path="/remarks"/>
		/// </param>
		/// <param name="deepCopy">
		/// <inheritdoc cref="DeepCopy" path="/summary"/>
		/// <inheritdoc cref="DeepCopy" path="/remarks"/>
		/// </param>
		public CopyMapperMappingOptions(
			MemberVisibilityFlags? propertiesToMap = null,
			MemberVisibilityFlags? fieldsToMap = null,
			DeepCopyFlags? deepCopy = null) {

			PropertiesToMap = propertiesToMap;
			FieldsToMap = fieldsToMap;
			DeepCopy = deepCopy;
		}


		/// <summary>
		/// <inheritdoc cref="CopyMapperOptions.PropertiesToMap" path="/summary" />
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="CopyMapperOptions"/>.</remarks>
		public MemberVisibilityFlags? PropertiesToMap { get; init; }

		/// <summary>
		/// <inheritdoc cref="CopyMapperOptions.FieldsToMap" path="/summary" />
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="CopyMapperOptions"/>.</remarks>
		public MemberVisibilityFlags? FieldsToMap { get; init; }

		/// <summary>
		/// <inheritdoc cref="CopyMapperOptions.DeepCopy" path="/summary" />
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="CopyMapperOptions"/>.</remarks>
		public DeepCopyFlags? DeepCopy { get; init; }
	}
}
