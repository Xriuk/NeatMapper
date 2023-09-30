namespace NeatMapper {
	public sealed class MappingContext : MatchingContext {
		internal MappingContext() { }


		/// <summary>
		/// Mapper which can be used for nested mappings
		/// </summary>
		public IMapper Mapper { get; internal set; } = null!;
	}
}
