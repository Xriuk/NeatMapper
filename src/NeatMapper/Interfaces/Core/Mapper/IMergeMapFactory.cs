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
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			destination);
	}

	/// <summary>
	/// Typed version of <see cref="IMergeMapFactory"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <remarks>Implementations of this class must be thread-safe.</remarks>
	public abstract class MergeMapFactory<TSource, TDestination> : IMergeMapFactory {
		public abstract Type SourceType { get; }

		public abstract Type DestinationType { get; }


		/// <inheritdoc cref="IMergeMapFactory.Invoke(object, object)" path="/summary"/>
		/// <inheritdoc cref="IMergeMapFactory.Invoke(object, object)" path="/param[@name='source']"/>
		/// <returns>
		/// The resulting object of the mapping of type <typeparamref name="TDestination"/>, can be the same as
		/// <paramref name="destination"/> or a new one, may be null.
		/// </returns>
		/// <inheritdoc cref="IMergeMapFactory.Invoke(object, object)" path="/exception"/>
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
			source,
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			destination);

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
			IMergeMapFactory.Invoke(
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return Invoke((TSource)source, (TDestination)destination);

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
			,
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			>(
			MergeMapFactory<
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
