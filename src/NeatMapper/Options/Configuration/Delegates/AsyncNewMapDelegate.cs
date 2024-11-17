using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Delegate which allows mapping an object to a new one asynchronously, used to add custom
	/// <see cref="IAsyncNewMap{TSource, TDestination}"/>
	/// </summary>
	/// <typeparam name="TSource">Source type</typeparam>
	/// <typeparam name="TDestination">Destination type</typeparam>
	/// <param name="source">Object to map, may be null</param>
	/// <param name="context">Mapping context, which allows nested mappings, services retrieval via DI, ...</param>
	/// <returns>The newly created object, may be null</returns>
	public delegate Task<TDestination?> AsyncNewMapDelegate<in TSource, TDestination>(TSource? source, AsyncMappingContext context);
}
