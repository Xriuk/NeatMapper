using System;

namespace NeatMapper {
	/// <summary>
	/// Interface which allows matching two objects.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IMatcher {
		/// <summary>
		/// Checks if two objects are equivalent.
		/// </summary>
		/// <param name="source">Object to compare, may be null.</param>
		/// <param name="sourceType">Type of the source object, used to retrieve the available maps.</param>
		/// <param name="destination">Object to be compared to, may be null.</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps.</param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the matcher and/or the maps, null to ignore.
		/// </param>
		/// <returns><see langword="true"/> if the two objects match.</returns>
		/// <exception cref="MapNotFoundException">The provided types could not be matched.</exception>
		/// <exception cref="MatcherException">
		/// An exception was thrown while matching the types, check the inner exception for details.
		/// </exception>
		bool Match(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			destination,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null);
	}
}
