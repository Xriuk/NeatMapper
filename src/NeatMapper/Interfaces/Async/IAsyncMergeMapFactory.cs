using System;
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
		/// <returns>
		/// A task which when completed returns the resulting object of the mapping of type
		/// <see cref="DestinationType"/>, which can be the same as <paramref name="destination"/> or a new one,
		/// may be null.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided objects could not be mapped.</exception>
		/// <exception cref="MappingException">An exception was thrown inside the map.</exception>
		Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> Invoke(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			destination);
	}

	/// <summary>
	/// Typed version of <see cref="IAsyncMergeMapFactory"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <remarks>Implementations of this class must be thread-safe.</remarks>
	public abstract class AsyncMergeMapFactory<TSource, TDestination> : IAsyncMergeMapFactory {
		public abstract Type SourceType { get; }

		public abstract Type DestinationType { get; }


		/// <inheritdoc cref="IAsyncMergeMapFactory.Invoke(object, object)" path="/summary"/>
		/// <inheritdoc cref="IAsyncMergeMapFactory.Invoke(object, object)" path="/param[@name='source']"/>
		/// <returns>
		/// A task which when completed returns the resulting object of the mapping of type
		/// <typeparamref name="TDestination"/>, which can be the same as <paramref name="destination"/> or a new one,
		/// may be null.
		/// </returns>
		/// <inheritdoc cref="IAsyncMergeMapFactory.Invoke(object, object)" path="/exception"/>
		public abstract Task<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> Invoke(
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			destination);

		public abstract void Dispose();


		async Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> IAsyncMergeMapFactory.Invoke(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			destination) {

			return await Invoke((TSource)source, (TDestination)destination);
		}
	}
}
