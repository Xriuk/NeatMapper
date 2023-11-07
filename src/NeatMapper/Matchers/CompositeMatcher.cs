using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	public sealed class CompositeMatcher : IMatcher, IMatcherCanMatch {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private readonly IList<IMatcher> _matchers;
		private readonly NestedMatchingContext _nestedMatchingContext;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif

		/// <summary>
		/// Creates the matcher by using the provided matchers list
		/// </summary>
		/// <param name="matchers">Matchers to delegate the matching to</param>
		public CompositeMatcher(params IMatcher[] matchers) : this((IList<IMatcher>)matchers) { }

		/// <summary>
		/// Creates the matcher by using the provided matchers list
		/// </summary>
		/// <param name="matchers">Matchers to delegate the matching to</param>
		public CompositeMatcher(IList<IMatcher> matchers) {
			if (matchers == null)
				throw new ArgumentNullException(nameof(matchers));

			_matchers = new List<IMatcher>(matchers);
			_nestedMatchingContext = new NestedMatchingContext(this);
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			foreach (var matcher in _matchers) {
				try {
					return matcher.Match(source, sourceType, destination, destinationType, mappingOptions);
				}
				catch (MapNotFoundException) { }
			}

			throw new MapNotFoundException((sourceType, destinationType));

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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			// Check if any mapper implements IMapperCanMap, if one of them throws it means that the map can be checked only when mapping
			var undeterminateMatchers = new List<IMatcher>();
			foreach (var matcher in _matchers.OfType<IMatcherCanMatch>()) {
				try {
					if (matcher.CanMatch(sourceType, destinationType, mappingOptions))
						return true;
				}
				catch (InvalidOperationException) {
					undeterminateMatchers.Add(matcher);
				}
			}

			// Try creating default source and destination objects and try matching them
			var matchersLeft = _matchers.Where(m => !(m is IMatcherCanMatch) || undeterminateMatchers.IndexOf(m) != -1);
			if (matchersLeft.Any()) {
				object source;
				object destination;
				try {
					source = ObjectFactory.GetOrCreateCached(sourceType) ?? throw new Exception(); // Just in case
					destination = ObjectFactory.GetOrCreateCached(destinationType) ?? throw new Exception(); // Just in case
				}
				catch {
					throw new InvalidOperationException("Cannot verify if the matcher supports the given match because unable to create the objects to test it");
				}

				foreach (var matcher in matchersLeft) {
					try {
						matcher.Match(source, sourceType, destination, destinationType, mappingOptions);
						return true;
					}
					catch (MapNotFoundException) { }
				}
			}

			if (undeterminateMatchers.Count > 0)
				throw new InvalidOperationException("Cannot verify if the matcher supports the given match");
			else
				return false;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		// Will override a mapper if not already overridden
		MappingOptions MergeOrCreateMappingOptions(MappingOptions options) {
			return (options ?? MappingOptions.Empty).ReplaceOrAdd<MatcherOverrideMappingOptions, NestedMatchingContext>(
				m => m?.Matcher != null ? m : new MatcherOverrideMappingOptions(this, m?.ServiceProvider),
				n => n != null ? new NestedMatchingContext(this, n) : _nestedMatchingContext);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
