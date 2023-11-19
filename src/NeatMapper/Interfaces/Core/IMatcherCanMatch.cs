using System;

namespace NeatMapper {
	/// <summary>
	/// Optional interface to be implemented by matchers, which allows to discover if two given types can be matched or not.
	/// </summary>
	public interface IMatcherCanMatch : IMatcher {
		/// <summary>
		/// Checks if the matcher can match an object with another one.
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
		/// <exception cref="InvalidOperationException">Could not verify if the matcher supports the given types.</exception>
		bool CanMatch(
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
