namespace NeatMapper {
	/// <summary>
	/// Delegate which allows matching two objects of two given types, used to override
	/// <see cref="IMatchMap{TSource, TDestination}"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <param name="source">Source object, may be null.</param>
	/// <param name="destination">Destination object, may be null.</param>
	/// <param name="context">Matching context, which allows nested matches, services retrieval via DI, ...</param>
	/// <returns><see langword="true"/> if the two objects match.</returns>
	public delegate bool MatchMapDelegate<in TSource, in TDestination>(TSource? source, TDestination? destination, MatchingContext context);
}
