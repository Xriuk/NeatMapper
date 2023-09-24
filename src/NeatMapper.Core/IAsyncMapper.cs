namespace NeatMapper.Core {
	public interface IAsyncMapper {
		public Task<object?> MapAsync(object? source, Type sourceType, Type destinationType, CancellationToken cancellationToken = default);

		public Task<object?> MapAsync(object? source, Type sourceType, object? destination, Type destinationType, CancellationToken cancellationToken = default);

		/// <summary>
		/// Checks if two objects are the same by invoking the corresponding <see cref="ICollectionElementComparer{TSource, TDestination}.Match"/>
		/// </summary>
		/// <param name="source">source object, may be null</param>
		/// <param name="sourceType">type of the source object, used to retrieve the available comparers</param>
		/// <param name="destination">destination object, may be null</param>
		/// <param name="destinationType">type of the destination object, used to retrieve the available comparers</param>
		/// <returns>true if the two objects are the same</returns>
		public bool Match(object? source, Type sourceType, object? destination, Type destinationType);
	}
}
