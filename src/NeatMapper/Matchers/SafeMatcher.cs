using System;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which wraps another <see cref="IMatcher"/> and returns <see langword="false">
	/// in case it throws <see cref="MapNotFoundException"/>.
	/// </summary>
	public sealed class SafeMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// Matcher to which delegate all the operations.
		/// </summary>
		private readonly IMatcher _matcher;

		/// <summary>
		/// Cached nested matching context.
		/// </summary>
		private readonly NestedMatchingContext _nestedMatchingContext;


		/// <summary>
		/// Creates a new instance of <see cref="SafeMatcher"/>.
		/// </summary>
		/// <param name="matcher">Matcher to which delegate all the operations.</param>
		public SafeMatcher(IMatcher matcher) {
			_matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
			_nestedMatchingContext = new NestedMatchingContext(this);
		}


		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return _matcher.CanMatch(sourceType, destinationType, GetOptions(mappingOptions));
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			mappingOptions = GetOptions(mappingOptions);

			if(!_matcher.CanMatch(sourceType, destinationType, mappingOptions))
				return false;

			try {
				return _matcher.Match(source, sourceType, destination, destinationType, mappingOptions);
			}
			catch(MapNotFoundException){
				return false;
			}
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			mappingOptions = GetOptions(mappingOptions);

			if (!_matcher.CanMatch(sourceType, destinationType, mappingOptions))
				return new DefaultMatchMapFactory(sourceType, destinationType, DefaultFactory);

			try { 
				return _matcher.MatchFactory(sourceType, destinationType, mappingOptions);
			}
			catch (MapNotFoundException) {
				return new DefaultMatchMapFactory(sourceType, destinationType, DefaultFactory);
			}
		}


		private MappingOptions GetOptions(MappingOptions? mappingOptions) {
			return (mappingOptions ?? MappingOptions.Empty)
				.ReplaceOrAdd<NestedMatchingContext>(n => n != null ? new NestedMatchingContext(this, n) : _nestedMatchingContext);
		}

		private static bool DefaultFactory(object? source, object? destination) {
			return false;
		}
	}
}
