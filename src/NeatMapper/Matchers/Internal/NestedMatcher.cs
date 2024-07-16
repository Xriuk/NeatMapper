using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

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
		/// Factory used to edit (or create) <see cref="MappingOptions"/> and apply them to <see cref="_matcher"/>.
		/// </summary>
		private readonly Func<MappingOptions, MappingOptions> _optionsFactory;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output <see cref="MappingOptions"/> (<see cref="MappingOptions.Cached"/>
		/// will depend on the factory) from <see cref="_optionsFactory"/>.
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MappingOptions> _optionsCache =
			new ConcurrentDictionary<MappingOptions, MappingOptions>();

		/// <summary>
		/// Cached output <see cref="MappingOptions"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have null keys) and <see cref="MappingOptions.Empty"/> inputs,
		/// also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		private readonly MappingOptions _optionsCacheNull;

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

			_matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
			_optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
			_optionsCacheNull = _optionsFactory.Invoke(null);
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

			return _matcher.Match(source, sourceType, destination, destinationType, GetOrCreateOptions(mappingOptions));
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

			return _matcher.CanMatch(sourceType, destinationType, GetOrCreateOptions(mappingOptions));
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

			return _matcher.MatchFactory(sourceType, destinationType, GetOrCreateOptions(mappingOptions));
		}


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private MappingOptions GetOrCreateOptions(MappingOptions mappingOptions) {
			if (mappingOptions == null || mappingOptions == MappingOptions.Empty)
				return _optionsCacheNull;
			else if (mappingOptions.Cached)
				return _optionsCache.GetOrAdd(mappingOptions, _optionsFactory);
			else
				return _optionsFactory.Invoke(mappingOptions);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
