namespace NeatMapper.Transitive {
	/// <summary>
	/// Options for transitive mappings (async and normal).<br/>
	/// These will override any configuration options defined in <see cref="TransitiveOptions"/>.
	/// </summary>
	public sealed class TransitiveMappingOptions {
		/// <summary>
		/// Creates a new instance of <see cref="TransitiveMappingOptions"/>.
		/// </summary>
		/// <param name="maxChainLength">
		/// <inheritdoc cref="MaxChainLength" path="/summary"/>
		/// <inheritdoc cref="MaxChainLength" path="/remarks"/>
		/// </param>
		public TransitiveMappingOptions(int? maxChainLength = null) {
			MaxChainLength = maxChainLength;
		}


		/// <summary>
		/// Maximum length of the chain of types to map, must be at least 2.
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="TransitiveOptions"/>.</remarks>
		public int? MaxChainLength { get; init; }
	}
}
