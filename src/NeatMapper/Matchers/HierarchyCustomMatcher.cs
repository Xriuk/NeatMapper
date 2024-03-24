using System.Linq;
using System;
using System.Collections.Concurrent;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches objects by using <see cref="IHierarchyMatchMap{TSource, TDestination}"/>.
	/// </summary>
	public sealed class HierarchyCustomMatcher : IMatcher, IMatcherCanMatch, IMatcherFactory {
		/// <summary>
		/// Configuration for class and additional maps for the matcher.
		/// </summary>
		private readonly CustomMapsConfiguration _configuration;

		/// <summary>
		/// Service provider available in the created <see cref="MatchingContext"/>s.
		/// </summary>
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> and output <see cref="MatchingContext"/>.
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MatchingContext> _contextsCache
			= new ConcurrentDictionary<MappingOptions, MatchingContext>();

		/// <summary>
		/// Cached output <see cref="MatchingContext"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have a null key), also provides faster access since locking isn't needed
		/// for thread-safety.
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
		public HierarchyCustomMatcher(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomMapsOptions?
#else
			CustomMapsOptions
#endif
			mapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomHierarchyMatchAdditionalMapsOptions?
#else
			CustomHierarchyMatchAdditionalMapsOptions
#endif
			additionalMapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) {

			_configuration = new CustomMapsConfiguration(
				(t, i) => {
					// Hierarchy matchers do not support generic maps
					if (!i.IsGenericType || t.ContainsGenericParameters)
						return false;
					var type = i.GetGenericTypeDefinition();
					return type == typeof(IHierarchyMatchMap<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(IHierarchyMatchMapStatic<,>)
#endif
					;
				},
				(mapsOptions ?? new CustomMapsOptions()).TypesToScan,
				additionalMapsOptions?._maps.Values
			);
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
			_contextsCacheNull = new MatchingContext(_serviceProvider, this, this, MappingOptions.Empty);
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

			return _configuration.Maps.Any(m => m.Key.From.IsAssignableFrom(sourceType) && m.Key.To.IsAssignableFrom(destinationType));
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

			var comparer = _configuration.GetDoubleMapCustomMatch<MatchingContext>((sourceType, destinationType), m =>
				m.Key.From.IsAssignableFrom(sourceType) &&
				m.Key.To.IsAssignableFrom(destinationType));
			MatchingContext context;
			if (mappingOptions == null)
				context = _contextsCacheNull;
			else {
				context = _contextsCache.GetOrAdd(mappingOptions, opts => {
					var overrideOptions = mappingOptions.GetOptions<MatcherOverrideMappingOptions>();
					return new MatchingContext(
						overrideOptions?.ServiceProvider ?? _serviceProvider,
						overrideOptions?.Matcher ?? this,
						this,
						mappingOptions
					);
				});
			}

			return new MatchMapFactory(sourceType, destinationType, (source, destination) => {
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
	}
}
