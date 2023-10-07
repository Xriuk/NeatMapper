using NeatMapper.Common;

namespace NeatMapper {
	/// <summary>
	/// Mapping options to use for the mapping operations, this will override any configuration options defined in <see cref="Configuration.MapperConfigurationOptions"/>
	/// </summary>
	public class MappingOptions {
		/// <summary>
		/// If true, will remove all the elements from destination which do not have a corresponding element in source,
		/// matched with <see cref="IMatchMap{TSource, TDestination}"/> (or <see cref="Matcher"/>)
		/// </summary>
		/// <remarks>null to ignore and use global setting</remarks>
		public bool? CollectionRemoveNotMatchedDestinationElements { get; set; }

		/// <summary>
		/// Provides (or overrides) <see cref="IMatchMap{TSource, TDestination}"/> for the outermost collection types
		/// </summary>
		/// <remarks>null to ignore and use the defined <see cref="IMatchMap{TSource, TDestination}"/> (if any)</remarks>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MatchMapDelegate<object, object>?
#else
			MatchMapDelegate<object, object>
#endif
			Matcher { get; set; }
	}
}
