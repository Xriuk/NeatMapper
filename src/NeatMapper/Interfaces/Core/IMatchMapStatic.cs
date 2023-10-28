﻿#if NET7_0_OR_GREATER
namespace NeatMapper {
	/// <summary>
	/// Interface which allows matching two objects
	/// </summary>
	/// <typeparam name="TSource">Source type</typeparam>
	/// <typeparam name="TDestination">Destination type</typeparam>
	public interface IMatchMapStatic<TSource, TDestination> {
		/// <summary>
		/// Checks if two objects are equivalent (usually by comparing the keys of the two)
		/// </summary>
		/// <param name="source">Source object, may be null</param>
		/// <param name="destination">Destination object, may be null</param>
		/// <param name="context">Matching context, which allows nested matches, services retrieval via DI, ...</param>
		/// <returns><see langword="true"/> if the two objects are equivalent</returns>
		public static abstract bool Match(TSource? source, TDestination? destination, MatchingContext context);
	}
}
#endif