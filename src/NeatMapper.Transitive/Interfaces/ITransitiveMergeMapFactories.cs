using System;
using System.Collections.Generic;

namespace NeatMapper.Transitive {
	/// <summary>
	/// A collection of factories which can be used to map objects of a given type into existing objects of another type.<br/>
	/// Even if the factories were created successfully they may fail at mapping the given objects (or even types),
	/// so you should catch exceptions thrown by the methods and act accordingly.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface ITransitiveMergeMapFactories : IMergeMapFactory {
		/// <summary>
		/// Collection of factories which can map the given types.
		/// </summary>
		IEnumerable<ITransitiveMergeMapFactory> Factories { get; }
	}

	/// <summary>
	/// Typed version of <see cref="ITransitiveMergeMapFactory"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <remarks>Implementations of this class must be thread-safe.</remarks>
	public abstract class TransitiveMergeMapFactories<TSource, TDestination> : ITransitiveMergeMapFactories {
		public abstract Type SourceType { get; }

		public abstract Type DestinationType { get; }

		public abstract IEnumerable<TransitiveMergeMapFactory<TSource, TDestination>> Factories { get; }


		/// <inheritdoc cref="IMergeMapFactory.Invoke(object, object)" path="/summary"/>
		/// <inheritdoc cref="IMergeMapFactory.Invoke(object, object)" path="/param[@name='source']"/>
		/// <returns>The newly created object of type <typeparamref name="TDestination"/>, may be null.</returns>
		/// <inheritdoc cref="IMergeMapFactory.Invoke(object, object)" path="/exception"/>
		public virtual
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
			destination) {

			// Try using the factories, if any
			foreach (var factory in Factories) {
				try {
					return factory.Invoke(source, destination);
				}
				catch (MapNotFoundException) { }
			}

			MapNotFoundException.Throw<TSource, TDestination>();
			throw new InvalidOperationException();
		}

		protected abstract void Dispose(bool disposing);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		IEnumerable<ITransitiveMergeMapFactory> ITransitiveMergeMapFactories.Factories => Factories;

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
			TransitiveMergeMapFactories<
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
