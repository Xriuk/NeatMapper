namespace NeatMapper {
	/// <summary>
	/// Interface which allows mapping an object to a new one or an existing one
	/// </summary>
	public interface IMapper {
		/// <summary>
		/// Maps an object to a new one.<br/>
		/// Can also map to collections automatically, will create the destination collection and map each element individually
		/// </summary>
		/// <param name="source">object to map, may be null</param>
		/// <param name="sourceType">type of the object to map, used to retrieve the available maps</param>
		/// <param name="destinationType">type of the destination object to create, used to retrieve the available maps</param>
		/// <returns>the newly created object of <paramref name="destinationType"/>, may be null</returns>
		public object? Map(object? source, Type sourceType, Type destinationType);

		/// <summary>
		/// Maps an object to an existing one and returns the result.<br/>
		/// Can also map to collections automatically, will try to match elements with <see cref="IMatchMapStatic{TSource, TDestination}"/>
		/// (or the passed <paramref name="collectionElementComparer"/>), will create the destination collection if it is null and map each element individually
		/// </summary>
		/// <param name="source">object to be mapped, may be null</param>
		/// <param name="sourceType">type of the object to be mapped, used to retrieve the available maps</param>
		/// <param name="destination">object to map to, may be null</param>
		/// <param name="destinationType">type of the destination object, used to retrieve the available maps</param>
		/// <param name="mappingOptions">additional options for the current map</param>
		/// <returns>
		/// the resulting object of the mapping of <paramref name="destinationType"/> type, can be the same as <paramref name="destination"/> or a new one,
		/// may be null
		/// </returns>
		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null);
	}
}
