using System;

namespace NeatMapper {
	/// <summary>
	/// Optional interface to be implemented by asynchronous mappers, which allows to create a factory to map objects
	/// of given types, instead of mapping them directly. The created factories should share the same
	/// <see cref="AsyncMappingContextOptions"/>.<br/>
	/// The implementation should be more efficient than calling IAsyncMapper.MapAsync() multiple times,
	/// and thus can be used for collections.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe, this includes the returned factories too.</remarks>
	public interface IAsyncMapperFactory : IAsyncMapper {
		/// <summary>
		/// Creates a factory to map an object to a new one asynchronously.
		/// </summary>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object to create, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore.
		/// </param>
		/// <returns>
		/// A factory which can be used to map objects of type <paramref name="sourceType"/> into new objects
		/// of type <paramref name="destinationType"/> asynchronously.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		IAsyncNewMapFactory MapAsyncNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null);

		/// <summary>
		/// Creates a factory to map an object to an existing one asynchronously.
		/// </summary>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object to create, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore.
		/// </param>
		/// <returns>
		/// A factory which can be used to map objects of type <paramref name="sourceType"/> into existing objects
		/// of type <paramref name="destinationType"/> asynchronously.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		IAsyncMergeMapFactory MapAsyncMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null);
	}
}
