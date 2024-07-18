using System;
using System.Collections.Generic;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches by invoking a <see cref="IEqualityComparer{T}"/>.
	/// </summary>
	public sealed class EqualityComparerMatcher : IMatcher, IMatcherCanMatch, IMatcherFactory {
		/// <summary>
		/// Creates an instance of <see cref="EqualityComparerMatcher"/> by using the provided
		/// <see cref="IEqualityComparer{T}"/>.
		/// </summary>
		/// <typeparam name="TElement">Type of the source and destination objects.</typeparam>
		/// <param name="equalityComparer">Equality comparer to use for matching.</param>
		/// <returns>A new instance of <see cref="EqualityComparerMatcher"/> for the given equality comparer.</returns>
		public static EqualityComparerMatcher Create<TElement>(IEqualityComparer<TElement> equalityComparer) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return new EqualityComparerMatcher(typeof(TElement), (source, destination) => {
				TypeUtils.CheckObjectType(source, typeof(TElement), nameof(source));
				TypeUtils.CheckObjectType(destination, typeof(TElement), nameof(destination));

				try {
					return equalityComparer.Equals((TElement)source, (TElement)destination);
				}
				catch (MapNotFoundException e) {
					if (e.From == typeof(TElement) && e.To == typeof(TElement))
						throw;
					else
						throw new MappingException(e, (typeof(TElement), typeof(TElement)));
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception e) {
					throw new MatcherException(e, (typeof(TElement), typeof(TElement)));
				}
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}


		/// <summary>
		/// Type which can be matched
		/// </summary>
		private readonly Type _comparerType;

		/// <summary>
		/// Delegate to use for matching.
		/// </summary>
		private readonly Func<object, object, bool> _equalityComparer;


		// Private constructor because requires type-safe type and delegate.
		private EqualityComparerMatcher(Type comparerType, Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?, object?
#else
			object, object
#endif
			, bool> equalityComparer) {

			_comparerType = comparerType ?? throw new ArgumentNullException(nameof(comparerType));
			_equalityComparer = equalityComparer ?? throw new ArgumentNullException(nameof(equalityComparer));
		}


		public bool Match(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			destination,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (!CanMatch(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return _equalityComparer.Invoke(source, destination);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public bool CanMatch(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return sourceType == _comparerType && destinationType == _comparerType;
		}

		public IMatchMapFactory MatchFactory(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (!CanMatch(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultMatchMapFactory(sourceType, destinationType, _equalityComparer);
		}
	}
}
