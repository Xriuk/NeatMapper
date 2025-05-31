#if NET7_0_OR_GREATER
namespace NeatMapper {
	/// <summary>
	/// Optional interface which allows checking if a given mapper can map the given types asynchronously.
	/// Mostly useful for generic types because allows checking the inner generic type arguments.
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic.</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic.</typeparam>
	/// <remarks>
	/// This interface is the same as <see cref="ICanMapAsyncNew{TSource, TDestination}"/>, but allows greater flexibility:
	/// for example it can be used in classes which cannot be instantiated (which do not have parameterless constructors).<br/>
	/// Implementations of this interface must be thread-safe.
	/// </remarks>
	public interface ICanMapAsyncNewStatic<in TSource, TDestination> : IAsyncNewMapStatic<TSource, TDestination> {
		/// <summary>
		/// Checks if the implemented <see cref="IAsyncNewMapStatic{TSource, TDestination}"/> could create
		/// a new object from a given one asynchronously.
		/// </summary>
		/// <param name="context">
		/// Mapping context, which allows checking nested mappings, services retrieval via DI, additional options, ....
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be created
		/// from an object of type <typeparamref name="TSource"/> asynchronously.
		/// </returns>
		public static abstract bool CanMapAsyncNew(AsyncMappingContextOptions context);
	}
}
#endif
