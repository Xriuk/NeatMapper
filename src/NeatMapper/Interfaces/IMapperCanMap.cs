using System;

namespace NeatMapper {
	/// <summary>
	/// Optional interface to be implemented by mappers, which allows to discover if two given types can be mapped or not
	/// </summary>
	public interface IMapperCanMap : IMapper {
		/// <summary>
		/// Checks if the mapper can create a new object from a given one
		/// </summary>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object to create, used to retrieve the available maps</param>
		/// <returns>True if an object of <paramref name="destinationType"/> can be created from a parameter of <paramref name="sourceType"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		bool CanMapNew(Type sourceType, Type destinationType);

		/// <summary>
		/// Checks if the mapper can merge an object into an existing one
		/// </summary>
		/// <param name="sourceType">Type of the object to be mapped, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <returns>True if an object of <paramref name="sourceType"/> can be merged into an object of <paramref name="destinationType"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		bool CanMapMerge(Type sourceType, Type destinationType);
	}
}
