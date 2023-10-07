using System.Threading.Tasks;

namespace NeatMapper.Async {
	/// <summary>
	/// Delegate which allows mapping an object to an existing onee asynchronously,
	/// used to add custom <see cref="IAsyncMergeMap{TSource, TDestination}"/>
	/// </summary>
	/// <typeparam name="TSource">Source type</typeparam>
	/// <typeparam name="TDestination">Destination type</typeparam>
	/// <param name="source">object to be mapped, may be null</param>
	/// <param name="destination">object to map to, may be null</param>
	/// <param name="context">mapping context, which allows nested mappings, services retrieval via DI, ...</param>
	/// <returns>
	/// a task which when completed returns the resulting object of the mapping,
	/// can be <paramref name="destination"/> or a new one, may be null
	/// </returns>
	public delegate
		Task<
#if NET5_0_OR_GREATER
		TDestination?
#else
		TDestination
#endif
		> AsyncMergeMapDelegate<TSource, TDestination>(
#if NET5_0_OR_GREATER
		TSource?
#else
		TSource
#endif
		source,
#if NET5_0_OR_GREATER
		TDestination?
#else
		TDestination
#endif
		destination,
		AsyncMappingContext context);
}
