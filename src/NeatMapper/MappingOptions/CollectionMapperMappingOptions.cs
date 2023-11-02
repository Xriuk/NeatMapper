namespace NeatMapper {
	/// <summary>
	/// Provided by <see cref="CollectionMapper"/>, gives info about the parent mapper
	/// </summary>
	public sealed class CollectionMapperMappingOptions {
		/// <summary>
		/// Parent mapper (derived from <see cref="CollectionMapper"/>) of the current mapper
		/// </summary>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IMapper?
#else
			IMapper
#endif
			Mapper { get; set; }
	}
}
