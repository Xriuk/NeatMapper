namespace NeatMapper {
	/// <summary>
	/// Delegate which allows checking if an object could be mapped to a new one asynchronously, used to add custom
	/// <see cref="ICanMapAsyncNew{TSource, TDestination}"/>, in conjunction with
	/// <see cref="AsyncNewMapDelegate{TSource, TDestination}"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <param name="context">Mapping context, which allows checking nested mappings, services retrieval via DI, ....</param>
	/// <returns>
	/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be created
	/// from an object of type <typeparamref name="TSource"/> asynchronously.
	/// </returns>
	public delegate bool CanMapAsyncNewDelegate<in TSource, TDestination>(AsyncMappingContextOptions context);
}
