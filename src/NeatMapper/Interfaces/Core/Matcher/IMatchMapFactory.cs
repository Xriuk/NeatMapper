﻿using System;

namespace NeatMapper {
	/// <summary>
	/// A factory which can be used to map objects of a given type into new objects of another type.<br/>
	/// Even if the factory was created successfully it may fail at mapping the given objects (or even types),
	/// so you should catch exceptions thrown by the methods and act accordingly.<br/>
	/// Created by <see cref="IMapperFactory"/>.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IMatchMapFactory : IDisposable {
		/// <summary>
		/// Type of the object to map.
		/// </summary>
		Type SourceType { get; }

		/// <summary>
		/// Type of the destination object to create.
		/// </summary>
		Type DestinationType { get; }

		/// <summary>
		/// Checks if two objects are equivalent (usually by comparing the keys of the two).
		/// </summary>
		/// <param name="source">Source object, may be null.</param>
		/// <param name="destination">Destination object, may be null.</param>
		/// <returns><see langword="true"/> if the two objects match.</returns>
		/// <exception cref="MatcherException">An exception was thrown inside the map.</exception>
		bool Invoke(
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
	/// Typed version of <see cref="IMatchMapFactory"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <remarks>Implementations of this class must be thread-safe.</remarks>
	public abstract class MatchMapFactory<TSource, TDestination> : IMatchMapFactory {
		public abstract Type SourceType { get; }

		public abstract Type DestinationType { get; }


		/// <inheritdoc cref="IMatchMapFactory.Invoke(object, object)"/>
		public abstract bool Invoke(
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


		bool IMatchMapFactory.Invoke(
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
			bool>(
			MatchMapFactory<
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
