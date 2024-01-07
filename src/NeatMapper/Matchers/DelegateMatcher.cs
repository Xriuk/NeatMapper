using System;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches by invoking a delegate. Should throw <see cref="MapNotFoundException"/>
	/// for incompatible types.
	/// </summary>
	public sealed class DelegateMatcher : IMatcher, IMatcherCanMatch, IMatcherFactory {
		readonly MatchMapDelegate<object, object> _matchDelegate;
		readonly IMatcher _nestedMatcher;
		readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Creates the matcher by using the provided delegate.
		/// </summary>
		/// <param name="matchDelegate">
		/// Delegate to use for matching, should throw <see cref="MapNotFoundException"/>
		/// if the passed objects are not of expected types.
		/// </param>
		/// <param name="nestedMatcher">
		/// Optional matcher passed to the matching context, will be combined with the delegate matcher itself.
		/// </param>
		/// <param name="serviceProvider">Optional service provider passed to the matching context.</param>
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

			return MatchFactory(sourceType, destinationType, mappingOptions).Invoke(source, destination);
		}

		// DEV: remove, and let extension methods handle
		[Obsolete("The method will be removed in future versions, use the extension methods instead.")]
		public bool CanMatch(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			// Try creating two default source and destination objects and try mapping them
			object source;
			object destination;
			try {
				source = ObjectFactory.GetOrCreateCached(sourceType) ?? throw new Exception(); // Just in case
				destination = ObjectFactory.GetOrCreateCached(destinationType) ?? throw new Exception(); // Just in case
			}
			catch {
				throw new InvalidOperationException("Cannot verify if the matcher supports the given match because unable to create the objects to test it");
			}

			try {
				Match(source, sourceType, destination, destinationType, mappingOptions);
				return true;
			}
			catch (MapNotFoundException) {
				return false;
			}
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

			var overrideOptions = mappingOptions?.GetOptions<MatcherOverrideMappingOptions>();
			var context = new MatchingContext(
				overrideOptions?.ServiceProvider ?? _serviceProvider,
				overrideOptions?.Matcher ?? _nestedMatcher,
				this,
				mappingOptions ?? MappingOptions.Empty
			);

			return (source, destination) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));
				TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

				try {
					return _matchDelegate.Invoke(source, destination, context);
				}
				catch (MapNotFoundException) {
					throw;
				}
				catch (TaskCanceledException) {
					throw;
				}
				catch (Exception e) {
					throw new MatcherException(e, (sourceType, destinationType));
				}
			};

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
