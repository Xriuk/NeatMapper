using System;

namespace NeatMapper {
	/// <summary>
	/// Interface which allows matching two objects
	/// </summary>
	public interface IMatcher {
		/// <summary>
		/// Checks if two objects are the same by invoking the corresponding <see cref="IMatchMap{TSource, TDestination}.Match"/>.
		/// This will create a delegate which can be invoked multiple times
		/// </summary>
		/// <param name="source">object to compare, may be null</param>
		/// <param name="sourceType">type of the source object, used to retrieve the available maps</param>
		/// <param name="destination">object to be compared to, may be null</param>
		/// <param name="destinationType">type of the destination object, used to retrieve the available maps</param>
		/// <returns>true if the two objects match</returns>
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
			Type destinationType);
	}
}
