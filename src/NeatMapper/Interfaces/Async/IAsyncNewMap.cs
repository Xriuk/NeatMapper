using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Map which allows mapping an object to a new one asynchronously, supports open generic types too
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic</typeparam>
	public interface IAsyncNewMap<TSource, TDestination> {
		/// <summary>
		/// Maps an object to a new one asynchronously
		/// </summary>
		/// <param name="source">Object to map, may be null</param>
		/// <param name="context">Mapping context, which allows nested mappings, services retrieval via DI, ...</param>
		/// <returns>A task which when completed returns the newly created object, which may be null</returns>
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
			source,
			AsyncMappingContext context);
	}
}
