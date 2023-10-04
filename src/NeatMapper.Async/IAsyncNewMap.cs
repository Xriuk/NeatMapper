using System.Threading.Tasks;

namespace NeatMapper.Async {
	/// <summary>
	/// Map which allows mapping an object to a new one asynchronously
	/// </summary>
	/// <typeparam name="TSource">Source type</typeparam>
	/// <typeparam name="TDestination">Destination type</typeparam>
	public interface IAsyncNewMap<TSource, TDestination> {
		/// <summary>
		/// Maps an object to a new one asynchronously
		/// </summary>
		/// <param name="source">object to map, may be null</param>
		/// <param name="context">mapping context, which allows nested mappings, services retrieval via DI, ...</param>
		/// <returns>a task which when completed returns the newly created object, which may be null</returns>
		Task<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> MapAsync(
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source, AsyncMappingContext context);
	}
}
