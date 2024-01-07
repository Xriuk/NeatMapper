using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which creates and caches factories from another <see cref="IMatcher"/>
	/// based on mapped types and <see cref="MappingOptions"/>, and uses them to perform the matches.<br/>
	/// This allows to reuse the same factories if <see cref="MappingOptions"/> do not change.
	/// </summary>
	internal sealed class CachedFactoryMatcher : IMatcher, IMatcherCanMatch, IMatcherFactory {
		private readonly IMatcher _matcher;
		private readonly Dictionary<(Type, Type, MappingOptions), Func<object, object, bool>> _factories = new Dictionary<(Type, Type, MappingOptions), Func<object, object, bool>>();

		public CachedFactoryMatcher(IMatcher matcher) {
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
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return MatchFactory(sourceType, destinationType, mappingOptions).Invoke(source, destination);
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

			return _matcher.CanMatch(sourceType, destinationType, mappingOptions);
		}

		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?, object?, bool
#else
			object, object, bool
#endif
			> MatchFactory(
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

			lock (_factories) {
				if (!_factories.TryGetValue((sourceType, destinationType, mappingOptions), out var factory)) {
					factory = _matcher.MatchFactory(sourceType, destinationType, mappingOptions);
					_factories.Add((sourceType, destinationType, mappingOptions), factory);
				}
				return factory;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
