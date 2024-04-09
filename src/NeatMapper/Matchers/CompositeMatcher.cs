using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which delegates mapping to other <see cref="IMatcher"/>s,
	/// this allows to combine different matching capabilities.<br/>
	/// Each matcher is invoked in order and the first one to succeed in matching is returned.
	/// </summary>
	public sealed class CompositeMatcher : IMatcher, IMatcherCanMatch, IMatcherFactory {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		/// <summary>
		/// Singleton array used for Linq queries.
		/// </summary>
		private static readonly object[] _singleElementArray = new object[] { null };


		private static bool MatchInternal(IEnumerable<IMatcher> matchers,
			object source, Type sourceType, object destination, Type destinationType,
			MappingOptions mappingOptions) {

			foreach (var matcher in matchers) {
				try {
					return matcher.Match(source, sourceType, destination, destinationType, mappingOptions);
				}
				catch (MapNotFoundException) { }
			}

			throw new MapNotFoundException((sourceType, destinationType));
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


		/// <summary>
		/// List of <see cref="IMatcher"/>s to be tried in order when matching types.
		/// </summary>
		private readonly IList<IMatcher> _matchers;

		/// <summary>
		/// Cached <see cref="NestedMatchingContext"/> to provide, if not already provided in <see cref="MappingOptions"/>.
		/// </summary>
		private readonly NestedMatchingContext _nestedMatchingContext;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MappingOptions> _optionsCache = new ConcurrentDictionary<MappingOptions, MappingOptions>();

		/// <summary>
		/// Cached output <see cref="MappingOptions"/> for the <see langword="null"/> input <see cref="MappingOptions"/>
		/// (since a dictionary can't have a null key), also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		private readonly MappingOptions _optionsCacheNull;


		/// <summary>
		/// Creates the matcher by using the provided matchers list
		/// </summary>
		/// <param name="matchers">Matchers to delegate the matching to</param>
		public CompositeMatcher(params IMatcher[] matchers) : this((IList<IMatcher>)matchers ?? throw new ArgumentNullException(nameof(matchers))) { }

		/// <summary>
		/// Creates the matcher by using the provided matchers list
		/// </summary>
		/// <param name="matchers">Matchers to delegate the matching to</param>
		public CompositeMatcher(IList<IMatcher> matchers) {
			_matchers = new List<IMatcher>(matchers ?? throw new ArgumentNullException(nameof(matchers)));
			_nestedMatchingContext = new NestedMatchingContext(this);
			_optionsCacheNull = GetOrCreateMappingOptions(MappingOptions.Empty);
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

			mappingOptions = GetOrCreateMappingOptions(mappingOptions);
			return MatchInternal(_matchers, source, sourceType, destination, destinationType, mappingOptions);

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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = GetOrCreateMappingOptions(mappingOptions);

			// Check if any mapper implements IMapperCanMap, if one of them throws it means that the map can be checked only when mapping
			var undeterminateMatchers = new List<IMatcher>();
			foreach (var matcher in _matchers.OfType<IMatcherCanMatch>()) {
				try {
					if (matcher.CanMatch(sourceType, destinationType, mappingOptions))
						return true;
				}
				catch (InvalidOperationException) {
					undeterminateMatchers.Add(matcher);
				}
			}

			// Try creating default source and destination objects and try matching them
			var matchersLeft = _matchers.Where(m => !(m is IMatcherCanMatch) || undeterminateMatchers.IndexOf(m) != -1);
			if (matchersLeft.Any()) {
				object source;
				object destination;
				try {
					source = ObjectFactory.GetOrCreateCached(sourceType) ?? throw new Exception(); // Just in case
					destination = ObjectFactory.GetOrCreateCached(destinationType) ?? throw new Exception(); // Just in case
				}
				catch {
					throw new InvalidOperationException("Cannot verify if the matcher supports the given match because unable to create the objects to test it");
				}

				foreach (var matcher in matchersLeft) {
					try {
						matcher.Match(source, sourceType, destination, destinationType, mappingOptions);
						return true;
					}
					catch (MapNotFoundException) { }
				}
			}

			if (undeterminateMatchers.Count > 0)
				throw new InvalidOperationException("Cannot verify if the matcher supports the given match");
			else
				return false;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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

			mappingOptions = GetOrCreateMappingOptions(mappingOptions);

			var unavailableMatchers = new HashSet<IMatcher>();
			var factories = new CachedLazyEnumerable<IMatchMapFactory>(
				_matchers.OfType<IMatcherFactory>()
				.Select(matcher => {
					try {
						return matcher.MatchFactory(sourceType, destinationType, mappingOptions);
					}
					catch (MapNotFoundException) {
						unavailableMatchers.Add(matcher);
						return null;
					}
				})
				.Concat(_singleElementArray.Select(_ => {
					// Since we finished the mappers, we check if any mapper left can map the types
					foreach (var matcher in _matchers.OfType<IMatcherCanMatch>()) {
						if (matcher is IMatchMapFactory)
							continue;

						try {
							if (!matcher.CanMatch(sourceType, destinationType, mappingOptions)) {
								lock (unavailableMatchers) {
									unavailableMatchers.Add(matcher);
								}
							}
							else
								break;
						}
						catch { }
					}

					return (IMatchMapFactory)null;
				}))
				.Where(factory => factory != null));

			// DEV: maybe check with CanMatch and if returns false throw instead of creating the factory?

			return new DisposableMatchMapFactory(
				sourceType, destinationType,
				(source, destination) => {
					// Try using the factories, if any
					foreach (var factory in factories) {
						try {
							return factory.Invoke(source, destination);
						}
						catch (MapNotFoundException) { }
					}

					// Invoke the default map if there are any mappers left (no locking needed on unavailableMappers
					// because factories is already fully enumerated)
					if (unavailableMatchers.Count != _matchers.Count)
						return MatchInternal(_matchers.Except(unavailableMatchers), source, sourceType, destination, destinationType, mappingOptions);
					else
						throw new MapNotFoundException((sourceType, destinationType));
				},
				factories, new LambdaDisposable(() => {
					foreach (var factory in factories.Cached) {
						factory.Dispose();
					}
				}));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		// Will override the matcher if not already overridden
		private MappingOptions GetOrCreateMappingOptions(MappingOptions options) {
			if (options == null)
				return _optionsCacheNull;
			else {
				return _optionsCache.GetOrAdd(options, opts => opts.ReplaceOrAdd<MatcherOverrideMappingOptions, NestedMatchingContext>(
					m => m?.Matcher != null ? m : new MatcherOverrideMappingOptions(this, m?.ServiceProvider),
					n => n != null ? new NestedMatchingContext(this, n) : _nestedMatchingContext));
			}
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
