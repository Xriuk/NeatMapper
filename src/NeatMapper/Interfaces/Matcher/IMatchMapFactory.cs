using System;

namespace NeatMapper {
	/// <summary>
	/// A factory which can be used to match objects of a given type with new objects of another type.<br/>
	/// Even if the factory was created successfully it may fail at matching the given objects (or even types),
	/// so you should catch exceptions thrown by the methods and act accordingly.<br/>
	/// Created by <see cref="IMatcherFactory"/>.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IMatchMapFactory : IDisposable {
		/// <summary>
		/// Type of the source object to compare.
		/// </summary>
		Type SourceType { get; }

		/// <summary>
		/// Type of the destination object to be compared to.
		/// </summary>
		Type DestinationType { get; }

		/// <summary>
		/// Checks if two objects are equivalent (usually by comparing the keys of the two).
		/// </summary>
		/// <param name="source">Source object, of type <see cref="SourceType"/>, may be null.</param>
		/// <param name="destination">
		/// Destination object, of type <see cref="DestinationType"/>, may be null.
		/// </param>
		/// <returns><see langword="true"/> if the two objects match.</returns>
		/// <exception cref="MatcherException">An exception was thrown inside the map.</exception>
		bool Invoke(object? source, object? destination);
	}

	/// <summary>
	/// Typed version of <see cref="IMatchMapFactory"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <remarks>Implementations of this class must be thread-safe.</remarks>
	public abstract class MatchMapFactory<TSource, TDestination> : IMatchMapFactory {
		public Type SourceType => typeof(TSource);

		public Type DestinationType => typeof(TDestination);


		/// <inheritdoc cref="IMatchMapFactory.Invoke(object?, object?)"/>
		public abstract bool Invoke(TSource? source, TDestination? destination);

		protected abstract void Dispose(bool disposing);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		bool IMatchMapFactory.Invoke(object? source, object? destination) {
			TypeUtils.CheckObjectType(source, typeof(TSource), nameof(source));
			TypeUtils.CheckObjectType(destination, typeof(TDestination), nameof(destination));

			return Invoke((TSource?)source, (TDestination?)destination);
		}


		public static implicit operator Func<TSource?, TDestination?, bool>(
			MatchMapFactory<TSource?, TDestination?> factory) => factory.Invoke;
	}
}
