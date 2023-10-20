using NeatMapper.Common.Mapper;

namespace NeatMapper {
	/// <summary>
	/// Contains informations and services for the current mapping operation
	/// </summary>
	public sealed class MappingContext : BaseContext {
		internal MappingContext() { }


		/// <summary>
		/// Mapper which can be used for nested mappings
		/// </summary>
		public IMapper Mapper { get; internal set; }
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			= null!;
#endif
		
		/// <summary>
		/// Additional mapping options, contains multiple options of different types,
		/// each mapper/map should try to retrieve its options and use them
		/// </summary>
		public MappingOptions MappingOptions { get; internal set; }
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			= null!;
#endif
	}
}
