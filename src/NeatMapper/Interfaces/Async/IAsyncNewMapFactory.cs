using System;
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
		/// <returns>
		/// A task which when completed returns the newly created object of type <see cref="DestinationType"/>,
		/// which may be null.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided object could not be mapped.</exception>
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
			source);
	}

	/// <summary>
	/// Typed version of <see cref="IAsyncNewMapFactory"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IAsyncNewMapFactory<TSource, TDestination> : IAsyncNewMapFactory {
		/// <inheritdoc cref="IAsyncNewMapFactory.Invoke(object)" path="/summary"/>
		/// <inheritdoc cref="IAsyncNewMapFactory.Invoke(object)" path="/param[@name='source']"/>
		/// <returns>
		/// A task which when completed returns the newly created object of type <typeparamref name="TDestination"/>,
		/// which may be null.
		/// </returns>
		/// <inheritdoc cref="IAsyncNewMapFactory.Invoke(object)" path="/exception"/>
		Task<
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
			source);
	}
}
