#if NET7_0_OR_GREATER
namespace NeatMapper.Async {
	/// <summary>
	/// Map which allows mapping an object to a new one asynchronously
	/// </summary>
	/// <typeparam name="TSource">Source type</typeparam>
	/// <typeparam name="TDestination">Destination type</typeparam>
	public interface IAsyncNewMapStatic<TSource, TDestination> {
		/// <summary>
		/// Maps an object to a new one asynchronously
		/// </summary>
		/// <param name="source">object to map, may be null</param>
		/// <param name="context">mapping context, which allows nested mappings, services retrieval via DI, ...</param>
		/// <returns>a task which when completed returns the newly created object, which may be null</returns>
		public static abstract Task<TDestination?> MapAsync(TSource? source, AsyncMappingContext context);
	}
}
#endif