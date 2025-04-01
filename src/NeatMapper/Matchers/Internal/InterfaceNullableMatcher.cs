using System;

namespace NeatMapper {
	/// <summary>
	/// Common matcher for <see cref="EquatableMatcher"> and EqualityOperatorsMatcher. Internal matcher.
	/// </summary>
	public abstract class InterfaceNullableMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// Options to handle <see cref="Nullable{T}"/> types automatic matching.
		/// </summary>
		private readonly NullableTypesMatchingOptions _nullableTypesOptions;


		internal InterfaceNullableMatcher(NullableTypesMatchingOptions? nullableTypesOptions) {
			_nullableTypesOptions = nullableTypesOptions ?? new NullableTypesMatchingOptions();
		}


		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMatchInternal(sourceType, destinationType, mappingOptions, out _);
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatchInternal(sourceType, destinationType, mappingOptions, out var types))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			// If we are handling Nullable<T> the types will be different
			if (sourceType != types.From || destinationType != types.To) {
				if (source == null && destination == null)
					return true;

				if (source == null || destination == null)
					return false;

				if (sourceType != types.From)
					source = Convert.ChangeType(source, types.From);
				if (destinationType != types.To)
					destination = Convert.ChangeType(destination, types.To);
			}

			var comparer = GetOrCreateDelegate(types.From, types.To);

			try { 
				return comparer.Invoke(source, destination);
			}
			catch (OperationCanceledException) {
				throw;
			}
			catch (Exception e) {
				throw new MatcherException(e, (sourceType, destinationType));
			}
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatchInternal(sourceType, destinationType, mappingOptions, out var types))
				throw new MapNotFoundException((sourceType, destinationType));

			var comparer = GetOrCreateDelegate(types.From, types.To);

			return new DefaultMatchMapFactory(sourceType, destinationType,
				(source, destination) => {
					TypeUtils.CheckObjectType(source, sourceType, nameof(source));
					TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

					// If we are handling Nullable<T> the types will be different
					if (sourceType != types.From || destinationType != types.To) {
						if (source == null && destination == null)
							return true;

						if (source == null || destination == null)
							return false;

						if (sourceType != types.From)
							source = Convert.ChangeType(source, types.From);
						if (destinationType != types.To)
							destination = Convert.ChangeType(destination, types.To);
					}

					try {
						return comparer.Invoke(source, destination);
					}
					catch (OperationCanceledException) {
						throw;
					}
					catch (Exception e) {
						throw new MatcherException(e, (sourceType, destinationType));
					}
				});
		}


		protected abstract bool CanMatchTypes((Type From, Type To) types);

		protected abstract Func<object?, object?, bool> GetOrCreateDelegate(Type sourceType, Type destinationType);

		private bool CanMatchInternal(
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions,
			out (Type From, Type To) types) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			types = (sourceType, destinationType);

			if (CanMatchTypes(types))
				return true;

			if (mappingOptions?.GetOptions<NullableTypesMatchingMappingOptions>()?.SupportNullableTypes ?? _nullableTypesOptions.SupportNullableTypes) {
				if (types.From.IsNullable()) {
					types.From = Nullable.GetUnderlyingType(types.From)!;
					if (CanMatchTypes(types))
						return true;
				}

				if (types.To.IsNullable()) {
					types.To = Nullable.GetUnderlyingType(types.To)!;
					return CanMatchTypes(types);
				}
			}

			return false;
		}
	}
}
