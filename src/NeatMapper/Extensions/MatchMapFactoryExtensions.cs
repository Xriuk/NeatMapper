using System;

namespace NeatMapper {
	public static class MatchMapFactoryExtensions {
		#region Predicate
		#region Explicit source and destination
		#region Source
		/// <summary>
		/// Creates a factory which can be used as a predicate to match multiple objects against a single one.
		/// </summary>
		/// <param name="source">Object to compare other objects to, may be null.</param>
		/// <returns>
		/// A factory which can be used as a predicate to compare objects of type <typeparamref name="TDestination"/>
		/// with the provided object <paramref name="source"/>.
		/// </returns>
		public static NewMapFactory<TDestination, bool> Predicate<TSource, TDestination>(this MatchMapFactory<TSource, TDestination> factory,
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			return new DefaultNewMapFactory<TDestination, bool>(destination => factory.Invoke(source, destination));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region Destination
		/// <summary>
		/// Creates a factory which can be used as a predicate to match multiple objects against a single one.
		/// </summary>
		/// <param name="destination">Object to compare other objects to, may be null.</param>
		/// <returns>
		/// A factory which can be used as a predicate to compare objects of type <typeparamref name="TSource"/>
		/// with the provided object <paramref name="destination"/>.
		/// </returns>
		public static NewMapFactory<TSource, bool> Predicate<TSource, TDestination>(this MatchMapFactory<TSource, TDestination> factory,
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			destination) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			return new DefaultNewMapFactory<TSource, bool>(source => factory.Invoke(source, destination));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion
		#endregion
		#endregion
	}
}
