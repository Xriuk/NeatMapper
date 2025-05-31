using System;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which delegates mapping to other <see cref="IMatcher"/>s,
	/// this allows to combine different matching capabilities.<br/>
	/// Types can be matched in any order, the exact one is tried first, then the types are reverted. This
	/// behaviour can be configured with <see cref="CompositeMatcherOptions"/> (and
	/// <see cref="CompositeMatcherMappingOptions"/>).<br/>
	/// Each matcher is invoked in order (in the exact order of types first, then with the types reverted)
	/// and the first one to succeed in matching is returned.
	/// </summary>
	public sealed class CompositeMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// Matchers to delegate the matching to and other settings.
		/// </summary>
		private readonly CompositeMatcherOptions _compositeMatcherOptions;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		/// <summary>
		/// Creates the matcher by using the provided options.
		/// </summary>
		/// <param name="compositeMatcherOptions">Matchers to delegate the matching to and other settings.</param>
		public CompositeMatcher(CompositeMatcherOptions compositeMatcherOptions) {
			if(compositeMatcherOptions == null)
				throw new ArgumentNullException(nameof(compositeMatcherOptions));

			_compositeMatcherOptions = new CompositeMatcherOptions(compositeMatcherOptions);
			var nestedMatchingContext = new NestedMatchingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<MatcherOverrideMappingOptions, NestedMatchingContext>(
				m => m?.Matcher != null ? m : new MatcherOverrideMappingOptions(this, m?.ServiceProvider),
				n => n != null ? new NestedMatchingContext(this, n) : nestedMatchingContext, options.Cached));
		}


		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			if(_compositeMatcherOptions.Matchers.Any(m => m.CanMatch(sourceType, destinationType, mappingOptions)))
				return true;

			// Try reversing types
			if (sourceType != destinationType &&
				(mappingOptions.GetOptions<CompositeMatcherMappingOptions>()?.ReverseTypes ?? _compositeMatcherOptions.ReverseTypes)) {

				return _compositeMatcherOptions.Matchers.Any(m => m.CanMatch(destinationType, sourceType, mappingOptions));
			}

			return false;
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var matcher = _compositeMatcherOptions.Matchers.FirstOrDefault(m => m.CanMatch(sourceType, destinationType, mappingOptions));
			if (matcher != null)
				return matcher.Match(source, sourceType, destination, destinationType, mappingOptions);

			// Try reversing types
			if (sourceType != destinationType &&
				(mappingOptions.GetOptions<CompositeMatcherMappingOptions>()?.ReverseTypes ?? _compositeMatcherOptions.ReverseTypes)) {

				matcher = _compositeMatcherOptions.Matchers.FirstOrDefault(m => m.CanMatch(destinationType, sourceType, mappingOptions));
				if (matcher != null)
					return matcher.Match(destination, destinationType, source, sourceType, mappingOptions);
			}

			throw new MapNotFoundException((sourceType, destinationType));
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			// Try retrieving a new factory
			IMatcher? validMatcher = null;
			foreach (var matcher in _compositeMatcherOptions.Matchers) {
				if (matcher.CanMatch(sourceType, destinationType, mappingOptions)) {
					if (matcher is IMatcherFactory factory)
						return factory.MatchFactory(sourceType, destinationType, mappingOptions);
					else
						validMatcher ??= matcher;
				}
			}

			// If we can match the types we return Match wrapped in a delegate
			if (validMatcher != null) {
				return new DefaultMatchMapFactory(
					sourceType, destinationType,
					(source, destination) => validMatcher.Match(source, sourceType, destination, destinationType, mappingOptions));
			}

			// Try reversing types
			if (sourceType != destinationType &&
				(mappingOptions.GetOptions<CompositeMatcherMappingOptions>()?.ReverseTypes ?? _compositeMatcherOptions.ReverseTypes)) {

				// Try retrieving a new factory in reverse
				foreach (var matcher in _compositeMatcherOptions.Matchers) {
					if (matcher.CanMatch(destinationType, sourceType, mappingOptions)) {
						if (matcher is IMatcherFactory factory) {
							var reverseFactory = factory.MatchFactory(destinationType, sourceType, mappingOptions);
							try { 
								return new DisposableMatchMapFactory(
									sourceType, destinationType,
									(source, destination) => reverseFactory.Invoke(destination, source),
									reverseFactory);
							}
							catch {
								reverseFactory.Dispose();
								throw;
							}
						}
						else
							validMatcher ??= matcher;
					}
				}

				// If we can match the types we return Match reversed and wrapped in a delegate
				if (validMatcher != null) {
					return new DefaultMatchMapFactory(
						sourceType, destinationType,
						(source, destination) => validMatcher.Match(destination, destinationType, source, sourceType, mappingOptions));
				}
			}

			throw new MapNotFoundException((sourceType, destinationType));
		}
	}
}
