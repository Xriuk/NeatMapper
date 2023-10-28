﻿namespace NeatMapper {
	/// <summary>
	/// Mapping options applied to merge mappings, these will override any configuration options defined in <see cref="MergeCollectionsOptions"/>
	/// </summary>
	public sealed class MergeCollectionsMappingOptions{
		public MergeCollectionsMappingOptions() { }
		public MergeCollectionsMappingOptions(MergeCollectionsMappingOptions other) {
			RemoveNotMatchedDestinationElements = other.RemoveNotMatchedDestinationElements;
			Matcher = other.Matcher;
		}


		/// <summary>
		/// If <see langword="true"/>, will remove all the elements from destination which do not have a corresponding element in source,
		/// matched with <see cref="IMatchMap{TSource, TDestination}"/> (or <see cref="Matcher"/>)
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting</remarks>
		public bool? RemoveNotMatchedDestinationElements { get; set; }

		/// <summary>
		/// Provides (or overrides) <see cref="IMatchMap{TSource, TDestination}"/> for the outermost collection types
		/// </summary>
		/// <remarks><see langword="null"/> to use the defined <see cref="IMatchMap{TSource, TDestination}"/> (if any)</remarks>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MatchMapDelegate<object, object>?
#else
			MatchMapDelegate<object, object>
#endif
			Matcher { get; set; }
	}
}
