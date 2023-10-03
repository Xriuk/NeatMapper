namespace NeatMapper {
	/// <summary>
	/// Contains informations and services for the current mapping operation
	/// </summary>
	public sealed class MappingContext : MatchingContext {
		internal MappingContext() { }


		/// <summary>
		/// Mapper which can be used for nested mappings
		/// </summary>
		public IMapper Mapper { get; internal set; }
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			= null!;
#endif
	}
}
