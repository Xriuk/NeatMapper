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
		public CompositeMatcher(params IMatcher[] matchers) : this((IList<IMatcher>)matchers) { }

		/// <summary>
		/// Creates the matcher by using the provided matchers list
		/// </summary>
		/// <param name="matchers">Matchers to delegate the matching to</param>
		public CompositeMatcher(IList<IMatcher> matchers) {
			if (matchers == null)
				throw new ArgumentNullException(nameof(matchers));

			_matchers = new List<IMatcher>(matchers);
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

			foreach (var matcher in _matchers) {
				try {
					return matcher.Match(source, sourceType, destination, destinationType, mappingOptions);
				}
				catch (MapNotFoundException) { }
			}

			throw new MapNotFoundException((sourceType, destinationType));

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

			var unavailableMatchers = new ConcurrentBag<IMatcher>();
			var factoriesCache = new ConcurrentBag<IMatchMapFactory>();
			var enumerator = _matchers.OfType<IMatcherFactory>().GetEnumerator();

			// DEV: maybe check with CanMatch and if returns false throw instead of creating the factory?

			return new DisposableMatchMapFactory(
				sourceType, destinationType,
				(source, destination) => {
					// Try using cached factories, if any
					foreach (var factory in factoriesCache) {
						try {
							return factory.Invoke(source, destination);
						}
						catch (MapNotFoundException) { }
					}

					// Retrieve and cache new factories, if the matchers throw while retrieving the factory
					// they can never match the given types (we assume that if it cannot provide a factory for two types,
					// it cannot even map them), otherwise they might match them and fail (and we'll retry later)
					lock (enumerator) {
						if (enumerator.MoveNext()) {
							while (true) {
								IMatchMapFactory factory;
								try {
									factory = enumerator.Current.MatchFactory(sourceType, destinationType, mappingOptions);
								}
								catch (MapNotFoundException) {
									unavailableMatchers.Add(enumerator.Current);
									if (enumerator.MoveNext())
										continue;
									else
										break;
								}

								factoriesCache.Add(factory);

								try {
									return factory.Invoke(source, destination);
								}
								catch (MapNotFoundException) { }

								// Since we finished the matchers, we check if any matcher left can match the types
								if (!enumerator.MoveNext()) {
									foreach (var matcher in _matchers.OfType<IMatcherCanMatch>()) {
										if (matcher is IMatcherFactory)
											continue;

										try {
											if (!matcher.CanMatch(sourceType, destinationType, mappingOptions))
												unavailableMatchers.Add(matcher);
										}
										catch { }
									}

									break;
								}
							}
						}
					}

					// Invoke the default match if there are any matchers left
					if (unavailableMatchers.Count != _matchers.Count)
						return Match(source, sourceType, destination, destinationType, mappingOptions);
					else
						throw new MapNotFoundException((sourceType, destinationType));
				},
				enumerator, new LambdaDisposable(() => {
					foreach (var factory in factoriesCache) {
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
		MappingOptions GetOrCreateMappingOptions(MappingOptions options) {
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
