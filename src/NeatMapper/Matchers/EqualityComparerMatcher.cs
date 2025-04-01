using System;
using System.Collections.Generic;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches by invoking an <see cref="IEqualityComparer{T}"/>.
	/// </summary>
	public sealed class EqualityComparerMatcher : IMatcher, IMatcherFactory {
		/// <inheritdoc cref="Create{TElement}(IEqualityComparer{TElement}, NullableTypesMatchingOptions?)"/>
		public static EqualityComparerMatcher Create<TElement>(IEqualityComparer<TElement?> equalityComparer) {
			return CreateInternal(equalityComparer, null);
		}
		/// <summary>
		/// Creates a new instance of <see cref="EqualityComparerMatcher"/> by using the provided
		/// <see cref="IEqualityComparer{T}"/>.
		/// </summary>
		/// <typeparam name="TElement">Type of the source and destination objects.</typeparam>
		/// <param name="equalityComparer">Equality comparer to use for matching.</param>
		/// <param name="nullableTypesOptions">
		/// Additional options to handle <see cref="Nullable{T}"/> types automatic matching, null to use default.<br/>
		/// Can be overridden during matching with <see cref="NullableTypesMatchingMappingOptions"/>.
		/// </param>
		/// <returns>A new instance of <see cref="EqualityComparerMatcher"/> for the given equality comparer.</returns>
		public static EqualityComparerMatcher Create<TElement>(
			IEqualityComparer<TElement> equalityComparer,
			NullableTypesMatchingOptions? nullableTypesOptions) where TElement : struct {

			return CreateInternal(equalityComparer, nullableTypesOptions);
		}
		private static EqualityComparerMatcher CreateInternal<TElement>(
			IEqualityComparer<TElement?> equalityComparer,
			NullableTypesMatchingOptions? nullableTypesOptions) {

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
			}, nullableTypesOptions);
		}


		/// <summary>
		/// Type which can be matched
		/// </summary>
		private readonly Type _comparerType;

		/// <summary>
		/// Delegate to use for matching.
		/// </summary>
		private readonly Func<object?, object?, bool> _equalityComparer;

		/// <summary>
		/// Options to handle <see cref="Nullable{T}"/> types automatic matching.
		/// </summary>
		private readonly NullableTypesMatchingOptions _nullableTypesOptions;


		// Private constructor because requires type-safe type and delegate.
		private EqualityComparerMatcher(
			Type comparerType,
			Func<object?, object?, bool> equalityComparer,
			NullableTypesMatchingOptions? nullableTypesOptions) {

			_comparerType = comparerType ?? throw new ArgumentNullException(nameof(comparerType));
			_equalityComparer = equalityComparer ?? throw new ArgumentNullException(nameof(equalityComparer));
			_nullableTypesOptions = nullableTypesOptions ?? new NullableTypesMatchingOptions();
		}


		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMatchInternal(sourceType, destinationType, mappingOptions, out _);
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatchInternal(sourceType, destinationType, mappingOptions, out var typesNullable))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			// If we are handling Nullable<T> the types will be different
			if (typesNullable.From || typesNullable.To) {
				if (source == null && destination == null)
					return true;

				if (source == null || destination == null)
					return false;

				if (typesNullable.From)
					source = Convert.ChangeType(source, _comparerType);
				if (typesNullable.To)
					destination = Convert.ChangeType(destination, _comparerType);
			}

			return _equalityComparer.Invoke(source, destination);
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatchInternal(sourceType, destinationType, mappingOptions, out var typesNullable))
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultMatchMapFactory(sourceType, destinationType, (source, destination) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));
				TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

				// If we are handling Nullable<T> the types will be different
				if (typesNullable.From || typesNullable.To) {
					if (source == null && destination == null)
						return true;

					if (source == null || destination == null)
						return false;

					if (typesNullable.From)
						source = Convert.ChangeType(source, _comparerType);
					if (typesNullable.To)
						destination = Convert.ChangeType(destination, _comparerType);
				}

				return _equalityComparer.Invoke(source, destination);
			});
		}


		private bool CanMatchInternal(
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions,
			out (bool From, bool To) typesNullable) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			typesNullable = (false, false);

			if (sourceType == _comparerType && destinationType == _comparerType)
				return true;

			if (mappingOptions?.GetOptions<NullableTypesMatchingMappingOptions>()?.SupportNullableTypes ?? _nullableTypesOptions.SupportNullableTypes) {
				if (sourceType.IsNullable()) { 
					sourceType = Nullable.GetUnderlyingType(sourceType)!;
					typesNullable.From = true;
					if (sourceType == _comparerType && destinationType == _comparerType)
						return true;
				}

				if (destinationType.IsNullable()) { 
					destinationType = Nullable.GetUnderlyingType(destinationType)!;
					typesNullable.To = true;
					return sourceType == _comparerType && destinationType == _comparerType;
				}
			}

			return false;
		}
	}
}
