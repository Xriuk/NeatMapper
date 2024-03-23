namespace NeatMapper {
	/// <summary>
	/// Map which allows mapping an object to a new one, supports open generic types too.
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic.</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic.</typeparam>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface INewMap<TSource, TDestination> {
		/// <summary>
		/// Maps an object to a new one.
		/// </summary>
		/// <param name="source">Object to map, may be null.</param>
		/// <param name="context">
		/// Mapping context, which allows nested mappings, services retrieval via DI, additional options, ....
		/// </param>
		/// <returns>The newly created object, may be null.</returns>
#if NET5_0_OR_GREATER
		TDestination?
#else
		TDestination
#endif
			Map(
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
			MappingContext context);
	}
}
