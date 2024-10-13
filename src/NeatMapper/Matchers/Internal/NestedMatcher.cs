using System;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which wraps another <see cref="IMatcher"/> and overrides some <see cref="MappingOptions"/>.
	/// </summary>
	internal sealed class NestedMatcher : IMatcher, IMatcherCanMatch, IMatcherFactory {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		/// <summary>
		/// <see cref="IMatcher"/> to wrap.
		/// </summary>
		private readonly IMatcher _matcher;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


		/// <summary>
		/// Creates a new instance of <see cref="NestedMatcher"/>.
		/// </summary>
		/// <param name="matcher">Matcher to forward the actual matching to.</param>
		/// <param name="optionsFactory">
		/// Method to invoke to alter the <see cref="MappingOptions"/> passed to the matcher,
		/// both the passed parameter and the returned value may be null.
		/// If null, the passed parameter should be treated as <see cref="MappingOptions.Empty"/>.
		/// </param>
		public NestedMatcher(
			IMatcher matcher,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				MappingOptions?, MappingOptions?
#else
				MappingOptions, MappingOptions
#endif
			> optionsFactory) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			_matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(
				optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory)));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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

			return _matcher.Match(source, sourceType, destination, destinationType, _optionsCache.GetOrCreate(mappingOptions));
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

			return _matcher.CanMatch(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions));
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

			return _matcher.MatchFactory(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions));
		}
	}
}
