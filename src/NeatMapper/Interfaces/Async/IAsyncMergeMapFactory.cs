using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// A factory which can be used to map objects of a given type into existing objects of another type asynchronously.<br/>
	/// Even if the factory was created successfully it may fail at mapping the given objects (or even types),
	/// so you should catch exceptions thrown by the methods and act accordingly.<br/>
	/// Created by <see cref="IAsyncMapperFactory"/>.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IAsyncMergeMapFactory : IDisposable {
		/// <summary>
		/// Type of the object to map.
		/// </summary>
		Type SourceType { get; }

		/// <summary>
		/// Type of the destination object.
		/// </summary>
		Type DestinationType { get; }

		/// <summary>
		/// Maps an object to an existing one and returns the result asynchronously.
		/// </summary>
		/// <param name="source">Object to be mapped, may be null.</param>
		/// <param name="destination">Object to map to, may be null.</param>
		/// <param name="cancellationToken">
		/// Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping.
		/// </param>
		/// <returns>
		/// A task which when completed returns the resulting object of the mapping of type
		/// <see cref="DestinationType"/>, which can be the same as <paramref name="destination"/> or a new one,
		/// may be null.
		/// </returns>
		/// <exception cref="MappingException">An exception was thrown inside the map.</exception>
		Task<object?> Invoke(object? source, object? destination, CancellationToken cancellationToken = default);
	}

	/// <summary>
	/// Typed version of <see cref="IAsyncMergeMapFactory"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <remarks>Implementations of this class must be thread-safe.</remarks>
	public abstract class AsyncMergeMapFactory<TSource, TDestination> : IAsyncMergeMapFactory {
		// DEV: virtual to be backwards compatible, should remove
		public virtual Type SourceType => typeof(TSource);

		// DEV: virtual to be backwards compatible, should remove
		public virtual Type DestinationType => typeof(TDestination);


		/// <inheritdoc cref="IAsyncMergeMapFactory.Invoke(object?, object?, CancellationToken)" path="/summary"/>
		/// <inheritdoc cref="IAsyncMergeMapFactory.Invoke(object?, object?, CancellationToken)" path="/param[@name='source']"/>
		/// <inheritdoc cref="IAsyncMergeMapFactory.Invoke(object?, object?, CancellationToken)" path="/param[@name='cancellationToken']"/>
		/// <returns>
		/// A task which when completed returns the resulting object of the mapping of type
		/// <typeparamref name="TDestination"/>, which can be the same as <paramref name="destination"/> or a new one,
		/// may be null.
		/// </returns>
		/// <inheritdoc cref="IAsyncMergeMapFactory.Invoke(object?, object?, CancellationToken)" path="/exception"/>
		public abstract Task<TDestination?> Invoke(TSource? source, TDestination? destination, CancellationToken cancellationToken = default);

		protected abstract void Dispose(bool disposing);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		async Task<object?> IAsyncMergeMapFactory.Invoke(object? source, object? destination, CancellationToken cancellationToken) {
			TypeUtils.CheckObjectType(source, typeof(TSource), nameof(source));
			TypeUtils.CheckObjectType(destination, typeof(TDestination), nameof(destination));

			return await Invoke((TSource?)source, (TDestination?)destination, cancellationToken);
		}


		public static implicit operator Func<TSource?, TDestination?, CancellationToken, Task<TDestination?>>(
			AsyncMergeMapFactory<TSource?, TDestination?> factory) => factory.Invoke;
	}
}
