using System;

namespace NeatMapper {
	/// <summary>
	/// Optional interface to be implemented by mappers, which allows to discover if two given types can be mapped or not.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IMapperCanMap : IMapper {
		/// <summary>
		/// Checks if the mapper could create a new object from a given one. It does not guarantee
		/// that the actual map will succeed.
		/// </summary>
		/// <param name="sourceType">Type of the source object, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object to create, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to match the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <paramref name="destinationType"/> can be created
		/// from a parameter of type <paramref name="sourceType"/>.
		/// </returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types.</exception>
		bool CanMapNew(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null);

		/// <summary>
		/// Checks if the mapper could merge an object into an existing one. It does not guarantee
		/// that the actual map will succeed.
		/// </summary>
		/// <param name="sourceType">Type of the object to be mapped, used to retrieve the available maps.</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps.</param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to map the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <paramref name="sourceType"/> can be merged
		/// into an object of type <paramref name="destinationType"/>.
		/// </returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types.</exception>
		bool CanMapMerge(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null);
	}
}
