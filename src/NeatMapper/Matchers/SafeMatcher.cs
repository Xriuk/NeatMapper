using System;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which tries to match the given types using another <see cref="IMatcher"/>
	/// and returns false in case it throws <see cref="MapNotFoundException"/>
	/// </summary>
	public sealed class SafeMatcher : IMatcher {
		private readonly IMatcher _matcher;
		private readonly NestedMatchingContext _nestedMatchingContext;

		public SafeMatcher(IMatcher matcher) {
			_matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
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

			mappingOptions = (mappingOptions ?? MappingOptions.Empty)
				.ReplaceOrAdd<NestedMatchingContext>(n => n != null ? new NestedMatchingContext(this, n) : _nestedMatchingContext);

			try {
				return _matcher.Match(source, sourceType, destination, destinationType, mappingOptions);
			}
			catch(MapNotFoundException){
				return false;
			}
		}
	}
}
