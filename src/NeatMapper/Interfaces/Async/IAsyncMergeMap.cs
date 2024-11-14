using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Map which allows mapping an object to an existing one asynchronously, supports open generic types too
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic</typeparam>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IAsyncMergeMap<in TSource, TDestination> {
		/// <summary>
		/// Maps an object to an existing one asynchronously and returns the result
		/// </summary>
		/// <param name="source">Object to be mapped, may be null</param>
		/// <param name="destination">Object to map to, may be null</param>
		/// <param name="context">Mapping context, which allows nested mappings, services retrieval via DI, ...</param>
		/// <returns>
		/// A task which when completed returns the resulting object of the mapping,
		/// can be <paramref name="destination"/> or a new one, may be null
		/// </returns>
		Task<TDestination?> MapAsync(TSource? source, TDestination? destination, AsyncMappingContext context);
	}
}
