using System;

namespace NeatMapper {
	/// <summary>
	/// Interface which allows matching two objects.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IMatcher {
		/// <summary>
		/// Checks if the matcher could match an object with another one. It does not guarantee that the actual map will succeed.
		/// </summary>
		/// <param name="sourceType">Type of the source object, used to retrieve the available maps.</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps.</param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to match the types, this helps obtaining more accurate results,
		/// since some matchers may depend on specific options to match or not two given types.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <paramref name="destinationType"/> can be matched
		/// with an object of type <paramref name="sourceType"/>.
		/// </returns>
		bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null);

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
		bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null);
	}
}
