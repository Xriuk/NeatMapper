using NeatMapper.Core.Configuration;

namespace NeatMapper.Core {
	/// <summary>
	/// Mapping options to use for the mapping operations, this will override any configuration options defined in <see cref="MapperConfigurationOptions"/>
	/// </summary>
	public class MappingOptions {
		/// <summary>
		/// If true, will remove all the elements from destination which do not have a corresponding element in source,
		/// matched with <see cref="ICollectionElementComparer{TSource, TDestination}"/> (or <see cref="CollectionElementComparer"/>)
		/// </summary>
		/// <remarks>null to ignore and use global setting</remarks>
		public bool? CollectionRemoveNotMatchedDestinationElements { get; set; }

		/// <summary>
		/// Provides (or overrides) <see cref="ICollectionElementComparer{TSource, TDestination}"/> for the outermost collection types
		/// </summary>
		/// <remarks>null to ignore and use the defined <see cref="ICollectionElementComparer{TSource, TDestination}"/> (if any)</remarks>
		public Func<object?, object?, MappingContext, bool>? CollectionElementComparer { get; set; }
	}
}
