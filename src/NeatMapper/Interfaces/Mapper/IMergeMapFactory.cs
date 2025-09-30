using System;

namespace NeatMapper {
	/// <summary>
	/// A factory which can be used to map objects of a given type into existing objects of another type.<br/>
	/// Even if the factory was created successfully it may fail at mapping the given objects (or even types),
	/// so you should catch exceptions thrown by the methods and act accordingly.<br/>
	/// Created by <see cref="IMapperFactory"/>.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IMergeMapFactory : IDisposable {
		/// <summary>
		/// Type of the object to map.
		/// </summary>
		Type SourceType { get; }

		/// <summary>
		/// Type of the destination object.
		/// </summary>
		Type DestinationType { get; }

		/// <summary>
		/// Maps an object to an existing one and returns the result.
		/// </summary>
		/// <param name="source">Object to be mapped, of type <see cref="SourceType"/>, may be null.</param>
		/// <param name="destination">Object to map to, of type <see cref="DestinationType"/>, may be null.</param>
		/// <returns>
		/// The resulting object of the mapping of type <see cref="DestinationType"/>, can be the same as
		/// <paramref name="destination"/> or a new one, may be null.
		/// </returns>
		/// <exception cref="MappingException">
		/// An exception was thrown inside the map, check the inner exception for details.
		/// </exception>
		object? Invoke(object? source, object? destination);
	}

	/// <summary>
	/// Typed version of <see cref="IMergeMapFactory"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <remarks>Implementations of this class must be thread-safe.</remarks>
	public abstract class MergeMapFactory<TSource, TDestination> : IMergeMapFactory {
		public Type SourceType => typeof(TSource);

		public Type DestinationType => typeof(TDestination);


		/// <inheritdoc cref="IMergeMapFactory.Invoke(object?, object?)" path="/summary"/>
		/// <inheritdoc cref="IMergeMapFactory.Invoke(object?, object?)" path="/param[@name='source']"/>
		/// <returns>
		/// The resulting object of the mapping of type <typeparamref name="TDestination"/>, can be the same as
		/// <paramref name="destination"/> or a new one, may be null.
		/// </returns>
		/// <inheritdoc cref="IMergeMapFactory.Invoke(object?, object?)" path="/exception"/>
		public abstract TDestination? Invoke(TSource? source, TDestination? destination);

		protected abstract void Dispose(bool disposing);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		object? IMergeMapFactory.Invoke(object? source, object? destination) {
			TypeUtils.CheckObjectType(source, typeof(TSource), nameof(source));
			TypeUtils.CheckObjectType(destination, typeof(TDestination), nameof(destination));

			return Invoke((TSource?)source, (TDestination?)destination);
		}


		public static implicit operator Func<TSource?, TDestination?, TDestination?>(
			MergeMapFactory<TSource?, TDestination?> factory) => factory.Invoke;
	}
}
