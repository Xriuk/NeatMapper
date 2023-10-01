﻿namespace NeatMapper {
	/// <summary>
	/// Interface which allows matching two objects
	/// </summary>
	public interface IMatcher {
		/// <summary>
		/// Checks if two objects are the same by invoking the corresponding <see cref="IMatchMapStatic{TSource, TDestination}.Match"/>.
		/// This will create a delegate which can be invoked multiple times
		/// </summary>
		/// <param name="source">object to compare, may be null</param>
		/// <param name="sourceType">type of the source object, used to retrieve the available maps</param>
		/// <param name="destination">object to be compared to, may be null</param>
		/// <param name="destinationType">type of the destination object, used to retrieve the available maps</param>
		/// <returns>true if the two object matches</returns>
		public bool Match(object? source, Type sourceType, object? destination, Type destinationType);
	}
}