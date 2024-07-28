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
		/// <exception cref="MapNotFoundException">The provided object could not be mapped.</exception>
		/// <exception cref="MappingException">An exception was thrown inside the map.</exception>
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
		object?
#else
		object
#endif
			Invoke(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source);
	}

	/// <summary>
	/// Typed version of <see cref="INewMapFactory"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <remarks>Implementations of this class must be thread-safe.</remarks>
	public abstract class NewMapFactory<TSource, TDestination> : INewMapFactory {
		public abstract Type SourceType { get; }

		public abstract Type DestinationType { get; }


		/// <inheritdoc cref="INewMapFactory.Invoke(object)" path="/summary"/>
		/// <inheritdoc cref="INewMapFactory.Invoke(object)" path="/param[@name='source']"/>
		/// <returns>The newly created object of type <typeparamref name="TDestination"/>, may be null.</returns>
		/// <inheritdoc cref="INewMapFactory.Invoke(object)" path="/exception"/>
		public abstract
#if NET5_0_OR_GREATER
		TDestination?
#else
		TDestination
#endif
			Invoke(
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source);

		protected abstract void Dispose(bool disposing);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
		object?
#else
		object
#endif
			INewMapFactory.Invoke(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return Invoke((TSource)source);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}


		public static implicit operator Func<
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			,
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			>(
			NewMapFactory<
#if NET5_0_OR_GREATER
				TSource?
#else
				TSource
#endif
				,
#if NET5_0_OR_GREATER
				TDestination?
#else
				TDestination
#endif
				> factory) => factory.Invoke;
	}
}
