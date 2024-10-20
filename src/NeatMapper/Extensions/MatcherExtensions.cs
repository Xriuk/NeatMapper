using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	public static class MatcherExtensions {
		#region CanMatch
		#region Runtime
		/// <inheritdoc cref="IMatcher.CanMatch(Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		/// <inheritdoc cref="IMatcher.CanMatch(Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMatch(this IMatcher matcher,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return matcher.CanMatch(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IMatcher.CanMatch(Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMatcher.CanMatch(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMatcher.CanMatch(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMatcher.CanMatch(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be matched
		/// with an object of type <typeparamref name="TSource"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMatch<TSource, TDestination>(this IMatcher matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return matcher.CanMatch(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="CanMatch{TSource, TDestination}(IMatcher, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMatch<TSource, TDestination>(this IMatcher matcher,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return matcher.CanMatch(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region Match
		#region Runtime
		/// <inheritdoc cref="IMatcher.Match(object, Type, object, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		/// <inheritdoc cref="IMatcher.Match(object, Type, object, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));

			return matcher.Match(source, sourceType, destination, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IMatcher.Match(object, Type, object, Type, MappingOptions)"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMatcher.Match(object, Type, object, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMatcher.Match(object, Type, object, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		// DEV: cannot have a "params object[] mappingOptions" overload because causes ambiguity with Runtime overloads
		// (with "IEnumerable mappingOptions") when types are not specified (which is a farly-widely used case)
		#endregion
		#endregion

		#region MatchFactory
		#region Runtime
		/// <summary>
		/// Creates a factory which can be used to check if two objects are equivalent,
		/// will check if the given matcher supports <see cref="IMatcherFactory"/> first otherwise will return
		/// <see cref="IMatcher.Match(object, Type, object, Type, MappingOptions)"/> wrapped in a delegate.
		/// </summary>
		/// <remarks>It is NOT guaranteed that the created factory shares the same <see cref="MappingContext"/>.</remarks>
		/// <inheritdoc cref="IMatcherFactory.MatchFactory(Type, Type, MappingOptions)"/>
		public static IMatchMapFactory MatchFactory(this IMatcher matcher,
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

			// Check if the matcher implements IMatcherFactory
			if (matcher is IMatcherFactory matcherFactory)
				return matcherFactory.MatchFactory(sourceType, destinationType, mappingOptions);

			// Check if the matcher can match the types (we don't do it via the extension method above because
			// it may require actually mapping the two types if the interface is not implemented,
			// and as the returned factory may still throw MapNotFoundException we are still compliant)
			try {
				if (!matcher.CanMatch(sourceType, destinationType))
					throw new MapNotFoundException((sourceType, destinationType));
			}
			catch (MapNotFoundException) {
				throw;
			}
			catch { }

			// Return the match wrapped
			return new DefaultMatchMapFactory(sourceType, destinationType, (source, destination) => matcher.Match(source, sourceType, destination, destinationType, mappingOptions));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MatchFactory(IMatcher, Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IMatchMapFactory MatchFactory(this IMatcher matcher,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return matcher.MatchFactory(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MatchFactory(IMatcher, Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IMatchMapFactory MatchFactory(this IMatcher matcher,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return matcher.MatchFactory(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="MatchFactory(IMatcher, Type, Type, MappingOptions)" path="/summary"/>
		/// <inheritdoc cref="MatchFactory(IMatcher, Type, Type, MappingOptions)" path="/remarks"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMatcherFactory.MatchFactory(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMatcherFactory.MatchFactory(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMatcherFactory.MatchFactory(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="IMatcherFactory.MatchFactory(Type, Type, MappingOptions)" path="/returns"/>
		/// <inheritdoc cref="IMatcherFactory.MatchFactory(Type, Type, MappingOptions)" path="/exception"/>
		public static MatchMapFactory<TSource, TDestination> MatchFactory<TSource, TDestination>(this IMatcher matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			var factory = matcher.MatchFactory(typeof(TSource), typeof(TDestination), mappingOptions);
			try {
				return new DisposableMatchMapFactory<TSource, TDestination>((source, destination) => factory.Invoke(source, destination), factory);
			}
			catch {
				factory.Dispose();
				throw;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MatchFactory{TSource, TDestination}(IMatcher, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MatchMapFactory<TSource, TDestination> MatchFactory<TSource, TDestination>(this IMatcher matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return matcher.MatchFactory<TSource, TDestination>(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MatchFactory{TSource, TDestination}(IMatcher, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MatchMapFactory<TSource, TDestination> MatchFactory<TSource, TDestination>(this IMatcher matcher,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return matcher.MatchFactory<TSource, TDestination>(mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region Predicate
		#region Runtime
		#region Source
		/// <summary>
		/// Creates a factory which can be used as a predicate to match multiple objects against a single one.
		/// </summary>
		/// <param name="source">Object to compare other objects to, may be null.</param>
		/// <param name="sourceType">Type of the object to compare to, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the other objects to compare with <paramref name="source"/>, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the matcher and/or the maps, null to ignore.
		/// </param>
		/// <returns>
		/// A factory which can be used as a predicate to compare objects of type <paramref name="destinationType"/>
		/// with the provided object <paramref name="source"/>.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be matched.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IPredicateFactory Predicate(this IMatcher matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return matcher.MatchFactory(sourceType, destinationType, mappingOptions).Predicate(source);
		}

		/// <inheritdoc cref="Predicate(IMatcher, object, Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IPredicateFactory Predicate(this IMatcher matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return matcher.Predicate(source, sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Predicate(IMatcher, object, Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IPredicateFactory Predicate(this IMatcher matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return matcher.Predicate(source, sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Destination
		/// <summary>
		/// Creates a factory which can be used as a predicate to match multiple objects against a single one.
		/// </summary>
		/// <param name="sourceType">
		/// Type of the other objects to compare with <paramref name="destination"/>, used to retrieve the available maps.
		/// </param>
		/// <param name="destination">Object to compare other objects to, may be null.</param>
		/// <param name="destinationType">Type of the object to compare to, used to retrieve the available maps.</param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the matcher and/or the maps, null to ignore.
		/// </param>
		/// <returns>
		/// A factory which can be used as a predicate to compare objects of type <paramref name="sourceType"/>
		/// with the provided object <paramref name="destination"/>.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be matched.</exception>
		public static IPredicateFactory Predicate(this IMatcher matcher,
			Type sourceType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			destination,
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

			var factory = matcher.MatchFactory(sourceType, destinationType, mappingOptions);
			return new DisposablePredicateFactory(
				factory.DestinationType, factory.SourceType,
				source => factory.Invoke(source, destination),
				factory);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Predicate(IMatcher, Type, object, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IPredicateFactory Predicate(this IMatcher matcher,
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

			return matcher.Predicate(sourceType, destination, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Predicate(IMatcher, Type, object, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IPredicateFactory Predicate(this IMatcher matcher,
			Type sourceType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			destination,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return matcher.Predicate(sourceType, destination, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region Explicit destination, inferred source
		/// <summary>
		/// Creates a factory which can be used as a predicate to match multiple objects against a single one.
		/// </summary>
		/// <param name="source">
		/// Object to compare other objects to, CANNOT be null as the source type will be retrieved from it at runtime,
		/// which will be used to retrieve the available maps
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the matcher and/or the maps, null to ignore.
		/// </param>
		/// <returns>
		/// A factory which can be used as a predicate to compare objects of type <typeparamref name="TDestination"/>
		/// with the provided object <paramref name="source"/>.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be matched.</exception>
		public static PredicateFactory<TDestination> Predicate<TDestination>(this IMatcher matcher,
			object source,
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
			if (source == null) { 
				throw new ArgumentNullException(nameof(source),
					"Type cannot be inferred from null source, use an overload with an explicit source type");
			}

			return matcher.MatchFactory(source.GetType(), typeof(TDestination), mappingOptions).Predicate<TDestination>(source);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Predicate{TDestination}(IMatcher, object, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TDestination> Predicate<TDestination>(this IMatcher matcher,
			object source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return matcher.Predicate<TDestination>(source, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Predicate{TDestination}(IMatcher, object, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TDestination> Predicate<TDestination>(this IMatcher matcher,
			object source,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return matcher.Predicate<TDestination>(source, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		#region Source
		/// <summary>
		/// Creates a factory which can be used as a predicate to match multiple objects against a single one.
		/// </summary>
		/// <param name="source">Object to compare other objects to, may be null.</param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the matcher and/or the maps, null to ignore.
		/// </param>
		/// <returns>
		/// A factory which can be used as a predicate to compare objects of type <typeparamref name="TDestination"/>
		/// with the provided object <paramref name="source"/>.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be matched.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TDestination> Predicate<TSource, TDestination>(this IMatcher matcher,
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return matcher.MatchFactory<TSource, TDestination>(mappingOptions).Predicate(source);
		}

		/// <inheritdoc cref="Predicate{TSource, TDestination}(IMatcher, TSource, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TDestination> Predicate<TSource, TDestination>(this IMatcher matcher,
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return matcher.Predicate<TSource, TDestination>(source, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Predicate{TSource, TDestination}(IMatcher, TSource, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TDestination> Predicate<TSource, TDestination>(this IMatcher matcher,
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return matcher.Predicate<TSource, TDestination>(source, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Destination
		/// <summary>
		/// Creates a factory which can be used as a predicate to match multiple objects against a single one.
		/// </summary>
		/// <param name="destination">Object to compare other objects to, may be null.</param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the matcher and/or the maps, null to ignore.
		/// </param>
		/// <returns>
		/// A factory which can be used as a predicate to compare objects of type <typeparamref name="TSource"/>
		/// with the provided object <paramref name="destination"/>.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be matched.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TSource> Predicate<TSource, TDestination>(this IMatcher matcher,
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

			return matcher.MatchFactory<TSource, TDestination>(mappingOptions).Predicate(destination);
		}

		/// <inheritdoc cref="Predicate{TSource, TDestination}(IMatcher, TDestination, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TSource> Predicate<TSource, TDestination>(this IMatcher matcher,
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

			return matcher.Predicate<TSource, TDestination>(destination, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Predicate{TSource, TDestination}(IMatcher, TDestination, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TSource> Predicate<TSource, TDestination>(this IMatcher matcher,
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			destination,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return matcher.Predicate<TSource, TDestination>(destination, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion
		#endregion
	}
}
