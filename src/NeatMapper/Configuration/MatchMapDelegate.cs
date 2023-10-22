namespace NeatMapper {
	/// <summary>
	/// Delegate which allows matching two objects of two given types, used to override <see cref="IMatchMap{TSource, TDestination}"/>
	/// </summary>
	/// <typeparam name="TSource">Source type</typeparam>
	/// <typeparam name="TDestination">Destination type</typeparam>
	/// <param name="source">source object, may be null</param>
	/// <param name="destination">destination object, may be null</param>
	/// <param name="context">matching context, which allows nested matches, services retrieval via DI, ...</param>
	/// <returns>true if the two objects match</returns>
	public delegate bool MatchMapDelegate<TSource, TDestination>(
#if NET5_0_OR_GREATER
		TSource?
#else
		TSource
#endif
		source,
#if NET5_0_OR_GREATER
		TDestination?
#else
		TDestination
#endif
		destination,
		MatchingContext context);
}
