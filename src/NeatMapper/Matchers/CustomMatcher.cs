using System;
using System.Collections.Concurrent;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches objects by using <see cref="IMatchMap{TSource, TDestination}"/>.
	/// </summary>
	public sealed class CustomMatcher : IMatcher, IMatcherCanMatch, IMatcherFactory {
		/// <summary>
		/// Configuration for class and additional maps for the matcher.
		/// </summary>
		private readonly CustomMapsConfiguration _configuration;

		/// <summary>
		/// Service provider available in the created <see cref="MatchingContext"/>s.
		/// </summary>
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output <see cref="MatchingContext"/>.
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MatchingContext> _contextsCache
			= new ConcurrentDictionary<MappingOptions, MatchingContext>();

		/// <summary>
		/// Cached output <see cref="MatchingContext"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have null keys) and <see cref="MappingOptions.Empty"/>,
		/// also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		private readonly MatchingContext _contextsCacheNull;


		/// <summary>
		/// Creates a new instance of <see cref="CustomMatcher"/>.<br/>
		/// At least one between <paramref name="mapsOptions"/> and <paramref name="additionalMapsOptions"/>
		/// should be specified.
		/// </summary>
		/// <param name="mapsOptions">Options to retrieve user-defined maps for the matcher, null to ignore.</param>
		/// <param name="additionalMapsOptions">Additional user-defined maps for the matcher, null to ignore.</param>
		/// <param name="serviceProvider">
		/// Service provider to be passed to the maps inside <see cref="MatchingContext"/>, 
		/// null to pass an empty service provider.<br/>
		/// Can be overridden during matching with <see cref="MatcherOverrideMappingOptions"/>.
		/// </param>
		public CustomMatcher(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomMapsOptions?
#else
			CustomMapsOptions
#endif
			mapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomMatchAdditionalMapsOptions?
#else
			CustomMatchAdditionalMapsOptions
#endif
			additionalMapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) {

			_configuration = new CustomMapsConfiguration(
				(_, i) => {
					if (!i.IsGenericType)
						return false;
					var type = i.GetGenericTypeDefinition();
					return type == typeof(IMatchMap<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(IMatchMapStatic<,>)
#endif
					;
				},
				(mapsOptions ?? new CustomMapsOptions()).TypesToScan,
				additionalMapsOptions?._maps.Values
			);
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
			_contextsCacheNull = CreateMatchingContext(MappingOptions.Empty);
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			var comparer = _configuration.GetDoubleMap<MatchingContext>((sourceType, destinationType));
			var context = GetOrCreateMatchingContext(mappingOptions);

			try {
				return (bool)comparer.Invoke(source, destination, context);
			}
			catch (MappingException e) {
				throw new MatcherException(e.InnerException, (sourceType, destinationType));
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			try {
				// Try retrieving a regular map
				_configuration.GetDoubleMap<MatchingContext>((sourceType, destinationType));
				return true;
			}
			catch (MapNotFoundException) {
				return false;
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

			var comparer = _configuration.GetDoubleMap<MatchingContext>((sourceType, destinationType));
			var context = GetOrCreateMatchingContext(mappingOptions);

			return new DefaultMatchMapFactory(sourceType, destinationType, (source, destination) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));
				TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

				try {
					return (bool)comparer.Invoke(source, destination, context);
				}
				catch (MappingException e) {
					throw new MatcherException(e.InnerException, (sourceType, destinationType));
				}
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private MatchingContext GetOrCreateMatchingContext(MappingOptions options) {
			if (options == null || options == MappingOptions.Empty)
				return _contextsCacheNull;
			else if (options.Cached)
				return _contextsCache.GetOrAdd(options, CreateMatchingContext);
			else
				return CreateMatchingContext(options);
		}

		private MatchingContext CreateMatchingContext(MappingOptions options) {
			var overrideOptions = options.GetOptions<MatcherOverrideMappingOptions>();
			return new MatchingContext(
				overrideOptions?.ServiceProvider ?? _serviceProvider,
				overrideOptions?.Matcher ?? this,
				this,
				options
			);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
