namespace NeatMapper {
	public static class MatcherExtensions {
		/// <summary>
		/// Checks if two objects are the same by invoking the corresponding <see cref="IMatchMapStatic{TSource, TDestination}.Match"/>
		/// </summary>
		/// <typeparam name="TSource">type of the source object, used to retrieve the available comparers</typeparam>
		/// <typeparam name="TDestination">type of the destination object, used to retrieve the available comparers</typeparam>
		/// <param name="source">source object, may be null</param>
		/// <param name="destination">destination object, may be null</param>
		/// <returns>true if the two objects are the same</returns>
		public static bool Match<TSource, TDestination>(this IMatcher mapper, TSource? source, TDestination? destination) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return mapper.Match(source, typeof(TSource), destination, typeof(TDestination));
		}
	}
}
