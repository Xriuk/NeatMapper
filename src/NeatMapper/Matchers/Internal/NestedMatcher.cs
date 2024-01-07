using System;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which wraps another <see cref="IMatcher"/> and overrides some <see cref="MappingOptions"/>.
	/// </summary>
	internal sealed class NestedMatcher : IMatcher, IMatcherCanMatch, IMatcherFactory {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private readonly IMatcher _matcher;
		private readonly Func<MappingOptions, MappingOptions> _mappingOptionsEditor;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif

		/// <summary>
		/// Creates a new instance of <see cref="NestedMatcher"/>.
		/// </summary>
		/// <param name="matcher">Matcher to forward the actual matching to.</param>
		/// <param name="mappingOptionsEditor">
		/// Method to invoke to alter the <see cref="MappingOptions"/> passed to the matcher,
		/// both the passed parameter and the returned value may be null.
		/// </param>
		public NestedMatcher(
			IMatcher matcher,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				MappingOptions?, MappingOptions?
#else
				MappingOptions, MappingOptions
#endif
			> mappingOptionsEditor) {

			_matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
			_mappingOptionsEditor = mappingOptionsEditor ?? throw new ArgumentNullException(nameof(mappingOptionsEditor));
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

			return _matcher.Match(source, sourceType, destination, destinationType, _mappingOptionsEditor.Invoke(mappingOptions));
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

			return _matcher.CanMatch(sourceType, destinationType, _mappingOptionsEditor.Invoke(mappingOptions));
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

			return _matcher.MatchFactory(sourceType, destinationType, _mappingOptionsEditor.Invoke(mappingOptions));
		}
	}
}
