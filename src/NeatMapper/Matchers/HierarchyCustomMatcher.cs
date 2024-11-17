using System;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches objects by using <see cref="IHierarchyMatchMap{TSource, TDestination}"/>.
	/// </summary>
	public sealed class HierarchyCustomMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// Configuration for class and additional maps for the matcher.
		/// </summary>
		private readonly CustomMapsConfiguration _configuration;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> and output <see cref="MatchingContext"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MatchingContext> _contextsCache;


		/// <summary>
		/// Creates a new instance of <see cref="CustomMatcher"/>.
		/// </summary>
		/// <param name="mapsOptions">Options to retrieve user-defined maps for the matcher, null to ignore.</param>
		/// <param name="additionalMapsOptions">Additional user-defined maps for the matcher, null to ignore.</param>
		/// <param name="serviceProvider">
		/// Service provider to be passed to the maps inside <see cref="MatchingContext"/>, 
		/// null to pass an empty service provider.<br/>
		/// Can be overridden during matching with <see cref="MatcherOverrideMappingOptions"/>.
		/// </param>
		/// <remarks>
		/// At least one between <paramref name="mapsOptions"/> and <paramref name="additionalMapsOptions"/>
		/// should be specified.
		/// </remarks>
		public HierarchyCustomMatcher(
			CustomMapsOptions? mapsOptions = null,
			CustomHierarchyMatchAdditionalMapsOptions? additionalMapsOptions = null,
			IServiceProvider? serviceProvider = null) {

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
			serviceProvider ??= EmptyServiceProvider.Instance;
			_contextsCache = new MappingOptionsFactoryCache<MatchingContext>(options => {
				var overrideOptions = options.GetOptions<MatcherOverrideMappingOptions>();
				return new MatchingContext(
					overrideOptions?.ServiceProvider ?? serviceProvider,
					overrideOptions?.Matcher ?? this,
					this,
					options
				);
			});
		}


		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return _configuration.TryGetDoubleMapCustomMatch<MatchingContext>((sourceType, destinationType), m =>
				m.Key.From.IsAssignableFrom(sourceType) &&
				m.Key.To.IsAssignableFrom(destinationType), out _);
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (!_configuration.TryGetDoubleMapCustomMatch<MatchingContext>((sourceType, destinationType), m =>
				m.Key.From.IsAssignableFrom(sourceType) &&
				m.Key.To.IsAssignableFrom(destinationType), out var map)) {

				throw new MapNotFoundException((sourceType, destinationType));
			}

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			var context = _contextsCache.GetOrCreate(mappingOptions);

			try {
				return (bool)map.Invoke(source, destination, context)!;
			}
			catch (MappingException e) {
				throw new MatcherException(e.InnerException!, (sourceType, destinationType));
			}
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if(!_configuration.TryGetDoubleMapCustomMatch<MatchingContext>((sourceType, destinationType), m =>
				m.Key.From.IsAssignableFrom(sourceType) &&
				m.Key.To.IsAssignableFrom(destinationType), out var map)) {

				throw new MapNotFoundException((sourceType, destinationType));
			}

			var context = _contextsCache.GetOrCreate(mappingOptions);

			return new DefaultMatchMapFactory(sourceType, destinationType, (source, destination) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));
				TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

				try {
					return (bool)map.Invoke(source, destination, context)!;
				}
				catch (MappingException e) {
					throw new MatcherException(e.InnerException!, (sourceType, destinationType));
				}
			});
		}
	}
}
