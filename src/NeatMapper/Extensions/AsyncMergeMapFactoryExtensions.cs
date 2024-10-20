using System;

namespace NeatMapper {
	public static class AsyncMergeMapFactoryExtensions {
		#region MapAsyncNewFactory
		/// <summary>
		/// Creates a <see cref="IAsyncNewMapFactory"/> from a <see cref="IAsyncMergeMapFactory"/>
		/// by creating empty destination objects and passing them to the provided factory.
		/// </summary>
		/// <param name="shouldDispose">
		/// True if the method should dispose the provided factory on creation exceptions or inside the returned factory,
		/// false if the provided factory will be disposed elsewhere.
		/// </param>
		/// <returns>
		/// A factory which can be used to map objects of the same types of the provided factory asynchronously.
		/// </returns>
		public static IAsyncNewMapFactory MapAsyncNewFactory(this IAsyncMergeMapFactory factory, bool shouldDispose = true) {
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			try {
				// Try creating a destination and forward to merge map
				if (!ObjectFactory.CanCreate(factory.DestinationType))
					throw new MapNotFoundException((factory.SourceType, factory.DestinationType));

				var destinationFactory = ObjectFactory.CreateFactory(factory.DestinationType);

				return new DisposableAsyncNewMapFactory(
					factory.SourceType, factory.DestinationType,
					(source, cancellationToken) => {
						object destination;
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
		/// <typeparam name="TSource">Type of the objects to map.</typeparam>
		/// <typeparam name="TDestination">Type of the destination objects.</typeparam>
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMergeMapFactory, bool)" path="/param[@name='shouldDispose']"/>
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMergeMapFactory, bool)" path="/returns"/>
		public static AsyncNewMapFactory<TSource, TDestination> MapAsyncNewFactory<TSource, TDestination>(this AsyncMergeMapFactory<TSource, TDestination> factory, bool shouldDispose = true) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			var newFactory = ((IAsyncMergeMapFactory)factory).MapAsyncNewFactory(shouldDispose);
			return new DisposableAsyncNewMapFactory<TSource, TDestination>((source, cancellationToken) => TaskUtils.AwaitTask<TDestination>(newFactory.Invoke(source, cancellationToken)), newFactory);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		} 
		#endregion
	}
}
