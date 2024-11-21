namespace NeatMapper {
	/// <summary>
	/// Optional interface which allows checking if a given matcher can match the given types.
	/// Mostly useful for generic types because allows checking the inner generic type arguments.
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic.</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic.</typeparam>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface ICanMatch<in TSource, in TDestination> : IMatchMap<TSource, TDestination> {
		/// <summary>
		/// Checks if the implemented <see cref="IMatchMap{TSource, TDestination}"/> could match
		/// an object with another one.
		/// </summary>
		/// <param name="context">
		/// Matching context, which allows checking nested matches, services retrieval via DI, additional options, ....
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be matched
		/// with an object of type <typeparamref name="TDestination"/>.
		/// </returns>
		bool CanMatch(MatchingContext context);
	}
}
