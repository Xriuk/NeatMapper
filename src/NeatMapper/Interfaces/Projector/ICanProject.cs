namespace NeatMapper {
	/// <summary>
	/// Optional interface which allows checking if a given projector can project the given types.
	/// Mostly useful for generic types because allows checking the inner generic type arguments.
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic.</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic.</typeparam>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface ICanProject<TSource, TDestination> : IProjectionMap<TSource, TDestination> {
		/// <summary>
		/// Checks if the implemented <see cref="IProjectionMap{TSource, TDestination}"/> could project
		/// a given object to another.
		/// </summary>
		/// <param name="context">
		/// Projection context, which allows checking nested projections, services retrieval via DI, additional options, ....
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be projected
		/// to an object of type <typeparamref name="TDestination"/>.
		/// </returns>
		bool CanProject(ProjectionContext context);
	}
}
