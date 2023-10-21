using System;
using System.Collections;

namespace NeatMapper.Common.Matchers {
	/// <summary>
	/// <see cref="IMatcher"/> which tries to match the given types using another <see cref="IMatcher"/>
	/// and returns false in case it throws <see cref="MapNotFoundException"/>
	/// </summary>
	public sealed class SafeMatcher : IMatcher {
		readonly IMatcher _matcher;

		public SafeMatcher(IMatcher matcher) {
			_matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
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
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions = null) {

			try {
				return _matcher.Match(source, sourceType, destination, destinationType, mappingOptions);
			}
			catch(MapNotFoundException){
				return false;
			}
		}
	}
}
