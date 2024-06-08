using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches by invoking a delegate. Should throw <see cref="MapNotFoundException"/>
	/// for incompatible types.
	/// </summary>
	public sealed class DelegateMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// Delegate to use for matching.
		/// </summary>
		readonly MatchMapDelegate<object, object> _matchDelegate;

		/// <summary>
		/// Nested matcher available in the created <see cref="MatchingContext"/>s.
		/// </summary>
		readonly IMatcher _nestedMatcher;

		/// <summary>
		/// Service provider available in the created <see cref="MatchingContext"/>s.
		/// </summary>
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
			_nestedMatcher = nestedMatcher != null ? (IMatcher)new CompositeMatcher(new CompositeMatcherOptions {
				Matchers = new List<IMatcher> { this, nestedMatcher }
			}) : (IMatcher)this;
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

			using (var factory = MatchFactory(sourceType, destinationType, mappingOptions)) {
				return factory.Invoke(source, destination);
			}
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Not caching context (for now), because DelegateMatcher is pretty short-lived and created when needed (or at least it should be)
			var overrideOptions = mappingOptions?.GetOptions<MatcherOverrideMappingOptions>();
			var context = new MatchingContext(
				overrideOptions?.ServiceProvider ?? _serviceProvider,
				overrideOptions?.Matcher ?? _nestedMatcher,
				this,
				mappingOptions ?? MappingOptions.Empty
			);

			return new DefaultMatchMapFactory(sourceType, destinationType, (source, destination) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));
				TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

				try {
					return _matchDelegate.Invoke(source, destination, context);
				}
				catch (MapNotFoundException e) {
					if (e.From == sourceType && e.To == destinationType)
						throw;
					else
						throw new MappingException(e, (sourceType, destinationType));
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception e) {
					throw new MatcherException(e, (sourceType, destinationType));
				}
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
