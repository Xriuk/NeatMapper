using System;

namespace NeatMapper {
	/// <summary>
	/// A factory which can be used to map objects of a given type into new objects of another type.<br/>
	/// Even if the factory was created successfully it may fail at mapping the given objects (or even types),
	/// so you should catch exceptions thrown by the methods and act accordingly.<br/>
	/// Created by <see cref="IMapperFactory"/>.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface INewMapFactory : IDisposable {
		/// <summary>
		/// Type of the object to map.
		/// </summary>
		Type SourceType { get; }

		/// <summary>
		/// Type of the destination object to create.
		/// </summary>
		Type DestinationType { get; }

		/// <summary>
		/// Maps an object into a new one.
		/// </summary>
		/// <param name="source">Object to map, of type <see cref="SourceType"/>, may be null.</param>
		/// <returns>The newly created object, of type <see cref="DestinationType"/>, may be null.</returns>
		/// <exception cref="MappingException">An exception was thrown inside the map.</exception>
		object? Invoke(object? source);
	}

	/// <summary>
	/// Typed version of <see cref="INewMapFactory"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <remarks>Implementations of this class must be thread-safe.</remarks>
	public abstract class NewMapFactory<TSource, TDestination> : INewMapFactory {
		// DEV: virtual to be backwards compatible, should remove
		public virtual Type SourceType => typeof(TSource);

		// DEV: virtual to be backwards compatible, should remove
		public virtual Type DestinationType => typeof(TDestination);


		/// <inheritdoc cref="INewMapFactory.Invoke(object?)" path="/summary"/>
		/// <inheritdoc cref="INewMapFactory.Invoke(object?)" path="/param[@name='source']"/>
		/// <returns>The newly created object of type <typeparamref name="TDestination"/>, may be null.</returns>
		/// <inheritdoc cref="INewMapFactory.Invoke(object?)" path="/exception"/>
		public abstract TDestination? Invoke(TSource? source);

		protected abstract void Dispose(bool disposing);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		

		object? INewMapFactory.Invoke(object? source) {
			TypeUtils.CheckObjectType(source, typeof(TSource), nameof(source));

			return Invoke((TSource?)source);
		}


		public static implicit operator Func<TSource?, TDestination?>(
			NewMapFactory<TSource?, TDestination?> factory) => factory.Invoke;

		public static implicit operator Converter<TSource?, TDestination?>(
			NewMapFactory<TSource?, TDestination?> factory) => factory.Invoke;
	}
}
