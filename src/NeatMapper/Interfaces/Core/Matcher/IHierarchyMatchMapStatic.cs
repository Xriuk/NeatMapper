#if NET7_0_OR_GREATER
namespace NeatMapper {
	/// <summary>
	/// Interface which allows matching two objects of two given types or derived types.
	/// </summary>
	/// <typeparam name="TSource">Source type, includes derived types.</typeparam>
	/// <typeparam name="TDestination">Destination type, includes derived types.</typeparam>
	/// <remarks>
	/// This interface is the same as <see cref="IHierarchyMatchMap{TSource, TDestination}"/>, but allows greater flexibility:
	/// for example it can be used in classes which cannot be instantiated (which do not have parameterless constructors).<br/>
	/// Implementations of this interface must be thread-safe.
	/// </remarks>
	public interface IHierarchyMatchMapStatic<in TSource, in TDestination> {
		/// <summary>
		/// Checks if two objects are equivalent (usually by comparing the keys of the two).
		/// </summary>
		/// <param name="source">Source object, may be null.</param>
		/// <param name="destination">Destination object, may be null.</param>
		/// <param name="context">
		/// Matching context, which allows nested matches, services retrieval via DI, ....
		/// </param>
		/// <returns><see langword="true"/> if the two objects match.</returns>
		/// <exception cref="MapNotFoundException">The provided types could not be matched.</exception>
		/// <exception cref="MatcherException">
		/// An exception was thrown while matching the types, check the inner exception for details.
		/// </exception>
		public static abstract bool Match(TSource? source, TDestination? destination, MatchingContext context);
	}
}
#endif