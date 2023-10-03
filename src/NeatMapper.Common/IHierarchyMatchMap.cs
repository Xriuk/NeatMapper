﻿namespace NeatMapper {
	/// <summary>
	/// Interface which allows matching two objects of two given types or derived types
	/// </summary>
	/// <typeparam name="TSource">Source type, includes derived types</typeparam>
	/// <typeparam name="TDestination">Destination type, includes derived types</typeparam>
	public interface IHierarchyMatchMap<TSource, TDestination> {
		/// <summary>
		/// Checks if two objects are the same (usually by comparing the keys of the two)
		/// </summary>
		/// <param name="source">source object, may be null</param>
		/// <param name="destination">destination object, may be null</param>
		/// <param name="context">matching context, which allows nested matches, services retrieval via DI, ...</param>
		/// <returns>true if the two objects match</returns>
		bool Match(
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			destination,
			MatchingContext context);
	}
}