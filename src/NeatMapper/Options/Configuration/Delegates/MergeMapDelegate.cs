namespace NeatMapper {
	/// <summary>
	/// Delegate which allows mapping an object to an existing one, used to add custom
	/// <see cref="IMergeMap{TSource, TDestination}"/>
	/// </summary>
	/// <typeparam name="TSource">Source type</typeparam>
	/// <typeparam name="TDestination">Destination type</typeparam>
	/// <param name="source">Object to be mapped, may be null</param>
	/// <param name="destination">Object to map to, may be null</param>
	/// <param name="context">Mapping context, which allows nested mappings, services retrieval via DI, ...</param>
	/// <returns>
	/// The resulting object of the mapping, can be <paramref name="destination"/> or a new one,
	/// may be null
	/// </returns>
	public delegate TDestination? MergeMapDelegate<in TSource, TDestination>(TSource? source, TDestination? destination, MappingContext context);
}
