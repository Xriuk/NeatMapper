using System;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	public static class AsyncMergeMapFactoryExtensions {
		#region MapAsyncNewFactory
		/// <summary>
		/// Creates a <see cref="IAsyncNewMapFactory"/> from a <see cref="IAsyncMergeMapFactory"/>
		/// by creating empty destination objects and passing them to the provided factory.
		/// </summary>
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMergeMapFactory, bool, bool)" path="/param[@name='shouldDispose']"/>
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMergeMapFactory, bool, bool)" path="/returns"/>
		/// <exception cref="MapNotFoundException">
		/// The provided types cannot be new mapped because instances of the destination type
		/// could not be created.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IAsyncNewMapFactory MapAsyncNewFactory(this IAsyncMergeMapFactory factory, bool shouldDispose = true) {
			return factory.MapAsyncNewFactory(true, shouldDispose);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncNewMapFactory"/> from a <see cref="IAsyncMergeMapFactory"/>
		/// by creating empty or default destination objects and passing them to the provided factory.
		/// </summary>
		/// <param name="shouldCreateDestination">
		/// True if an empty destination instance should be created, false to use the default value.
		/// </param>
		/// <param name="shouldDispose">
		/// True if the method should dispose the provided factory on creation exceptions or inside the returned factory,
		/// false if the provided factory will be disposed elsewhere.
		/// </param>
		/// <returns>
		/// A factory which can be used to map objects of the same types of the provided factory asynchronously.
		/// </returns>
		/// <exception cref="MapNotFoundException">
		/// The provided types cannot be new mapped because instances of the destination type
		/// could not be created (only if <paramref name="shouldCreateDestination"/> is true).
		/// </exception>
		public static IAsyncNewMapFactory MapAsyncNewFactory(this IAsyncMergeMapFactory factory, bool shouldCreateDestination, bool shouldDispose) {
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			try {
				// Try creating a destination and forward to merge map
				Func<object?> destinationFactory;
				if (shouldCreateDestination) {
					try {
						destinationFactory = ObjectFactory.CreateFactory(factory.DestinationType);
					}
					catch (ObjectCreationException) {
						throw new MapNotFoundException((factory.SourceType, factory.DestinationType));
					}
				}
				else
					destinationFactory = factory.DestinationType.GetDefault;

				return new DisposableAsyncNewMapFactory(
					factory.SourceType, factory.DestinationType,
					(source, cancellationToken) => {
						object? destination;
						try {
							destination = destinationFactory.Invoke();
						}
						catch (ObjectCreationException e) {
							throw new MappingException(e, (factory.SourceType, factory.DestinationType));
						}

						return factory.Invoke(source, destination, cancellationToken);
					},
					shouldDispose ? factory : null);
			}
			catch {
				if (shouldDispose)
					factory.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Creates a <see cref="AsyncNewMapFactory{TSource, TDestination}"/> from a
		/// <see cref="AsyncMergeMapFactory{TSource, TDestination}"/> by creating empty destination objects
		/// and passing them to the provided factory.
		/// </summary>
		/// <inheritdoc cref="MapAsyncNewFactory{TSource, TDestination}(AsyncMergeMapFactory{TSource, TDestination}, bool, bool)" path="/typeparam[@name='TSource']"/>
		/// <inheritdoc cref="MapAsyncNewFactory{TSource, TDestination}(AsyncMergeMapFactory{TSource, TDestination}, bool, bool)" path="/typeparam[@name='TDestination']"/>
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMergeMapFactory, bool, bool)" path="/param[@name='shouldDispose']"/>
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMergeMapFactory, bool, bool)" path="/returns"/>
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMergeMapFactory, bool)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AsyncNewMapFactory<TSource, TDestination> MapAsyncNewFactory<TSource, TDestination>(this AsyncMergeMapFactory<TSource, TDestination> factory,
			bool shouldDispose = true) {

			return factory.MapAsyncNewFactory(true, shouldDispose);
		}

		/// <summary>
		/// Creates a <see cref="AsyncNewMapFactory{TSource, TDestination}"/> from a
		/// <see cref="AsyncMergeMapFactory{TSource, TDestination}"/> by creating empty or default
		/// destination objects and passing them to the provided factory.
		/// </summary>
		/// <typeparam name="TSource">Type of the objects to map.</typeparam>
		/// <typeparam name="TDestination">Type of the destination objects.</typeparam>
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMergeMapFactory, bool, bool)" path="/param[@name='shouldCreateDestination']"/>
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMergeMapFactory, bool, bool)" path="/param[@name='shouldDispose']"/>
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMergeMapFactory, bool, bool)" path="/returns"/>
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMergeMapFactory, bool, bool)" path="/exception"/>
		public static AsyncNewMapFactory<TSource, TDestination> MapAsyncNewFactory<TSource, TDestination>(this AsyncMergeMapFactory<TSource, TDestination> factory,
			bool shouldCreateDestination, bool shouldDispose) {

			var newFactory = ((IAsyncMergeMapFactory)factory).MapAsyncNewFactory(shouldCreateDestination, shouldDispose);
			try {
				return new DisposableAsyncNewMapFactory<TSource, TDestination>(
					(source, cancellationToken) => TaskUtils.AwaitTask<TDestination>(newFactory.Invoke(source, cancellationToken)),
					newFactory);
			}
			catch {
				newFactory.Dispose();
				throw;
			}
		} 
		#endregion
	}
}
