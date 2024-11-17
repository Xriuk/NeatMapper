using System;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which wraps another <see cref="IMatcher"/> and overrides some <see cref="MappingOptions"/>.
	/// </summary>
	internal sealed class NestedMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// <see cref="IMatcher"/> to wrap.
		/// </summary>
		private readonly IMatcher _matcher;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		/// <summary>
		/// Creates a new instance of <see cref="NestedMatcher"/>.
		/// </summary>
		/// <param name="matcher">Matcher to forward the actual matching to.</param>
		/// <param name="optionsFactory">
		/// Method to invoke to alter the <see cref="MappingOptions"/> passed to the matcher.
		/// </param>
		public NestedMatcher(IMatcher matcher, Func<MappingOptions, MappingOptions> optionsFactory) {
			_matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(
				optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory)));
		}


		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return _matcher.CanMatch(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions));
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			return _matcher.Match(source, sourceType, destination, destinationType, _optionsCache.GetOrCreate(mappingOptions));
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return _matcher.MatchFactory(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions));
		}
	}
}
