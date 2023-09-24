namespace NeatMapper.Core {
	/// <summary>
	/// Interface which allows matching two elements inside a collection in an automatic map in <see cref="IMergeMap{TSource, TDestination}"/>
	/// and <see cref="IAsyncMergeMap{TSource, TDestination}"/>
	/// </summary>
	/// <typeparam name="TSource">Source type</typeparam>
	/// <typeparam name="TDestination">Destination type</typeparam>
	public interface ICollectionElementComparer<TSource, TDestination> {
		/// <summary>
		/// Checks if two objects are the same (usually by comparing the keys of the two)
		/// </summary>
		/// <param name="source">source collection element, may be null</param>
		/// <param name="destination">destination collection element, may be null</param>
		/// <param name="context">mapping context, which allows nested mappings or matches, services retrieval via DI, ...</param>
		/// <returns>true if the two objects are the same, thus should be merged together</returns>
		public static abstract bool Match(TSource? source, TDestination? destination, MappingContext context);
	}
}
