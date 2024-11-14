using System;

namespace NeatMapper {
	/// <summary>
	/// Optional interface to be implemented by mappers, which allows to create a factory to map objects
	/// of given types, instead of mapping them directly. The created factories should share the same
	/// <see cref="MappingContext"/>.<br/>
	/// The implementation should be more efficient than calling IMapper.Map() multiple times,
	/// and thus can be used for collections.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IMapperFactory : IMapper {
		/// <summary>
		/// Creates a factory to map an object to a new one.
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
		/// of type <paramref name="destinationType"/>.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null);

		/// <summary>
		/// Creates a factory to map an object to an existing one.
		/// </summary>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore.
		/// </param>
		/// <returns>
		/// A factory which can be used to map objects of type <paramref name="sourceType"/> into existing objects
		/// of type <paramref name="destinationType"/>.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null);
	}
}
