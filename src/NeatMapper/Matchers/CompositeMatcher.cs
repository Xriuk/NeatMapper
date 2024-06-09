using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which delegates mapping to other <see cref="IMatcher"/>s,
	/// this allows to combine different matching capabilities.<br/>
	/// Types can be matched in any order, the exact one is tried first, then the types are reverted. This
	/// behaviour can be configured with <see cref="CompositeMatcherOptions"/> (and
	/// <see cref="CompositeMatcherMappingOptions"/>).<br/>
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


		/// <summary>
		/// Matchers to delegate the matching to and other settings.
		/// </summary>
		private readonly CompositeMatcherOptions _compositeMatcherOptions;

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
		/// Creates the matcher by using the provided options.
		/// </summary>
		/// <param name="compositeMatcherOptions">Matchers to delegate the matching to and other settings.</param>
		public CompositeMatcher(CompositeMatcherOptions compositeMatcherOptions) {
			if(compositeMatcherOptions == null)
				throw new ArgumentNullException(nameof(compositeMatcherOptions));

			_compositeMatcherOptions = new CompositeMatcherOptions(compositeMatcherOptions);
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
			return MatchInternal(_compositeMatcherOptions.Matchers, source, sourceType, destination, destinationType, mappingOptions);

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
			var undeterminateMatchers = new HashSet<IMatcher>();
			foreach (var matcher in _compositeMatcherOptions.Matchers.OfType<IMatcherCanMatch>()) {
				try {
					if (matcher.CanMatch(sourceType, destinationType, mappingOptions))
						return true;
				}
				catch (InvalidOperationException) {
					undeterminateMatchers.Add(matcher);
				}
			}
			if(mappingOptions.GetOptions<CompositeMatcherMappingOptions>()?.ReverseTypes ?? _compositeMatcherOptions.ReverseTypes) {
				foreach (var matcher in _compositeMatcherOptions.Matchers.OfType<IMatcherCanMatch>()) {
					try {
						if (matcher.CanMatch(destinationType, sourceType, mappingOptions))
							return true;
					}
					catch (InvalidOperationException) {
						undeterminateMatchers.Add(matcher);
					}
				}
			}

			// Try creating default source and destination objects and try matching them
			var matchersLeft = _compositeMatcherOptions.Matchers.Where(m => !(m is IMatcherCanMatch) || undeterminateMatchers.Contains(m));
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

				try {
					MatchInternal(matchersLeft, source, sourceType, destination, destinationType, mappingOptions);
					return true;
				}
				catch (MapNotFoundException) { }
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

			var unavailableMatchersExact = new HashSet<IMatcher>();
			var factoriesExact = new CachedLazyEnumerable<IMatchMapFactory>(
				_compositeMatcherOptions.Matchers.OfType<IMatcherFactory>()
				.Select(matcher => {
					try {
						return matcher.MatchFactory(sourceType, destinationType, mappingOptions);
					}
					catch (MapNotFoundException) {
						unavailableMatchersExact.Add(matcher);
						return null;
					}
				})
				.Concat(_singleElementArray.Select(_ => {
					// Since we finished the mappers, we check if any mapper left can map the types
					foreach (var matcher in _compositeMatcherOptions.Matchers.OfType<IMatcherCanMatch>()) {
						if (matcher is IMatchMapFactory)
							continue;

						try {
							if (!matcher.CanMatch(sourceType, destinationType, mappingOptions)) {
								lock (unavailableMatchersExact) {
									unavailableMatchersExact.Add(matcher);
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
			HashSet<IMatcher> unavailableMatchersReverse;
			CachedLazyEnumerable<IMatchMapFactory> factoriesReverse;
			if(mappingOptions.GetOptions<CompositeMatcherMappingOptions>()?.ReverseTypes ?? _compositeMatcherOptions.ReverseTypes) { 
				unavailableMatchersReverse = new HashSet<IMatcher>();
				factoriesReverse = new CachedLazyEnumerable<IMatchMapFactory>(
					_compositeMatcherOptions.Matchers.OfType<IMatcherFactory>()
					.Select(matcher => {
						try {
							return matcher.MatchFactory(destinationType, sourceType, mappingOptions);
						}
						catch (MapNotFoundException) {
							unavailableMatchersReverse.Add(matcher);
							return null;
						}
					})
					.Concat(_singleElementArray.Select(_ => {
						// Since we finished the mappers, we check if any mapper left can map the types
						foreach (var matcher in _compositeMatcherOptions.Matchers.OfType<IMatcherCanMatch>()) {
							if (matcher is IMatchMapFactory)
								continue;

							try {
								if (!matcher.CanMatch(destinationType, sourceType, mappingOptions)) {
									lock (unavailableMatchersReverse) {
										unavailableMatchersReverse.Add(matcher);
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
			}
			else {
				unavailableMatchersReverse = new HashSet<IMatcher>(_compositeMatcherOptions.Matchers);
				factoriesReverse = new CachedLazyEnumerable<IMatchMapFactory>(Enumerable.Empty<IMatchMapFactory>());
			}

			// DEV: maybe check with CanMatch and if returns false throw instead of creating the factory?

			return new DisposableMatchMapFactory(
				sourceType, destinationType,
				(source, destination) => {
					// Try using the exact factories, if any
					foreach (var factory in factoriesExact) {
						try {
							return factory.Invoke(source, destination);
						}
						catch (MapNotFoundException) { }
					}

					// Try using the reverse factories, if any
					foreach (var factory in factoriesReverse) {
						try {
							return factory.Invoke(destination, source);
						}
						catch (MapNotFoundException) { }
					}

					// Invoke the default map if there are any mappers left (no locking needed on unavailableMappersExact/Reverse
					// because factories is already fully enumerated)
					if (unavailableMatchersExact.Count != _compositeMatcherOptions.Matchers.Count ||
						unavailableMatchersReverse.Count != _compositeMatcherOptions.Matchers.Count) { 

						return MatchInternal(_compositeMatcherOptions.Matchers.Except(unavailableMatchersExact.Intersect(unavailableMatchersReverse)), source, sourceType, destination, destinationType, mappingOptions);
					}
					else
						throw new MapNotFoundException((sourceType, destinationType));
				},
				factoriesExact, factoriesReverse, new LambdaDisposable(() => {
					foreach (var factory in factoriesExact.Cached) {
						factory.Dispose();
					}
					foreach (var factory in factoriesReverse.Cached) {
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

		private bool MatchInternal(IEnumerable<IMatcher> matchers,
			object source, Type sourceType, object destination, Type destinationType,
			MappingOptions mappingOptions) {

			// Try exact map
			foreach (var matcher in matchers) {
				try {
					return matcher.Match(source, sourceType, destination, destinationType, mappingOptions);
				}
				catch (MapNotFoundException) { }
			}

			// Try reverse map
			if (mappingOptions.GetOptions<CompositeMatcherMappingOptions>()?.ReverseTypes ?? _compositeMatcherOptions.ReverseTypes) { 
				foreach (var matcher in matchers) {
					try {
						return matcher.Match(destination, destinationType, source, sourceType, mappingOptions);
					}
					catch (MapNotFoundException) { }
				}
			}

			throw new MapNotFoundException((sourceType, destinationType));
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
