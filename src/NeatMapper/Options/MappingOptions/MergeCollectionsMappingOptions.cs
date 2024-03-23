namespace NeatMapper {
	/// <summary>
	/// Mapping options applied to merge mappings (async and normal), these will override any
	/// configuration options defined in <see cref="MergeCollectionsOptions"/>.
	/// </summary>
	public sealed class MergeCollectionsMappingOptions{
		/// <summary>
		/// Creates a new instance of <see cref="MergeCollectionsMappingOptions"/>.
		/// </summary>
		/// <param name="removeNotMatchedDestinationElements">
		/// <inheritdoc cref="RemoveNotMatchedDestinationElements" path="/summary"/>
		/// <inheritdoc cref="RemoveNotMatchedDestinationElements" path="/remarks"/>
		/// </param>
		/// <param name="matcher">
		/// <inheritdoc cref="Matcher" path="/summary"/>
		/// <inheritdoc cref="Matcher" path="/remarks"/>
		/// </param>
		public MergeCollectionsMappingOptions(bool? removeNotMatchedDestinationElements = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MatchMapDelegate<object, object>?
#else
			MatchMapDelegate<object, object>
#endif
			matcher = null) {

			RemoveNotMatchedDestinationElements = removeNotMatchedDestinationElements;
			Matcher = matcher;
		}


		/// <summary>
		/// If <see langword="true"/>, will remove all the elements from destination which do not have a corresponding element in source,
		/// matched with <see cref="IMatchMap{TSource, TDestination}"/> (or <see cref="Matcher"/>).
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="MergeCollectionsOptions"/>.</remarks>
		public bool? RemoveNotMatchedDestinationElements {
			get;
#if NET5_0_OR_GREATER
			init;
#endif
		}

		/// <summary>
		/// Provides (or overrides) <see cref="IMatchMap{TSource, TDestination}"/> for the outermost collection types.<br/>
		/// You should use the type-safe extensions <see cref="MapperExtensions"/> or <see cref="AsyncMapperExtensions"/>
		/// instead of setting this directly.<br/>
		/// The provided method should throw <see cref="MapNotFoundException"/> if the passed objects are not of expected types.
		/// </summary>
		/// <remarks><see langword="null"/> to use the defined <see cref="IMatchMap{TSource, TDestination}"/> (if any).</remarks>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MatchMapDelegate<object, object>?
#else
			MatchMapDelegate<object, object>
#endif
			Matcher {
				get;
#if NET5_0_OR_GREATER
				init;
#endif
		}
	}
}
