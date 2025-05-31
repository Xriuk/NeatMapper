using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	public static class MatcherExtensions {
		#region CanMatch
		#region Runtime
		/// <inheritdoc cref="IMatcher.CanMatch(Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMatch(this IMatcher matcher,
			Type sourceType,
			Type destinationType,
			IEnumerable? mappingOptions) {

			return matcher.CanMatch(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="IMatcher.CanMatch(Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMatch(this IMatcher matcher,
			Type sourceType,
			Type destinationType,
			params object?[]? mappingOptions) {

			return matcher.CanMatch(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IMatcher.CanMatch(Type, Type, MappingOptions?)" path="/summary"/>
		/// <typeparam name="TSource">
		/// Type of the source object, used to retrieve the available maps.
		/// </typeparam>
		/// <typeparam name="TDestination">
		/// Type of the destination object, used to retrieve the available maps.
		/// </typeparam>
		/// <inheritdoc cref="IMatcher.CanMatch(Type, Type, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be matched
		/// with an object of type <typeparamref name="TSource"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMatch<TSource, TDestination>(this IMatcher matcher, MappingOptions? mappingOptions = null) {
			return matcher.CanMatch(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="CanMatch{TSource, TDestination}(IMatcher, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMatch<TSource, TDestination>(this IMatcher matcher, IEnumerable? mappingOptions) {
			return matcher.CanMatch(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="CanMatch{TSource, TDestination}(IMatcher, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMatch<TSource, TDestination>(this IMatcher matcher, params object?[]? mappingOptions) {
			return matcher.CanMatch(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region Match
		#region Runtime
		/// <inheritdoc cref="IMatcher.Match(object?, Type, object?, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Match(this IMatcher matcher,
			object? source,
			Type sourceType,
			object? destination,
			Type destinationType,
			IEnumerable? mappingOptions) {

			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));

			return matcher.Match(source, sourceType, destination, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="IMatcher.Match(object?, Type, object?, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Match(this IMatcher matcher,
			object? source,
			Type sourceType,
			object? destination,
			Type destinationType,
			params object?[]? mappingOptions) {

			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));

			return matcher.Match(source, sourceType, destination, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IMatcher.Match(object?, Type, object?, Type, MappingOptions?)"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMatcher.Match(object?, Type, object?, Type, MappingOptions?)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMatcher.Match(object?, Type, object?, Type, MappingOptions?)" path="/param[@name='destinationType']"/></typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Match<TSource, TDestination>(this IMatcher matcher,
			TSource? source,
			TDestination? destination,
			MappingOptions? mappingOptions = null) {

			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));

			return matcher.Match(source, typeof(TSource), destination, typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="Match{TSource, TDestination}(IMatcher, TSource, TDestination, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Match<TSource, TDestination>(this IMatcher matcher,
			TSource? source,
			TDestination? destination,
			IEnumerable? mappingOptions) {

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
		/// <see cref="IMatcher.Match(object?, Type, object?, Type, MappingOptions?)"/> wrapped in a delegate.
		/// </summary>
		/// <remarks>It is NOT guaranteed that the created factory shares the same <see cref="MappingContext"/>.</remarks>
		/// <inheritdoc cref="IMatcherFactory.MatchFactory(Type, Type, MappingOptions?)"/>
		public static IMatchMapFactory MatchFactory(this IMatcher matcher,
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null) {

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
		}

		/// <inheritdoc cref="MatchFactory(IMatcher, Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IMatchMapFactory MatchFactory(this IMatcher matcher,
			Type sourceType,
			Type destinationType,
			IEnumerable? mappingOptions) {

			return matcher.MatchFactory(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MatchFactory(IMatcher, Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IMatchMapFactory MatchFactory(this IMatcher matcher,
			Type sourceType,
			Type destinationType,
			params object?[]? mappingOptions) {

			return matcher.MatchFactory(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="MatchFactory(IMatcher, Type, Type, MappingOptions?)" path="/summary"/>
		/// <inheritdoc cref="MatchFactory(IMatcher, Type, Type, MappingOptions?)" path="/remarks"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMatcherFactory.MatchFactory(Type, Type, MappingOptions?)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMatcherFactory.MatchFactory(Type, Type, MappingOptions?)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMatcherFactory.MatchFactory(Type, Type, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="IMatcherFactory.MatchFactory(Type, Type, MappingOptions?)" path="/returns"/>
		/// <inheritdoc cref="IMatcherFactory.MatchFactory(Type, Type, MappingOptions?)" path="/exception"/>
		public static MatchMapFactory<TSource, TDestination> MatchFactory<TSource, TDestination>(this IMatcher matcher,
			MappingOptions? mappingOptions = null) {

			var factory = matcher.MatchFactory(typeof(TSource), typeof(TDestination), mappingOptions);
			try {
				return new DisposableMatchMapFactory<TSource, TDestination>((source, destination) => factory.Invoke(source, destination), factory);
			}
			catch {
				factory.Dispose();
				throw;
			}
		}

		/// <inheritdoc cref="MatchFactory{TSource, TDestination}(IMatcher, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MatchMapFactory<TSource, TDestination> MatchFactory<TSource, TDestination>(this IMatcher matcher,
			IEnumerable? mappingOptions) {

			return matcher.MatchFactory<TSource, TDestination>(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MatchFactory{TSource, TDestination}(IMatcher, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MatchMapFactory<TSource, TDestination> MatchFactory<TSource, TDestination>(this IMatcher matcher,
			params object?[]? mappingOptions) {

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
			object? source,
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null) {

			return matcher.MatchFactory(sourceType, destinationType, mappingOptions).Predicate(source);
		}

		/// <inheritdoc cref="Predicate(IMatcher, object?, Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IPredicateFactory Predicate(this IMatcher matcher,
			object? source,
			Type sourceType,
			Type destinationType,
			IEnumerable? mappingOptions) {

			return matcher.Predicate(source, sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Predicate(IMatcher, object?, Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IPredicateFactory Predicate(this IMatcher matcher,
			object? source,
			Type sourceType,
			Type destinationType,
			params object?[]? mappingOptions) {

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
			object? destination,
			Type destinationType,
			MappingOptions? mappingOptions = null) {

			return matcher.MatchFactory(sourceType, destinationType, mappingOptions).PredicateDestination(destination);
		}

		/// <inheritdoc cref="Predicate(IMatcher, Type, object?, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IPredicateFactory Predicate(this IMatcher matcher,
			Type sourceType,
			object? destination,
			Type destinationType,
			IEnumerable? mappingOptions) {

			return matcher.Predicate(sourceType, destination, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Predicate(IMatcher, Type, object?, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IPredicateFactory Predicate(this IMatcher matcher,
			Type sourceType,
			object? destination,
			Type destinationType,
			params object?[]? mappingOptions) {

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
			MappingOptions? mappingOptions = null) {

			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));
			if (source == null) { 
				throw new ArgumentNullException(nameof(source),
					"Type cannot be inferred from null source, use an overload with an explicit source type");
			}

			return matcher.MatchFactory(source.GetType(), typeof(TDestination), mappingOptions).Predicate<TDestination>(source);
		}

		/// <inheritdoc cref="Predicate{TDestination}(IMatcher, object?, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TDestination> Predicate<TDestination>(this IMatcher matcher,
			object source,
			IEnumerable? mappingOptions) {

			return matcher.Predicate<TDestination>(source, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Predicate{TDestination}(IMatcher, object?, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TDestination> Predicate<TDestination>(this IMatcher matcher,
			object source,
			params object?[]? mappingOptions) {

			return matcher.Predicate<TDestination>(source, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source, inferred destination
		/// <summary>
		/// Creates a factory which can be used as a predicate to match multiple objects against a single one.
		/// </summary>
		/// <param name="destination">
		/// Object to compare other objects to, CANNOT be null as the source type will be retrieved from it at runtime,
		/// which will be used to retrieve the available maps
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the matcher and/or the maps, null to ignore.
		/// </param>
		/// <returns>
		/// A factory which can be used as a predicate to compare objects of type <typeparamref name="TSource"/>
		/// with the provided object <paramref name="destination"/>.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be matched.</exception>
		public static PredicateFactory<TSource> PredicateDestination<TSource>(this IMatcher matcher,
			object destination,
			MappingOptions? mappingOptions = null) {

			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));
			if (destination == null) {
				throw new ArgumentNullException(nameof(destination),
					"Type cannot be inferred from null destination, use an overload with an explicit destination type");
			}

			return matcher.MatchFactory(typeof(TSource), destination.GetType(), mappingOptions).Predicate<TSource>(destination);
		}

		/// <inheritdoc cref="PredicateDestination{TSource}(IMatcher, object?, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TSource> PredicateDestination<TSource>(this IMatcher matcher,
			object source,
			IEnumerable? mappingOptions) {

			return matcher.PredicateDestination<TSource>(source, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="PredicateDestination{TSource}(IMatcher, object?, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TSource> PredicateDestination<TSource>(this IMatcher matcher,
			object source,
			params object?[]? mappingOptions) {

			return matcher.PredicateDestination<TSource>(source, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
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
			TSource? source,
			MappingOptions? mappingOptions = null) {

			return matcher.MatchFactory<TSource, TDestination>(mappingOptions).Predicate(source);
		}

		/// <inheritdoc cref="Predicate{TSource, TDestination}(IMatcher, TSource, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TDestination> Predicate<TSource, TDestination>(this IMatcher matcher,
			TSource? source,
			IEnumerable? mappingOptions) {

			return matcher.Predicate<TSource, TDestination>(source, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Predicate{TSource, TDestination}(IMatcher, TSource, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TDestination> Predicate<TSource, TDestination>(this IMatcher matcher,
			TSource? source,
			params object?[]? mappingOptions) {

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
		public static PredicateFactory<TSource> PredicateDestination<TSource, TDestination>(this IMatcher matcher,
			TDestination? destination,
			MappingOptions? mappingOptions = null) {

			return matcher.MatchFactory<TSource, TDestination>(mappingOptions).PredicateDestination(destination);
		}

		/// <inheritdoc cref="PredicateDestination{TSource, TDestination}(IMatcher, TDestination, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TSource> PredicateDestination<TSource, TDestination>(this IMatcher matcher,
			TDestination? destination,
			IEnumerable? mappingOptions) {

			return matcher.PredicateDestination<TSource, TDestination>(destination, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="PredicateDestination{TSource, TDestination}(IMatcher, TDestination, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PredicateFactory<TSource> PredicateDestination<TSource, TDestination>(this IMatcher matcher,
			TDestination? destination,
			params object?[]? mappingOptions) {

			return matcher.PredicateDestination<TSource, TDestination>(destination, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}


		#region Deprecated
		/// <inheritdoc cref="PredicateDestination{TSource, TDestination}(IMatcher, TDestination, MappingOptions?)"/>
		[Obsolete("This method will be removed in future versions, use PredicateDestination() instead.")]
		public static PredicateFactory<TSource> Predicate<TSource, TDestination>(this IMatcher matcher,
			TDestination? destination,
			MappingOptions? mappingOptions = null) {

			return matcher.PredicateDestination<TSource, TDestination>(destination, mappingOptions);
		}

		/// <inheritdoc cref="PredicateDestination{TSource, TDestination}(IMatcher, TDestination, MappingOptions?)"/>
		[Obsolete("This method will be removed in future versions, use PredicateDestination() instead.")]
		public static PredicateFactory<TSource> Predicate<TSource, TDestination>(this IMatcher matcher,
			TDestination? destination,
			IEnumerable? mappingOptions) {

			return matcher.PredicateDestination<TSource, TDestination>(destination, mappingOptions);
		}

		/// <inheritdoc cref="PredicateDestination{TSource, TDestination}(IMatcher, TDestination, MappingOptions?)"/>
		[Obsolete("This method will be removed in future versions, use PredicateDestination() instead.")]
		public static PredicateFactory<TSource> Predicate<TSource, TDestination>(this IMatcher matcher,
			TDestination? destination,
			params object?[]? mappingOptions) {

			return matcher.PredicateDestination<TSource, TDestination>(destination, mappingOptions);
		}
		#endregion
		#endregion
		#endregion
		#endregion
	}
}
