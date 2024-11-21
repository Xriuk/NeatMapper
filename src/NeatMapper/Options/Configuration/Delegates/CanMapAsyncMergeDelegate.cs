namespace NeatMapper {
	/// <summary>
	/// Delegate which allows checking if an object could be merged into a new one asynchronously, used to add custom
	/// <see cref="ICanMapAsyncMerge{TSource, TDestination}"/>, in conjunction with
	/// <see cref="AsyncMergeMapDelegate{TSource, TDestination}"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <param name="context">Mapping context, which allows checking nested mappings, services retrieval via DI, ....</param>
	/// <returns>
	/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be merged
	/// into an object of type <typeparamref name="TDestination"/> asynchronously.
	/// </returns>
	public delegate bool CanMapAsyncMergeDelegate<in TSource, TDestination>(AsyncMappingContextOptions context);
}
