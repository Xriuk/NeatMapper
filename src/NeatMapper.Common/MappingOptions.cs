namespace NeatMapper {
	/// <summary>
	/// Mapping options to use for the mapping operations, this will override any configuration options defined in <see cref="Configuration.MapperConfigurationOptions"/>
	/// </summary>
	public class MappingOptions {
		/// <summary>
		/// If true, will remove all the elements from destination which do not have a corresponding element in source,
		/// matched with <see cref="IMatchMapStatic{TSource, TDestination}"/> (or <see cref="Matcher"/>)
		/// </summary>
		/// <remarks>null to ignore and use global setting</remarks>
		public bool? CollectionRemoveNotMatchedDestinationElements { get; set; }

		/// <summary>
		/// Provides (or overrides) <see cref="IMatchMapStatic{TSource, TDestination}"/> for the outermost collection types
		/// </summary>
		/// <remarks>null to ignore and use the defined <see cref="IMatchMapStatic{TSource, TDestination}"/> (if any)</remarks>
		public Func<object?, object?, MatchingContext, bool>? Matcher { get; set; }
	}
}
