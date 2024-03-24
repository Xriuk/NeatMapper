#if NET7_0_OR_GREATER
namespace NeatMapper {
	/// <summary>
	/// Map which allows mapping an object to a new one, supports open generic types too.
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic.</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic.</typeparam>
	/// <remarks>
	/// This interface is the same as <see cref="INewMap{TSource, TDestination}"/>, but allows greater flexibility:
	/// for example it can be used in classes which cannot be instantiated (which do not have parameterless constructors).<br/>
	/// Implementations of this interface must be thread-safe.
	/// </remarks>
	public interface INewMapStatic<TSource, TDestination> {
		/// <summary>
		/// Maps an object to a new one.
		/// </summary>
		/// <param name="source">Object to map, may be null.</param>
		/// <param name="context">
		/// Mapping context, which allows nested mappings, services retrieval via DI, additional options, ....
		/// </param>
		/// <returns>The newly created object, may be null.</returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		/// <exception cref="MappingException">
		/// An exception was thrown while mapping the types, check the inner exception for details.
		/// </exception>
		public static abstract TDestination? Map(TSource? source, MappingContext context);
	}
}
#endif