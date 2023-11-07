using System;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches by invoking a delegate, won't throw as the delegate should always compare two objects
	/// (will return false for those that cannot be matched)
	/// </summary>
	public sealed class DelegateMatcher : IMatcher, IMatcherCanMatch {
		readonly MatchMapDelegate<object, object> _matchDelegate;
		readonly IMatcher _nestedMatcher;
		readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Creates the matcher by using the provided delegate
		/// </summary>
		/// <param name="matchDelegate">Delegate to use for matching</param>
		/// <param name="nestedMatcher">Optional matcher passed to the mapping context, will be combined with the delegate matcher itself</param>
		/// <param name="serviceProvider">Optional service provider passed to the mapping context</param>
		/// <exception cref="ArgumentNullException"></exception>
		public DelegateMatcher(
			MatchMapDelegate<object, object> matchDelegate,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IMatcher?
#else
			IMatcher
#endif
			nestedMatcher = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) {

			_matchDelegate = matchDelegate ?? throw new ArgumentNullException(nameof(matchDelegate));
			_nestedMatcher = nestedMatcher != null ? (IMatcher)new CompositeMatcher(this, nestedMatcher) : (IMatcher)this;
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			var overrideOptions = mappingOptions?.GetOptions<MatcherOverrideMappingOptions>();
			var context = new MatchingContext(
				overrideOptions?.ServiceProvider ?? _serviceProvider,
				overrideOptions?.Matcher ?? _nestedMatcher,
				mappingOptions ?? MappingOptions.Empty
			);

			try { 
				return _matchDelegate.Invoke(source, destination, context);
			}
			catch (Exception e) {
				throw new MatcherException(e, (sourceType, destinationType));
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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

			// Always true as the types CAN be matched, but the delegate should return false for not compatible types
			return true;
		}
	}
}
