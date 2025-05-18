using System;
using System.Diagnostics.CodeAnalysis;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches <see cref="Nullable{T}"/> and its underlying types
	/// by using another <see cref="IMatcher"/>.
	/// <list type="bullet">
	/// <item>
	/// <b><see cref="Nullable{T}"/> == Type</b> or <b>Type == <see cref="Nullable{T}"/></b><br/>
	/// <list type="bullet">
	/// <item>
	/// If <see cref="Nullable{T}.Value"/> is <see langword="null"/> and the Type is a reference type (nullable)
	/// and its value is <see langword="null"/> <see langword="true"/> is returned.
	/// </item>
	/// <item>
	/// If <see cref="Nullable{T}.Value"/> is <see langword="null"/> and the Type is not a reference type
	/// <see langword="false"/> is returned.
	/// </item>
	/// <item>
	/// If <see cref="Nullable{T}.Value"/> is not <see langword="null"/> the underlying type is matched.
	/// </item>
	/// </list>
	/// </item>
	/// <item>
	/// <b><see cref="Nullable{T}"/> == <see cref="Nullable{T}"/></b><br/>
	/// If <see cref="Nullable{T}.Value"/> are both <see langword="null"/> returns <see langword="true"/>,
	/// otherwise tries to match the underlying types in this order:
	/// <list type="number">
	/// <item><see cref="Nullable{T}"/> == Type (if the right type is not <see langword="null"/>).</item>
	/// <item>Type == <see cref="Nullable{T}"/> (if the left type is not <see langword="null"/>).</item>
	/// <item>Type1 == Type2 (if both types are not <see langword="null"/>).</item>
	/// </list>
	/// </item>
	/// </list>
	/// </summary>
	public sealed class NullableMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// <see cref="IMatcher"/> which is used to match concrete types of <see cref="Nullable{T}"/>,
		/// will be also provided together with the matcher itself as a nested matcher in
		/// <see cref="MatcherOverrideMappingOptions"/> (if not already present).
		/// </summary>
		private readonly IMatcher _concreteMatcher;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;

		/// <summary>
		/// Creates a new instance of <see cref="NullableMatcher"/>.
		/// </summary>
		/// <param name="concreteMatcher">
		/// <see cref="IMatcher"/> to use to match underlying types.<br/>
		/// Can be overridden during matching with <see cref="MatcherOverrideMappingOptions"/>.
		/// </param>
		public NullableMatcher(IMatcher concreteMatcher) {
			_concreteMatcher = concreteMatcher
				?? throw new ArgumentNullException(nameof(concreteMatcher));
			var nestedMatchingContext = new NestedMatchingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<MatcherOverrideMappingOptions, NestedMatchingContext>(
				m => m?.Matcher != null ? m : new MatcherOverrideMappingOptions(_concreteMatcher, m?.ServiceProvider),
				n => n != null ? new NestedMatchingContext(nestedMatchingContext.ParentMatcher, n) : nestedMatchingContext, options.Cached));
		}


		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMatchInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMatchInternal(sourceType, destinationType, ref mappingOptions, out var concreteMatcher, out var underlyingTypes))
				throw new MapNotFoundException(types);

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			// If both are null it's a match, if only one of them is null they might be non-matchable,
			// thus we return false
			if(source == null) {
				if (destination == null)
					return true;
				else if(sourceType != underlyingTypes.From)
					return false;
			}
			else if(destination == null && destinationType != underlyingTypes.To)
				return false;

			if (sourceType != underlyingTypes.From)
				source = Convert.ChangeType(source, underlyingTypes.From);
			if (destinationType != underlyingTypes.To)
				destination = Convert.ChangeType(destination, underlyingTypes.To);

			try {
				return concreteMatcher.Match(source, underlyingTypes.From, destination, underlyingTypes.To, mappingOptions);
			}
			catch (OperationCanceledException) {
				throw;
			}
			catch (MapNotFoundException) {
				throw new MapNotFoundException(types);
			}
			catch (Exception e) {
				throw new MatcherException(e, types);
			}
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMatchInternal(sourceType, destinationType, ref mappingOptions, out var concreteMatcher, out var underlyingTypes))
				throw new MapNotFoundException(types);

			IMatchMapFactory concreteFactory;
			try {
				concreteFactory = concreteMatcher.MatchFactory(underlyingTypes.From, underlyingTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				throw new MapNotFoundException(types);
			}

			try {
				return new DisposableMatchMapFactory(
					sourceType, destinationType,
					(source, destination) => {
						TypeUtils.CheckObjectType(source, sourceType, nameof(source));
						TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

						// If both are null it's a match, if only one of them is null they might be non-matchable,
						// thus we return false
						if (source == null) {
							if (destination == null)
								return true;
							else if (sourceType != underlyingTypes.From)
								return false;
						}
						else if (destination == null && destinationType != underlyingTypes.To)
							return false;

						if (sourceType != underlyingTypes.From)
							source = Convert.ChangeType(source, underlyingTypes.From);
						if (destinationType != underlyingTypes.To)
							destination = Convert.ChangeType(destination, underlyingTypes.To);

						try {
							return concreteFactory.Invoke(source, destination);
						}
						catch (OperationCanceledException) {
							throw;
						}
						catch (Exception e) {
							throw new MatcherException(e, types);
						}
					}, concreteFactory);
			}
			catch {
				concreteFactory.Dispose();
				throw;
			}
		}


		private bool CanMatchInternal(
			Type sourceType,
			Type destinationType,
			[NotNullWhen(true)] ref MappingOptions? mappingOptions,
			out IMatcher concreteMatcher,
			out (Type From, Type To) underlyingTypes) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// DEV: disallow nested matching if one of the most recent matchers is ourselves or a composite matcher containing us?

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			concreteMatcher = mappingOptions.GetOptions<MatcherOverrideMappingOptions>()?.Matcher
				?? _concreteMatcher;

			Type? destinationNullableUnderlying;
			if (destinationType.IsNullable()) {
				destinationNullableUnderlying = Nullable.GetUnderlyingType(destinationType)!;
				// Type1? == Type2
				if (concreteMatcher.CanMatch(sourceType, destinationNullableUnderlying, mappingOptions)) {
					underlyingTypes = (sourceType, destinationNullableUnderlying);
					return true;
				}
			}
			else
				destinationNullableUnderlying = null;

			Type? sourceNullableUnderlying;
			if (sourceType.IsNullable()) {
				sourceNullableUnderlying = Nullable.GetUnderlyingType(sourceType)!;
				// Type1 == Type2?
				if (concreteMatcher.CanMatch(sourceNullableUnderlying, destinationType, mappingOptions)) {
					underlyingTypes = (sourceNullableUnderlying, destinationType);
					return true;
				}
			}
			else
				sourceNullableUnderlying = null;


			// Type1 == Type2
			if (sourceNullableUnderlying != null && destinationNullableUnderlying != null &&
				concreteMatcher.CanMatch(sourceNullableUnderlying, destinationNullableUnderlying, mappingOptions)) {

				underlyingTypes = (sourceNullableUnderlying, destinationNullableUnderlying);
				return true;
			}
			else {
				underlyingTypes = default;
				return false;
			}
		}
	}
}
