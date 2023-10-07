using System;

namespace NeatMapper {
	public static class MatcherExtensions {
		/// <summary>
		/// Checks if two objects are the same by invoking the corresponding <see cref="IMatchMap{TSource, TDestination}.Match"/>
		/// </summary>
		/// <typeparam name="TSource">type of the source object, used to retrieve the available comparers</typeparam>
		/// <typeparam name="TDestination">type of the destination object, used to retrieve the available comparers</typeparam>
		/// <param name="source">source object, may be null</param>
		/// <param name="destination">destination object, may be null</param>
		/// <returns>true if the two objects are the same</returns>
		public static bool Match<TSource, TDestination>(this IMatcher mapper,
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
			destination) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return mapper.Match(source, typeof(TSource), destination, typeof(TDestination));
		}
	}
}
