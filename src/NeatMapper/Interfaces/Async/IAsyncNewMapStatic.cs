#if NET7_0_OR_GREATER
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Map which allows mapping an object to a new one asynchronously, supports open generic types too
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic</typeparam>
	public interface IAsyncNewMapStatic<TSource, TDestination> {
		/// <summary>
		/// Maps an object to a new one asynchronously
		/// </summary>
		/// <param name="source">Object to map, may be null</param>
		/// <param name="context">Mapping context, which allows nested mappings, services retrieval via DI, ...</param>
		/// <returns>A task which when completed returns the newly created object, which may be null</returns>
		public static abstract Task<TDestination?> MapAsync(TSource? source, AsyncMappingContext context);
	}
}
#endif