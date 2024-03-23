namespace NeatMapper {
	/// <summary>
	/// Interface which allows matching two objects of two given types.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IMatchMap<TSource, TDestination> {
		/// <summary>
		/// Checks if two objects are equivalent (usually by comparing the keys of the two).
		/// </summary>
		/// <param name="source">Source object, may be null.</param>
		/// <param name="destination">Destination object, may be null.</param>
		/// <param name="context">
		/// Matching context, which allows nested matches, services retrieval via DI, ....
		/// </param>
		/// <returns><see langword="true"/> if the two objects match.</returns>
		bool Match(
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
}
