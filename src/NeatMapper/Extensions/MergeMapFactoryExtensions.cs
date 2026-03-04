using System;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	public static class MergeMapFactoryExtensions {
		#region MapNewFactory
		/// <summary>
		/// Creates a <see cref="INewMapFactory"/> from a <see cref="IMergeMapFactory"/> by creating
		/// empty destination objects and passing them to the provided factory.
		/// </summary>
		/// <inheritdoc cref="MapNewFactory(IMergeMapFactory, bool, bool)" path="/param[@name='shouldDispose']"/>
		/// <inheritdoc cref="MapNewFactory(IMergeMapFactory, bool, bool)" path="/returns"/>
		/// <exception cref="MapNotFoundException">
		/// The provided types cannot be new mapped because instances of the destination type
		/// could not be created.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static INewMapFactory MapNewFactory(this IMergeMapFactory factory, bool shouldDispose = true) {
			return factory.MapNewFactory(true, shouldDispose);
		}
		/// <summary>
		/// Creates a <see cref="INewMapFactory"/> from a <see cref="IMergeMapFactory"/> by creating
		/// empty or default destination objects and passing them to the provided factory.
		/// </summary>
		/// <param name="shouldCreateDestination">
		/// True if an empty destination instance should be created, false to use the default value.
		/// </param>
		/// <param name="shouldDispose">
		/// True if the method should dispose the provided factory on creation exceptions
		/// or inside the returned factory, false if the provided factory will be disposed elsewhere.
		/// </param>
		/// <returns>
		/// A factory which can be used to map objects of the same types of the provided factory.
		/// </returns>
		/// <exception cref="MapNotFoundException">
		/// The provided types cannot be new mapped because instances of the destination type
		/// could not be created (only if <paramref name="shouldCreateDestination"/> is true).
		/// </exception>
		public static INewMapFactory MapNewFactory(this IMergeMapFactory factory, bool shouldCreateDestination, bool shouldDispose) {
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

				return new DisposableNewMapFactory(
					factory.SourceType, factory.DestinationType,
					source => {
						object? destination;
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
		/// <inheritdoc cref="MapNewFactory{TSource, TDestination}(MergeMapFactory{TSource, TDestination}, bool, bool)" path="/typeparam[@name='TSource']"/>
		/// <inheritdoc cref="MapNewFactory{TSource, TDestination}(MergeMapFactory{TSource, TDestination}, bool, bool)" path="/typeparam[@name='TDestination']"/>
		/// <inheritdoc cref="MapNewFactory(IMergeMapFactory, bool, bool)" path="/param[@name='shouldDispose']"/>
		/// <inheritdoc cref="MapNewFactory(IMergeMapFactory, bool, bool)" path="/returns"/>
		/// <inheritdoc cref="MapNewFactory(IMergeMapFactory, bool)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NewMapFactory<TSource, TDestination> MapNewFactory<TSource, TDestination>(this MergeMapFactory<TSource, TDestination> factory,
			bool shouldDispose = true) {

			return factory.MapNewFactory(true, shouldDispose);
		}

		/// <summary>
		/// Creates a <see cref="NewMapFactory{TSource, TDestination}"/> from a
		/// <see cref="MergeMapFactory{TSource, TDestination}"/> by creating empty or default
		/// destination objects and passing them to the provided factory.
		/// </summary>
		/// <typeparam name="TSource">Type of the objects to map.</typeparam>
		/// <typeparam name="TDestination">Type of the destination objects.</typeparam>
		/// <inheritdoc cref="MapNewFactory(IMergeMapFactory, bool, bool)" path="/param[@name='shouldCreateDestination']"/>
		/// <inheritdoc cref="MapNewFactory(IMergeMapFactory, bool, bool)" path="/param[@name='shouldDispose']"/>
		/// <inheritdoc cref="MapNewFactory(IMergeMapFactory, bool, bool)" path="/returns"/>
		/// <inheritdoc cref="MapNewFactory(IMergeMapFactory, bool, bool)" path="/exception"/>
		public static NewMapFactory<TSource, TDestination> MapNewFactory<TSource, TDestination>(this MergeMapFactory<TSource, TDestination> factory,
			bool shouldCreateDestination, bool shouldDispose) {

			var newFactory = ((IMergeMapFactory)factory).MapNewFactory(shouldCreateDestination, shouldDispose);
			try { 
				return new DisposableNewMapFactory<TSource, TDestination>(
					source => (TDestination?)newFactory.Invoke(source),
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
