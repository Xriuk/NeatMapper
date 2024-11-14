using System;

namespace NeatMapper {
	public static class MatchMapFactoryExtensions {
		#region Predicate
		/// <summary>
		/// Creates a factory which can be used as a predicate to match multiple objects against a single one.
		/// </summary>
		/// <param name="source">
		/// Object to compare other objects to of type <see cref="IMatchMapFactory.SourceType"/>, may be null.
		/// </param>
		/// <param name="shouldDispose">
		/// True if the method should dispose the provided factory on creation exceptions or inside the returned factory,
		/// false if the provided factory will be disposed elsewhere.
		/// </param>
		/// <returns>
		/// A factory which can be used as a predicate to compare objects of type <see cref="IMatchMapFactory.DestinationType"/>
		/// with the provided object <paramref name="source"/>.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="source"/> type is not assignable to <see cref="IMatchMapFactory.SourceType"/>
		/// of the provided factory.
		/// </exception>
		public static IPredicateFactory Predicate(this IMatchMapFactory factory,
			object? source,
			bool shouldDispose = true) {

			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			try { 
				TypeUtils.CheckObjectType(source, factory.SourceType, nameof(source));

				return new DisposablePredicateFactory(
					factory.SourceType, factory.DestinationType,
					destination => factory.Invoke(source, destination),
					shouldDispose ? factory : null);
			}
			catch {
				if(shouldDispose)
					factory.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Creates a factory which can be used as a predicate to match multiple objects against a single one.
		/// </summary>
		/// <param name="destination">
		/// Object to compare other objects to of type <see cref="IMatchMapFactory.DestinationType"/>, may be null.
		/// </param>
		/// <param name="shouldDispose">
		/// True if the method should dispose the provided factory on creation exceptions or inside the returned factory,
		/// false if the provided factory will be disposed elsewhere.
		/// </param>
		/// <returns>
		/// A factory which can be used as a predicate to compare objects of type <see cref="IMatchMapFactory.SourceType"/>
		/// with the provided object <paramref name="destination"/>.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="destination"/> type is not assignable to <see cref="IMatchMapFactory.DestinationType"/>
		/// of the provided factory.
		/// </exception>
		public static IPredicateFactory PredicateDestination(this IMatchMapFactory factory,
			object? destination,
			bool shouldDispose = true) {

			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			try {
				TypeUtils.CheckObjectType(destination, factory.DestinationType, nameof(destination));

				return new DisposablePredicateFactory(
					factory.SourceType, factory.DestinationType,
					source => factory.Invoke(source, destination),
					shouldDispose ? factory : null);
			}
			catch {
				if (shouldDispose)
					factory.Dispose();
				throw;
			}
		}

		/// <inheritdoc cref="Predicate(IMatchMapFactory, object, bool)" path="/summary"/>
		/// <typeparam name="TComparer">
		/// Type of the objects to compare, this can be either <see cref="IMatchMapFactory.SourceType"/>
		/// or <see cref="IMatchMapFactory.DestinationType"/>.
		/// </typeparam>
		/// <param name="comparand">
		/// Object to compare other objects to, may be null. The type of the object must be the opposite of the specified
		/// <typeparamref name="TComparer"/>:
		/// <list type="bullet">
		/// <item>
		/// If <typeparamref name="TComparer"/> is <see cref="IMatchMapFactory.SourceType"/> <paramref name="comparand"/>
		/// must be <see cref="IMatchMapFactory.DestinationType"/>.
		/// </item>
		/// <item>
		/// If <typeparamref name="TComparer"/> is <see cref="IMatchMapFactory.DestinationType"/> <paramref name="comparand"/>
		/// must be <see cref="IMatchMapFactory.SourceType"/>.
		/// </item>
		/// </list>
		/// </param>
		/// <returns>
		/// A factory which can be used as a predicate to compare objects of type <typeparamref name="TComparer"/>
		/// with the provided object <paramref name="comparand"/>.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// <list type="bullet">
		/// <item>
		/// <typeparamref name="TComparer"/> type is not <see cref="IMatchMapFactory.SourceType"/> or
		/// <see cref="IMatchMapFactory.DestinationType"/>.
		/// </item>
		/// <item>
		/// <paramref name="comparand"/> is not assignable to <see cref="IMatchMapFactory.SourceType"/>
		/// (if <typeparamref name="TComparer"/> is <see cref="IMatchMapFactory.DestinationType"/>) or
		/// <see cref="IMatchMapFactory.DestinationType"/> (if <typeparamref name="TComparer"/>
		/// is <see cref="IMatchMapFactory.SourceType"/>).
		/// </item>
		/// </list>
		/// </exception>
		public static PredicateFactory<TComparer> Predicate<TComparer>(this IMatchMapFactory factory,
			object? comparand,
			bool shouldDispose = true) {

			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			try {
				if(typeof(TComparer) == factory.SourceType) { 
					TypeUtils.CheckObjectType(comparand, factory.DestinationType, nameof(comparand));

					return new DisposablePredicateFactory<TComparer>(
						factory.DestinationType,
						source => factory.Invoke(source, comparand),
						shouldDispose ? factory : null);
				}
				else if(typeof(TComparer) == factory.DestinationType) {
					TypeUtils.CheckObjectType(comparand, factory.SourceType, nameof(comparand));

					return new DisposablePredicateFactory<TComparer>(
						factory.SourceType,
						destination => factory.Invoke(comparand, destination),
						shouldDispose ? factory : null);
				}
				else {
					throw new ArgumentException($"The provided comparer type {typeof(TComparer).FullName ?? typeof(TComparer).Name} " +
						$"is not one of {factory.SourceType.FullName ?? factory.SourceType.Name} or " +
						$"{factory.DestinationType.FullName ?? factory.DestinationType.Name} factory types.", nameof(TComparer));
				}
			}
			catch {
				if (shouldDispose)
					factory.Dispose();
				throw;
			}
		}

		/// <inheritdoc cref="Predicate(IMatchMapFactory, object, bool)" path="/summary"/>
		/// <typeparam name="TSource">Type of the object to compare to.</typeparam>
		/// <typeparam name="TDestination">Type of the objects to compare.</typeparam>
		/// <param name="source">Object to compare other objects to, may be null.</param>
		/// <returns>
		/// A factory which can be used as a predicate to compare objects of type <typeparamref name="TDestination"/>
		/// with the provided object <paramref name="source"/>.
		/// </returns>
		public static PredicateFactory<TDestination> Predicate<TSource, TDestination>(this MatchMapFactory<TSource, TDestination> factory,
			TSource? source,
			bool shouldDispose = true) {
			
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			try { 
				return new DisposablePredicateFactory<TDestination>(
					factory.SourceType,
					destination => factory.Invoke(source, destination),
					shouldDispose ? factory : null);
			}
			catch {
				if(shouldDispose)
					factory.Dispose();
				throw;
			}
		}
		
		/// <inheritdoc cref="Predicate(IMatchMapFactory, object, bool)" path="/summary"/>
		/// <typeparam name="TSource">Type of the objects to compare.</typeparam>
		/// <typeparam name="TDestination">Type of the objects to compare to.</typeparam>
		/// <param name="destination">Object to compare other objects to, may be null.</param>
		/// <returns>
		/// A factory which can be used as a predicate to compare objects of type <typeparamref name="TSource"/>
		/// with the provided object <paramref name="destination"/>.
		/// </returns>
		public static PredicateFactory<TSource> PredicateDestination<TSource, TDestination>(this MatchMapFactory<TSource, TDestination> factory,
			TDestination? destination,
			bool shouldDispose = true) {

			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			try { 
				return new DisposablePredicateFactory<TSource>(
					factory.DestinationType,
					source => factory.Invoke(source, destination),
					shouldDispose ? factory : null);
			}
			catch {
				if(shouldDispose)
					factory.Dispose();
				throw;
			}
		}


		/// <inheritdoc cref="PredicateDestination{TSource, TDestination}(MatchMapFactory{TSource, TDestination}, TDestination, bool)"/>
		[Obsolete("This method will be removed in future releases, use PredicateDestination() instead.")]
		public static PredicateFactory<TSource> Predicate<TSource, TDestination>(this MatchMapFactory<TSource, TDestination> factory,
			TDestination? destination,
			bool shouldDispose = true) {

			return factory.PredicateDestination(destination, shouldDispose);
		}
		#endregion
	}
}
