namespace NeatMapper {
	/// <summary>
	/// Optional interface which allows checking if a given matcher can match the given types or derived types.
	/// Mostly useful for generic types because allows checking the inner generic type arguments.
	/// </summary>
	/// <typeparam name="TSource">Source type, includes derived types.</typeparam>
	/// <typeparam name="TDestination">Destination type, includes derived types.</typeparam>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface ICanMatchHierarchy<in TSource, in TDestination> : IHierarchyMatchMap<TSource, TDestination> {
		/// <summary>
		/// Checks if the implemented <see cref="IHierarchyMatchMap{TSource, TDestination}"/> could match
		/// an object with another one.
		/// </summary>
		/// <param name="context">
		/// Matching context, which allows checking nested matches, services retrieval via DI, additional options, ....
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> (or derived) can be matched
		/// with an object of type <typeparamref name="TDestination"/> (or derived).
		/// </returns>
		bool CanMatch(MatchingContext context);
	}
}
