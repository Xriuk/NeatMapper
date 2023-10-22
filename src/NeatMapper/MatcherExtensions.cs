using System;
using System.Collections;

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
		public static bool Match<TSource, TDestination>(this IMatcher matcher,
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
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions = null) {
			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));
			return matcher.Match(source, typeof(TSource), destination, typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <summary>
		/// Checks if two objects are the same by invoking the corresponding <see cref="IMatchMap{TSource, TDestination}.Match"/>
		/// </summary>
		/// <typeparam name="TSource">type of the source object, used to retrieve the available comparers</typeparam>
		/// <typeparam name="TDestination">type of the destination object, used to retrieve the available comparers</typeparam>
		/// <param name="source">source object, may be null</param>
		/// <param name="destination">destination object, may be null</param>
		/// <returns>true if the two objects are the same</returns>
		public static bool Match<TSource, TDestination>(this IMatcher matcher,
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
			params object[] mappingOptions) {
			
			return matcher.Match<TSource, TDestination>(source, destination, mappingOptions.Length > 0 ? (IEnumerable)mappingOptions : null);
		}
	}
}
