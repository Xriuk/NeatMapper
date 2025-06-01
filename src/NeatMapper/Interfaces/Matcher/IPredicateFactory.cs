using System;

namespace NeatMapper {
	/// <summary>
	/// A factory which can be used as a predicate to compare objects of type <see cref="ComparerType"/>
	/// with the provided object of type <see cref="ComparandType"/>.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IPredicateFactory : IDisposable {
		/// <summary>
		/// Object to compare to, of type <see cref="ComparandType"/>, may be null.
		/// </summary>
		object? Comparand { get; }

		/// <summary>
		/// Type of the object to compare to.
		/// </summary>
		Type ComparandType { get; }

		/// <summary>
		/// Type of the objects to compare.
		/// </summary>
		Type ComparerType { get; }

		/// <summary>
		/// Compares the given object with another fixed object.
		/// </summary>
		/// <param name="comparer">Object to compare, of type <see cref="ComparerType"/>, may be null.</param>
		/// <returns>
		/// True if the provided object matches with the fixed object of type <see cref="ComparandType"/>,
		/// false otherwise.
		/// </returns>
		/// <exception cref="MatcherException">An exception was thrown inside the map.</exception>
		bool Invoke(object? comparer);
	}

	/// <summary>
	/// Partially typed version of <see cref="IPredicateFactory"/>.
	/// </summary>
	/// <typeparam name="TComparer">Type of the objects to compare.</typeparam>
	/// <remarks>Implementations of this class must be thread-safe.</remarks>
	public abstract class PredicateFactory<TComparer> : IPredicateFactory {
		public abstract object? Comparand { get; }

		public abstract Type ComparandType { get; }

		public Type ComparerType => typeof(TComparer);


		/// <summary>
		/// Compares the given object with another fixed object.
		/// </summary>
		/// <param name="comparer">Object to compare, may be null.</param>
		/// <returns>
		/// True if the provided object matches with the fixed object of type <see cref="ComparandType"/>,
		/// false otherwise.
		/// </returns>
		/// <exception cref="MatcherException">An exception was thrown inside the map.</exception>
		public abstract bool Invoke(TComparer? comparer);

		protected abstract void Dispose(bool disposing);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		bool IPredicateFactory.Invoke(object? comparer) {
			TypeUtils.CheckObjectType(comparer, typeof(TComparer), nameof(comparer));

			return Invoke((TComparer?)comparer);
		}


		public static implicit operator Func<TComparer?, bool>(
			PredicateFactory<TComparer> factory) => factory.Invoke;

		public static implicit operator Predicate<TComparer?>(
			PredicateFactory<TComparer> factory) => factory.Invoke;
	}

	/// <summary>
	/// Full typed version of <see cref="PredicateFactory{TComparer}"/>.
	/// </summary>
	/// <typeparam name="TComparand">Type of the object to compare to.</typeparam>
	/// <typeparam name="TComparer">Type of the objects to compare.</typeparam>
	/// <remarks>Implementations of this class must be thread-safe.</remarks>
	public abstract class PredicateFactory<TComparand, TComparer> : PredicateFactory<TComparer> {
		public override sealed Type ComparandType => typeof(TComparand);

		/// <summary>
		/// Object to compare to, may be null.
		/// </summary>
		public TComparand? ComparandObject => (TComparand?)Comparand;
	}
}
