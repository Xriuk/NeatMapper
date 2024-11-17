namespace NeatMapper {
	/// <summary>
	/// Delegate which allows matching two objects of two given types or derived types, used to override
	/// <see cref="IHierarchyMatchMap{TSource, TDestination}"/>
	/// </summary>
	/// <typeparam name="TSource">Source type, includes derived types</typeparam>
	/// <typeparam name="TDestination">Destination type, includes derived types</typeparam>
	/// <param name="source">Source object, may be null</param>
	/// <param name="destination">Destination object, may be null</param>
	/// <param name="context">Matching context, which allows nested matches, services retrieval via DI, ...</param>
	/// <returns><see langword="true"/> if the two objects match</returns>
	public delegate bool HierarchyMatchMapDelegate<in TSource, in TDestination>(TSource? source, TDestination? destination, MatchingContext context);
}
