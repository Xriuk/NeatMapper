namespace NeatMapper.Core {
	public interface IMapper {
		/// <summary>
		/// Maps an object to a new one
		/// </summary>
		/// <param name="source">object to map, may be null</param>
		/// <param name="sourceType">type of the object to map, used to retrieve the available maps</param>
		/// <param name="destinationType">type of the destination object to create, used to retrieve the available maps</param>
		/// <returns>the newly created object of <paramref name="destinationType"/>, may be null</returns>
		public object? Map(object? source, Type sourceType, Type destinationType);

		/// <summary>
		/// Maps an object to an existing one and returns the result
		/// </summary>
		/// <param name="source">object to be mapped, may be null</param>
		/// <param name="sourceType">type of the object to be mapped, used to retrieve the available maps</param>
		/// <param name="destination">object to map to, may be null</param>
		/// <param name="destinationType">type of the destination object, used to retrieve the available maps</param>
		/// <param name="collectionElementComparer">
		/// if <paramref name="sourceType"/> and <paramref name="destinationType"/> are both collections
		/// you can optionally pass a method to provide (or override) the <see cref="ICollectionElementComparer{,}"/> used for mapping
		/// </param>
		/// <returns>
		/// the resulting object of the mapping of <paramref name="destinationType"/>, can be <paramref name="destination"/> or a new one,
		/// may be null
		/// </returns>
		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, Func<object, object, MappingContext, bool>? collectionElementComparer = null);
	}
}
