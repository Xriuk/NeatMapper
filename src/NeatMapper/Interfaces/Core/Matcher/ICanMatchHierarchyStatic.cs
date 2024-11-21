#if NET7_0_OR_GREATER
namespace NeatMapper {
	/// <summary>
	/// Optional interface which allows checking if a given matcher can match the given types or derived types.
	/// Mostly useful for generic types because allows checking the inner generic type arguments.
	/// </summary>
	/// <typeparam name="TSource">Source type, includes derived types.</typeparam>
	/// <typeparam name="TDestination">Destination type, includes derived types.</typeparam>
	/// <remarks>
	/// This interface is the same as <see cref="ICanMatchHierarchy{TSource, TDestination}"/>, but allows greater flexibility:
	/// for example it can be used in classes which cannot be instantiated (which do not have parameterless constructors).<br/>
	/// Implementations of this interface must be thread-safe.
	/// </remarks>
	public interface ICanMatchHierarchyStatic<in TSource, in TDestination> : IHierarchyMatchMapStatic<TSource, TDestination> {
		/// <summary>
		/// Checks if the implemented <see cref="IHierarchyMatchMapStatic{TSource, TDestination}"/> could match
		/// an object with another one.
		/// </summary>
		/// <param name="context">
		/// Matching context, which allows checking nested matches, services retrieval via DI, additional options, ....
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> (or derived) can be matched
		/// with an object of type <typeparamref name="TDestination"/> (or derived).
		/// </returns>
		public static abstract bool CanMatch(MatchingContext context);
	}
}
#endif