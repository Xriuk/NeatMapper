using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches by invoking an <see cref="IEqualityComparer{T}"/>.
	/// </summary>
	public sealed class EqualityComparerMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// Creates a new instance of <see cref="EqualityComparerMatcher"/> by using the provided
		/// <see cref="IEqualityComparer{T}"/>.
		/// </summary>
		/// <typeparam name="TElement">Type of the source and destination objects.</typeparam>
		/// <param name="equalityComparer">Equality comparer to use for matching.</param>
		/// <returns>A new instance of <see cref="EqualityComparerMatcher"/> for the given equality comparer.</returns>
		public static EqualityComparerMatcher Create<TElement>(IEqualityComparer<TElement?> equalityComparer) {
			return new EqualityComparerMatcher(typeof(TElement), (source, destination) => {
				TypeUtils.CheckObjectType(source, typeof(TElement), nameof(source));
				TypeUtils.CheckObjectType(destination, typeof(TElement), nameof(destination));

				try {
					return equalityComparer.Equals((TElement?)source, (TElement?)destination);
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception e) {
					throw new MatcherException(e, (typeof(TElement), typeof(TElement)));
				}
			});
		}


		/// <summary>
		/// Type which can be matched
		/// </summary>
		private readonly Type _comparerType;

		/// <summary>
		/// Delegate to use for matching.
		/// </summary>
		private readonly Func<object?, object?, bool> _equalityComparer;


		// Private constructor because requires type-safe type and delegate.
		private EqualityComparerMatcher(Type comparerType, Func<object?, object?, bool> equalityComparer) {
			_comparerType = comparerType ?? throw new ArgumentNullException(nameof(comparerType));
			_equalityComparer = equalityComparer ?? throw new ArgumentNullException(nameof(equalityComparer));
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return (sourceType == _comparerType && destinationType == _comparerType);
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatch(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			return _equalityComparer.Invoke(source, destination);
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatch(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultMatchMapFactory(sourceType, destinationType, _equalityComparer);
		}
	}
}
