using System;

namespace NeatMapper {
	/// <summary>
	/// Optional interface to be implemented by projectors, which allows to discover if two given types
	/// can be projected or not.
	/// </summary>
	public interface IProjectorCanProject : IProjector {
		/// <summary>
		/// Checks if the projector could project a given object to another. It does not guarantee that the actual map will succeed.
		/// </summary>
		/// <param name="sourceType">Type of the object to project, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object to project to, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to project the types, this helps obtaining more accurate results,
		/// since some projectors may depend on specific options to project or not two given types.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <paramref name="sourceType"/> can be projected
		/// to an object of type <paramref name="destinationType"/>.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// Could not verify if the projector supports the given types.
		/// </exception>
		bool CanProject(
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
