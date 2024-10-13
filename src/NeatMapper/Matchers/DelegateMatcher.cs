using System;
using System.Collections.Generic;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches by invoking a delegate.
	/// </summary>
	public sealed class DelegateMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// Creates an instance of <see cref="DelegateMatcher"/> by using the provided delegate.
		/// </summary>
		/// <typeparam name="TSource">Type of the source object.</typeparam>
		/// <typeparam name="TDestination">Type of the destination object.</typeparam>
		/// <param name="matchDelegate">Delegate to use for matching.</param>
		/// <param name="nestedMatcher">
		/// Optional matcher passed to the matching context, will be combined with the delegate matcher itself.
		/// </param>
		/// <param name="serviceProvider">Optional service provider passed to the matching context.</param>
		/// <returns>A new instance of <see cref="DelegateMatcher"/> for the given delegate.</returns>
		public static DelegateMatcher Create<TSource, TDestination>(
			MatchMapDelegate<TSource, TDestination> matchDelegate,
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return new DelegateMatcher(typeof(TSource), typeof(TDestination),
				(source, destination, context) => {
					TypeUtils.CheckObjectType(source, typeof(TSource), nameof(source));
					TypeUtils.CheckObjectType(destination, typeof(TDestination), nameof(destination));

					try {
						return matchDelegate.Invoke((TSource)source, (TDestination)destination, context);
					}
					catch (MapNotFoundException e) {
						if (e.From == typeof(TSource) && e.To == typeof(TDestination))
							throw;
						else
							throw new MatcherException(e, (typeof(TSource), typeof(TDestination)));
					}
					catch (OperationCanceledException) {
						throw;
					}
					catch (Exception e) {
						throw new MatcherException(e, (typeof(TSource), typeof(TDestination)));
					}
				},
				nestedMatcher,
				serviceProvider);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}


		/// <summary>
		/// Type which can be matched
		/// </summary>
		private readonly Type _sourceType;

		/// <summary>
		/// Type which can be matched
		/// </summary>
		private readonly Type _destinationType;

		/// <summary>
		/// Delegate to use for matching.
		/// </summary>
		private readonly MatchMapDelegate<object, object> _matchDelegate;

		/// <summary>
		/// Nested matcher available in the created <see cref="MatchingContext"/>s.
		/// </summary>
		private readonly IMatcher _nestedMatcher;

		/// <summary>
		/// Service provider available in the created <see cref="MatchingContext"/>s.
		/// </summary>
		private readonly IServiceProvider _serviceProvider;


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
		private DelegateMatcher(
			Type sourceType,
			Type destinationType,
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

			_sourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			_destinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			_matchDelegate = matchDelegate ?? throw new ArgumentNullException(nameof(matchDelegate));
			_nestedMatcher = nestedMatcher != null ? (IMatcher)new CompositeMatcher(new CompositeMatcherOptions {
				Matchers = new List<IMatcher> { this, nestedMatcher }
			}) : (IMatcher)this;
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return sourceType == _sourceType && destinationType == _destinationType;
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

			if (!CanMatch(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			// Not caching context (for now), because DelegateMatcher is pretty short-lived and created when needed (or at least it should be)
			var overrideOptions = mappingOptions?.GetOptions<MatcherOverrideMappingOptions>();
			var context = new MatchingContext(
				overrideOptions?.ServiceProvider ?? _serviceProvider,
				overrideOptions?.Matcher ?? _nestedMatcher,
				this,
				mappingOptions ?? MappingOptions.Empty
			);

			return new DefaultMatchMapFactory(sourceType, destinationType, (source, destination) => _matchDelegate.Invoke(source, destination, context));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
