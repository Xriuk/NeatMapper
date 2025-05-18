using System;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IMatcher"/> which just returns false for every match.
	/// </summary>
	public sealed class EmptyMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// Singleton instance of the matcher.
		/// </summary>
		public static readonly IMatcher Instance = new EmptyMatcher();


		private EmptyMatcher() { }


		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return false;
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			return false;
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return new DefaultMatchMapFactory(sourceType, destinationType, (source, destination) => false);
		}
	}
}
