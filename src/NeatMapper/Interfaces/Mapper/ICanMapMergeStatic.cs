#if NET7_0_OR_GREATER
namespace NeatMapper {
	/// <summary>
	/// Optional interface which allows checking if a given mapper can map the given types.
	/// Mostly useful for generic types because allows checking the inner generic type arguments.
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic.</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic.</typeparam>
	/// <remarks>
	/// This interface is the same as <see cref="ICanMapMerge{TSource, TDestination}"/>, but allows greater flexibility:
	/// for example it can be used in classes which cannot be instantiated (which do not have parameterless constructors).<br/>
	/// Implementations of this interface must be thread-safe.
	/// </remarks>
	public interface ICanMapMergeStatic<in TSource, TDestination> : IMergeMapStatic<TSource, TDestination> {
		/// <summary>
		/// Checks if the implemented <see cref="IMergeMapStatic{TSource, TDestination}"/> could merge
		/// an object into an existing one.
		/// </summary>
		/// <param name="context">
		/// Mapping context, which allows checking nested mappings, services retrieval via DI, additional options, ....
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be merged
		/// into an object of type <typeparamref name="TDestination"/>.
		/// </returns>
		public static abstract bool CanMapMerge(MappingContext context);
	}
}
#endif