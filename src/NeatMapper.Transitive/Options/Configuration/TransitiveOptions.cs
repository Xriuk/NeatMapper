namespace NeatMapper.Transitive {
	/// <summary>
	/// Options for transitive mappings (new and merge, async and normal) and projections.<br/>
	/// Can be overridden during mapping with <see cref="TransitiveMappingOptions"/>.
	/// </summary>
	public sealed class TransitiveOptions {
		/// <summary>
		/// Creates a new instance of <see cref="TransitiveOptions"/>.
		/// </summary>
		public TransitiveOptions() { }
		/// <summary>
		/// Creates a new instance of <see cref="TransitiveOptions"/> by copying options from another instance.
		/// </summary>
		public TransitiveOptions(TransitiveOptions options) {
			MaxChainLength = options.MaxChainLength;
		}


		/// <summary>
		/// Maximum length of the chain of types to map, must be at least 2.
		/// </summary>
		/// <remarks>Defaults to <see cref="int.MaxValue"/>, which is "almost" no limit.</remarks>
		public int MaxChainLength { get; set; } = int.MaxValue;
	}
}
