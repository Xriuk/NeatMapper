namespace NeatMapper {
	/// <summary>
	/// Optional interface which allows checking if a given mapper can map the given types.
	/// Mostly useful for generic types because allows checking the inner generic type arguments.
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic.</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic.</typeparam>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface ICanMapNew<in TSource, out TDestination> : INewMap<TSource, TDestination> {
		/// <summary>
		/// Checks if the implemented <see cref="INewMap{TSource, TDestination}"/> could create
		/// a new object from a given one.
		/// </summary>
		/// <param name="context">
		/// Mapping context, which allows checking nested mappings, services retrieval via DI, additional options, ....
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be created
		/// from an object of type <typeparamref name="TSource"/>.
		/// </returns>
		bool CanMapNew(MappingContext context);
	}
}
