using System;
using System.Collections;

namespace NeatMapper {
	public static class MatcherExtensions {
		#region Runtime
		/// <summary>
		/// Checks if two objects are the same by invoking the corresponding <see cref="IMatchMap{TSource, TDestination}.Match"/>.
		/// This will create a delegate which can be invoked multiple times
		/// </summary>
		/// <param name="source">Object to compare, may be null</param>
		/// <param name="sourceType">Type of the source object, used to retrieve the available maps</param>
		/// <param name="destination">Object to be compared to, may be null</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore</param>
		/// <returns>True if the two objects match</returns>
		public static bool Match(this IMatcher matcher,
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
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));
			return matcher.Match(source, sourceType, destination, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <summary>
		/// Checks if two objects are the same by invoking the corresponding <see cref="IMatchMap{TSource, TDestination}.Match"/>.
		/// This will create a delegate which can be invoked multiple times
		/// </summary>
		/// <param name="source">Object to compare, may be null</param>
		/// <param name="sourceType">Type of the source object, used to retrieve the available maps</param>
		/// <param name="destination">Object to be compared to, may be null</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore</param>
		/// <returns>True if the two objects match</returns>
		public static bool Match(this IMatcher matcher,
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
			params object[] mappingOptions) {

			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));
			return matcher.Match(source, sourceType, destination, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <summary>
		/// Checks if two objects are the same by invoking the corresponding <see cref="IMatchMap{TSource, TDestination}.Match"/>
		/// </summary>
		/// <typeparam name="TSource">Type of the source object, used to retrieve the available comparers</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available comparers</typeparam>
		/// <param name="source">Source object, may be null</param>
		/// <param name="destination">Destination object, may be null</param>
		/// <returns>True if the two objects are the same</returns>
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
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {
			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));
			return matcher.Match(source, typeof(TSource), destination, typeof(TDestination), mappingOptions);
		}

		/// <summary>
		/// Checks if two objects are the same by invoking the corresponding <see cref="IMatchMap{TSource, TDestination}.Match"/>
		/// </summary>
		/// <typeparam name="TSource">Type of the source object, used to retrieve the available comparers</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available comparers</typeparam>
		/// <param name="source">Source object, may be null</param>
		/// <param name="destination">Destination object, may be null</param>
		/// <returns>True if the two objects are the same</returns>
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

			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));
			return matcher.Match(source, typeof(TSource), destination, typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		} 
		#endregion
	}
}
