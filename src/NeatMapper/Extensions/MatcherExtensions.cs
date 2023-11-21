﻿using System;
using System.Collections;

namespace NeatMapper {
	public static class MatcherExtensions {
		#region Match
		#region Runtime
		/// <inheritdoc cref="IMatcher.Match(object, Type, object, Type, MappingOptions)"/>
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
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IMatcher.Match(object, Type, object, Type, MappingOptions)"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMatcher.Match(object, Type, object, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMatcher.Match(object, Type, object, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
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

		/// <inheritdoc cref="Match{TSource, TDestination}(IMatcher, TSource, TDestination, MappingOptions)"/>
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
		#endregion
		#endregion

		#region CanMatch
		#region Runtime
		/// <summary>
		/// Checks if the matcher could match an object with another one, will check if the given matcher supports
		/// <see cref="IMatcherCanMatch"/> first otherwise will create a dummy source and a dummy destination objects
		/// (cached) and try to match them. It does not guarantee that the actual map will succeed.
		/// </summary>
		/// <inheritdoc cref="IMatcherCanMatch.CanMatch(Type, Type, MappingOptions)"/>
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

		/// <inheritdoc cref="CanMatch(IMatcher, Type, Type, MappingOptions)"/>
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
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="CanMatch(IMatcher, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="CanMatch(IMatcher, Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="CanMatch(IMatcher, Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="CanMatch(IMatcher, Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be matched
		/// with an object of type <typeparamref name="TSource"/>.
		/// </returns>
		/// <inheritdoc cref="CanMatch(IMatcher, Type, Type, MappingOptions)" path="/exception"/>
		public static bool CanMatch<TSource, TDestination>(this IMatcher matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return matcher.CanMatch(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="CanMatch{TSource, TDestination}(IMatcher, MappingOptions)"/>
		public static bool CanMatch<TSource, TDestination>(this IMatcher matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return matcher.CanMatch(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion
	}
}
