﻿using System;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which wraps another <see cref="IMatcher"/>
	/// and returns false in case it throws <see cref="MapNotFoundException"/>
	/// </summary>
	public sealed class SafeMatcher : IMatcher, IMatcherCanMatch {
		private readonly IMatcher _matcher;
		private readonly NestedMatchingContext _nestedMatchingContext;

		/// <summary>
		/// Creates a new instance of <see cref="SafeMatcher"/>.
		/// </summary>
		/// <param name="matcher">Matcher to wrap.</param>
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

		public bool CanMatch(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			mappingOptions = (mappingOptions ?? MappingOptions.Empty)
				.ReplaceOrAdd<NestedMatchingContext>(n => n != null ? new NestedMatchingContext(this, n) : _nestedMatchingContext);

			return _matcher.CanMatch(sourceType, destinationType, mappingOptions);
		}
	}
}
