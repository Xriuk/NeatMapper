using System;

namespace NeatMapper {
	/// <summary>
	/// Interface which allows mapping an object to a new one or an existing one.
	/// </summary>
	/// <remarks>
	/// Note to implementers: if a mapper does not support one of the methods
	/// (<see cref="Map(object?, Type, Type, MappingOptions?)"/> or
	/// <see cref="Map(object?, Type, object?, Type, MappingOptions?)"/>) it should throw
	/// <see cref="MapNotFoundException"/> inside.<br/>
	/// Implementations of this interface must be thread-safe.
	/// </remarks>
	public interface IMapper {
		/// <summary>
		/// Checks if the mapper could create a new object from a given one.
		/// </summary>
		/// <param name="sourceType">
		/// Type of the source object, used to retrieve the available maps. Can be an open generic type.
		/// </param>
		/// <param name="destinationType">
		/// Type of the destination object to create, used to retrieve the available maps. Can be
		/// an open generic type.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to match the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <paramref name="destinationType"/> can be created
		/// from an object of type <paramref name="sourceType"/>.
		/// </returns>
		/// <remarks>
		/// When checking for open generic types the method might return true but some concrete generic types
		/// might not be mappable because of various constraints or missing nested maps.
		/// So you should really use open generic types to check if two types are never mappable.
		/// </remarks>
		bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null);

		/// <summary>
		/// Checks if the mapper could merge an object into an existing one.
		/// </summary>
		/// <param name="sourceType">
		/// Type of the object to be mapped, used to retrieve the available maps. Can be an open generic type.
		/// </param>
		/// <param name="destinationType">
		/// Type of the destination object, used to retrieve the available maps. Can be an open generic type.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to map the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <paramref name="sourceType"/> can be merged
		/// into an object of type <paramref name="destinationType"/>.
		/// </returns>
		/// <remarks>
		/// When checking for open generic types the method might return true but some concrete generic types
		/// might not be mappable because of various constraints or missing nested maps.
		/// So you should really use open generic types to check if two types are never mappable.
		/// </remarks>
		bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null);

		/// <summary>
		/// Maps an object to a new one.
		/// </summary>
		/// <param name="source">Object to map, may be null.</param>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object to create, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore.
		/// </param>
		/// <returns>The newly created object of type <paramref name="destinationType"/>, may be null.</returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		/// <exception cref="MappingException">
		/// An exception was thrown while mapping the types, check the inner exception for details.
		/// </exception>
		object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null);

		/// <summary>
		/// Maps an object to an existing one and returns the result.
		/// </summary>
		/// <param name="source">Object to be mapped, may be null.</param>
		/// <param name="sourceType">Type of the object to be mapped, used to retrieve the available maps.</param>
		/// <param name="destination">Object to map to, may be null.</param>
		/// <param name="destinationType">
		/// Type of the destination object, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore.
		/// </param>
		/// <returns>
		/// The resulting object of the mapping of type <paramref name="destinationType"/>, can be the same as
		/// <paramref name="destination"/> or a new one, may be null.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		/// <exception cref="MappingException">
		/// An exception was thrown while mapping the types, check the inner exception for details.
		/// </exception>
		object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null);
	}
}
