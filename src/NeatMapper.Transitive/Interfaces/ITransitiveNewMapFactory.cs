using System;
using System.Collections.Generic;

namespace NeatMapper.Transitive {
	/// <summary>
	/// A factory which can be used to map objects of a given type into new objects of another type.<br/>
	/// Even if the factory was created successfully it may fail at mapping the given objects (or even types),
	/// so you should catch exceptions thrown by the methods and act accordingly.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface ITransitiveNewMapFactory : INewMapFactory {
		/// <summary>
		/// Types involved in the map chain, the first will be <see cref="INewMapFactory.SourceType"/>
		/// and the last will be <see cref="INewMapFactory.DestinationType"/>.
		/// </summary>
		IList<Type> Types { get; }
	}

	/// <summary>
	/// Typed version of <see cref="ITransitiveNewMapFactory"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <remarks>Implementations of this class must be thread-safe.</remarks>
	public abstract class TransitiveNewMapFactory<TSource, TDestination> : ITransitiveNewMapFactory {
		public abstract Type SourceType { get; }

		public abstract Type DestinationType { get; }

		public abstract IList<Type> Types { get; }


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
			TransitiveNewMapFactory<
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
