using System;

namespace NeatMapper {
	public static class MergeMapFactoryExtensions {
		#region MapNewFactory
		/// <summary>
		/// Creates a <see cref="INewMapFactory"/> from a <see cref="IMergeMapFactory"/> by creating empty destination
		/// objects and passing them to the provided factory.
		/// </summary>
		/// <param name="shouldDispose">
		/// True if the method should dispose the provided factory on creation exceptions or inside the returned factory,
		/// false if the provided factory will be disposed elsewhere.
		/// </param>
		/// <returns>
		/// A factory which can be used to map objects of the same types of the provided factory.
		/// </returns>
		/// <exception cref="MapNotFoundException">
		/// The provided types cannot be new mapped because instances of the destination type could not be created.
		/// </exception>
		public static INewMapFactory MapNewFactory(this IMergeMapFactory factory, bool shouldDispose = true) {
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			try {
				// Try creating a destination and forward to merge map
				Func<object> destinationFactory;
				try {
					destinationFactory = ObjectFactory.CreateFactory(factory.DestinationType);
				}
				catch (ObjectCreationException) {
					throw new MapNotFoundException((factory.SourceType, factory.DestinationType));
				}

				return new DisposableNewMapFactory(
					factory.SourceType, factory.DestinationType,
					source => {
						object destination;
						try {
							destination = destinationFactory.Invoke();
						}
						catch (ObjectCreationException e) {
							throw new MappingException(e, (factory.SourceType, factory.DestinationType));
						}

						return factory.Invoke(source, destination);
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
		/// Creates a <see cref="NewMapFactory{TSource, TDestination}"/> from a
		/// <see cref="MergeMapFactory{TSource, TDestination}"/> by creating empty destination objects
		/// and passing them to the provided factory.
		/// </summary>
		/// <typeparam name="TSource">Type of the objects to map.</typeparam>
		/// <typeparam name="TDestination">Type of the destination objects.</typeparam>
		/// <inheritdoc cref="MapNewFactory(IMergeMapFactory, bool)" path="/param"/>
		/// <inheritdoc cref="MapNewFactory(IMergeMapFactory, bool)" path="/returns"/>
		/// <inheritdoc cref="MapNewFactory(IMergeMapFactory, bool)" path="/exception"/>
		public static NewMapFactory<TSource, TDestination> MapNewFactory<TSource, TDestination>(this MergeMapFactory<TSource, TDestination> factory, bool shouldDispose = true) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			var newFactory = ((IMergeMapFactory)factory).MapNewFactory(shouldDispose);
			return new DisposableNewMapFactory<TSource, TDestination>(source => (TDestination)newFactory.Invoke(source), newFactory);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		} 
		#endregion
	}
}
