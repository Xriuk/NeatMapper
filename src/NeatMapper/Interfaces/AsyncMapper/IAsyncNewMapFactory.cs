using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// A factory which can be used to map objects of a given type into new objects of another type asynchronously.<br/>
	/// Even if the factory was created successfully it may fail at mapping the given objects (or even types),
	/// so you should catch exceptions thrown by the methods and act accordingly.<br/>
	/// Created by <see cref="IAsyncMapperFactory"/>.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IAsyncNewMapFactory : IDisposable {
		/// <summary>
		/// Type of the object to map.
		/// </summary>
		Type SourceType { get; }

		/// <summary>
		/// Type of the destination object to create.
		/// </summary>
		Type DestinationType { get; }

		/// <summary>
		/// Maps an object into a new one asynchronously.
		/// </summary>
		/// <param name="source">Object to map, may be null.</param>
		/// <param name="cancellationToken">
		/// Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping.
		/// </param>
		/// <returns>
		/// A task which when completed returns the newly created object of type <see cref="DestinationType"/>,
		/// which may be null.
		/// </returns>
		/// <exception cref="MappingException">An exception was thrown inside the map.</exception>
		Task<object?> Invoke(object? source, CancellationToken cancellationToken = default);
	}

	/// <summary>
	/// Typed version of <see cref="IAsyncNewMapFactory"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <remarks>Implementations of this class must be thread-safe.</remarks>
	public abstract class AsyncNewMapFactory<TSource, TDestination> : IAsyncNewMapFactory {
		public Type SourceType => typeof(TSource);

		public Type DestinationType => typeof(TDestination);


		/// <inheritdoc cref="IAsyncNewMapFactory.Invoke(object?, CancellationToken)" path="/summary"/>
		/// <inheritdoc cref="IAsyncNewMapFactory.Invoke(object?, CancellationToken)" path="/param[@name='source']"/>
		/// <inheritdoc cref="IAsyncNewMapFactory.Invoke(object?, CancellationToken)" path="/param[@name='cancellationToken']"/>
		/// <returns>
		/// A task which when completed returns the newly created object of type <typeparamref name="TDestination"/>,
		/// which may be null.
		/// </returns>
		/// <inheritdoc cref="IAsyncNewMapFactory.Invoke(object?, CancellationToken)" path="/exception"/>
		public abstract Task<TDestination?> Invoke(TSource? source, CancellationToken cancellationToken = default);

		protected abstract void Dispose(bool disposing);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		async Task<object?> IAsyncNewMapFactory.Invoke(object? source, CancellationToken cancellationToken) {
			TypeUtils.CheckObjectType(source, typeof(TSource), nameof(source));

			return await Invoke((TSource?)source, cancellationToken);
		}


		public static implicit operator Func<TSource?, CancellationToken, Task<TDestination?>>(
			AsyncNewMapFactory<TSource?, TDestination?> factory) => factory.Invoke;
	}
}
