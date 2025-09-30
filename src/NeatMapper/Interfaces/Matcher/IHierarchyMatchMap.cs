namespace NeatMapper {
	/// <summary>
	/// Interface which allows matching two objects of two given types or derived types.
	/// </summary>
	/// <typeparam name="TSource">Source type, includes derived types.</typeparam>
	/// <typeparam name="TDestination">Destination type, includes derived types.</typeparam>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IHierarchyMatchMap<in TSource, in TDestination> {
		/// <summary>
		/// Checks if two objects are equivalent (usually by comparing the keys of the two).
		/// </summary>
		/// <param name="source">Source object, may be null.</param>
		/// <param name="destination">Destination object, may be null.</param>
		/// <param name="context">
		/// Matching context, which allows nested matches, services retrieval via DI, ....
		/// </param>
		/// <returns><see langword="true"/> if the two objects match.</returns>
		bool Match(TSource? source, TDestination? destination, MatchingContext context);
	}
}
