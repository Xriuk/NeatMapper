using System;
using System.Collections;

namespace NeatMapper {
	public static class MatcherExtensions {
		#region Match
		#region Runtime
		/// <summary>
		/// Checks if two objects are equivalent by invoking the corresponding <see cref="IMatchMap{TSource, TDestination}.Match"/>.
		/// This will create a delegate which can be invoked multiple times
		/// </summary>
		/// <param name="source">Object to compare, may be null</param>
		/// <param name="sourceType">Type of the source object, used to retrieve the available maps</param>
		/// <param name="destination">Object to be compared to, may be null</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore</param>
		/// <returns><see langword="true"/> if the two objects match</returns>
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
		/// Checks if two objects are equivalent by invoking the corresponding <see cref="IMatchMap{TSource, TDestination}.Match"/>.
		/// This will create a delegate which can be invoked multiple times
		/// </summary>
		/// <param name="source">Object to compare, may be null</param>
		/// <param name="sourceType">Type of the source object, used to retrieve the available maps</param>
		/// <param name="destination">Object to be compared to, may be null</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore</param>
		/// <returns><see langword="true"/> if the two objects match</returns>
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
		/// Checks if two objects are equivalent by invoking the corresponding <see cref="IMatchMap{TSource, TDestination}.Match"/>
		/// </summary>
		/// <typeparam name="TSource">Type of the source object, used to retrieve the available comparers</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available comparers</typeparam>
		/// <param name="source">Source object, may be null</param>
		/// <param name="destination">Destination object, may be null</param>
		/// <returns><see langword="true"/> if the two objects are equivalent</returns>
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
		/// Checks if two objects are equivalent by invoking the corresponding <see cref="IMatchMap{TSource, TDestination}.Match"/>
		/// </summary>
		/// <typeparam name="TSource">Type of the source object, used to retrieve the available comparers</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available comparers</typeparam>
		/// <param name="source">Source object, may be null</param>
		/// <param name="destination">Destination object, may be null</param>
		/// <returns><see langword="true"/> if the two objects are equivalent</returns>
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
			mappingOptions) {

			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));
			return matcher.Match(source, typeof(TSource), destination, typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <summary>
		/// Checks if two objects are equivalent by invoking the corresponding <see cref="IMatchMap{TSource, TDestination}.Match"/>
		/// </summary>
		/// <typeparam name="TSource">Type of the source object, used to retrieve the available comparers</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available comparers</typeparam>
		/// <param name="source">Source object, may be null</param>
		/// <param name="destination">Destination object, may be null</param>
		/// <returns><see langword="true"/> if the two objects are equivalent</returns>
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
		#endregion

		#region CanMatch
		#region Runtime
		/// <summary>
		/// Checks if the matcher can match an object with another one, will check if the given matcher supports
		/// <see cref="IMatcherCanMatch"/> first otherwise will create a dummy source and destination objects
		/// (cached) and try to match them
		/// </summary>
		/// <param name="sourceType">Type of the source object, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to match the types, this helps obtaining more accurate results,
		/// since some matchers may depend on specific options to match or not two given types
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <paramref name="destinationType"/> can be matched
		/// with an object of type <paramref name="sourceType"/>
		/// </returns>
		/// <exception cref="InvalidOperationException">Could not verify if the matcher supports the given types</exception>
		public static bool CanMatch(this IMatcher matcher,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Check if the mapper implements IMapperCanMap, if it throws it means that the map can be checked only when mapping
			if (matcher is IMatcherCanMatch matcherCanMatch)
				return matcherCanMatch.CanMatch(sourceType, destinationType, mappingOptions);

			// Try creating two default source and destination objects and try mapping them
			object source;
			object destination;
			try {
				source = ObjectFactory.GetOrCreateCached(sourceType) ?? throw new Exception(); // Just in case
				destination = ObjectFactory.GetOrCreateCached(destinationType) ?? throw new Exception(); // Just in case
			}
			catch {
				throw new InvalidOperationException("Cannot verify if the matcher supports the given match because unable to create the objects to test it");
			}

			try {
				matcher.Match(source, sourceType, destination, destinationType, mappingOptions);
				return true;
			}
			catch (MapNotFoundException) {
				return false;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Checks if the matcher can match an object with another one, will check if the given matcher supports
		/// <see cref="IMatcherCanMatch"/> first otherwise will create a dummy source and destination objects
		/// (cached) and try to match them
		/// </summary>
		/// <param name="sourceType">Type of the source object, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to match the types, this helps obtaining more accurate results,
		/// since some matchers may depend on specific options to match or not two given types
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <paramref name="destinationType"/> can be matched
		/// with an object of type <paramref name="sourceType"/>
		/// </returns>
		/// <exception cref="InvalidOperationException">Could not verify if the matcher supports the given types</exception>
		public static bool CanMatch(this IMatcher matcher,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return matcher.CanMatch(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <summary>
		/// Checks if the matcher can match an object with another one, will check if the given matcher supports
		/// <see cref="IMatcherCanMatch"/> first otherwise will create a dummy source and destination objects
		/// (cached) and try to match them
		/// </summary>
		/// <param name="sourceType">Type of the source object, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to match the types, this helps obtaining more accurate results,
		/// since some matchers may depend on specific options to match or not two given types
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <paramref name="destinationType"/> can be matched
		/// with an object of type <paramref name="sourceType"/>
		/// </returns>
		/// <exception cref="InvalidOperationException">Could not verify if the matcher supports the given types</exception>
		public static bool CanMatch(this IMatcher matcher, Type sourceType, Type destinationType, params object[] mappingOptions) {
			return matcher.CanMatch(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <summary>
		/// Checks if the matcher can match an object with another one, will check if the given matcher supports
		/// <see cref="IMatcherCanMatch"/> first otherwise will create a dummy source and destination objects
		/// (cached) and try to match them
		/// </summary>
		/// <typeparam name="TSource">Type of the source object, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="mappingOptions">
		/// Additional options which would be used to match the types, this helps obtaining more accurate results,
		/// since some matchers may depend on specific options to match or not two given types
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be matched
		/// with an object of type <typeparamref name="TSource"/>
		/// </returns>
		/// <exception cref="InvalidOperationException">Could not verify if the matcher supports the given types</exception>
		public static bool CanMatch<TSource, TDestination>(this IMatcher matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return matcher.CanMatch(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <summary>
		/// Checks if the matcher can match an object with another one, will check if the given matcher supports
		/// <see cref="IMatcherCanMatch"/> first otherwise will create a dummy source and destination objects
		/// (cached) and try to match them
		/// </summary>
		/// <typeparam name="TSource">Type of the source object, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="mappingOptions">
		/// Additional options which would be used to match the types, this helps obtaining more accurate results,
		/// since some matchers may depend on specific options to match or not two given types
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be matched
		/// with an object of type <typeparamref name="TSource"/>
		/// </returns>
		/// <exception cref="InvalidOperationException">Could not verify if the matcher supports the given types</exception>
		public static bool CanMatch<TSource, TDestination>(this IMatcher matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return matcher.CanMatch(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <summary>
		/// Checks if the matcher can match an object with another one, will check if the given matcher supports
		/// <see cref="IMatcherCanMatch"/> first otherwise will create a dummy source and destination objects
		/// (cached) and try to match them
		/// </summary>
		/// <typeparam name="TSource">Type of the source object, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="mappingOptions">
		/// Additional options which would be used to match the types, this helps obtaining more accurate results,
		/// since some matchers may depend on specific options to match or not two given types
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be matched
		/// with an object of type <typeparamref name="TSource"/>
		/// </returns>
		/// <exception cref="InvalidOperationException">Could not verify if the matcher supports the given types</exception>
		public static bool CanMatch<TSource, TDestination>(this IMatcher matcher, params object[] mappingOptions) {
			return matcher.CanMatch(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion
	}
}
