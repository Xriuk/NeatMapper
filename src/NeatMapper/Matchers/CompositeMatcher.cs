using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	public sealed class CompositeMatcher : IMatcher {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		readonly IList<IMatcher> _matchers;

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

			var overrideOptions = mappingOptions?.GetOptions<MatcherOverrideMappingOptions>();
			if (overrideOptions == null) {
				overrideOptions = new MatcherOverrideMappingOptions();
				if (mappingOptions != null)
					mappingOptions = new MappingOptions(mappingOptions.AsEnumerable().Concat(new[] { overrideOptions }));
				else
					mappingOptions = new MappingOptions(new[] { overrideOptions });
			}

			overrideOptions.Matcher = this;

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
	}
}
