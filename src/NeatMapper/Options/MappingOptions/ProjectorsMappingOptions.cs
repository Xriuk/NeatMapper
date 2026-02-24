namespace NeatMapper {
	/// <summary>
	/// Options applied to automatic collections merge mappings (async and normal).<br/>
	/// These will override any configuration options defined in <see cref="ProjectorsOptions"/>.
	/// </summary>
	public sealed class ProjectorsMappingOptions {
		/// <summary>
		/// Creates a new instance of <see cref="ProjectorsMappingOptions"/>.
		/// </summary>
		/// <param name="nullChecks">
		/// <inheritdoc cref="NullChecks" path="/summary"/>
		/// <inheritdoc cref="NullChecks" path="/remarks"/>
		/// </param>
		public ProjectorsMappingOptions(bool? nullChecks = null) {
			NullChecks = nullChecks;
		}


		/// <summary>
		/// <inheritdoc cref="ProjectorsOptions.NullChecks" path="/summary" />
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="ProjectorsOptions"/>.</remarks>
		public bool? NullChecks { get; init; }
	}
}
