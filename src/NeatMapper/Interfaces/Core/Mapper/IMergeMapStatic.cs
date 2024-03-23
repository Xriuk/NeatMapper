#if NET7_0_OR_GREATER
namespace NeatMapper {
	/// <summary>
	/// Map which allows mapping an object to an existing one, supports open generic types too.
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic.</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic.</typeparam>
	/// <remarks>
	/// This interface is the same as <see cref="IMergeMap{TSource, TDestination}"/>, but allows greater flexibility:
	/// for example it can be used in classes which cannot be instantiated (which do not have parameterless constructors).<br/>
	/// Implementations of this interface must be thread-safe.
	/// </remarks>
	public interface IMergeMapStatic<TSource, TDestination> {
		/// <summary>
		/// Maps an object to an existing one and returns the result.
		/// </summary>
		/// <param name="source">Object to be mapped, may be null.</param>
		/// <param name="destination">Object to map to, may be null.</param>
		/// <param name="context">
		/// Mapping context, which allows nested mappings, services retrieval via DI, additional options, ....
		/// </param>
		/// <returns>
		/// The resulting object of the mapping, can be <paramref name="destination"/> or a new one,
		/// may be null.
		/// </returns>
		public static abstract TDestination? Map(TSource? source, TDestination? destination, MappingContext context);
	}
}
#endif