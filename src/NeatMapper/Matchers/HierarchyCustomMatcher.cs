using System;
using System.Collections.Generic;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches objects by using <see cref="IHierarchyMatchMap{TSource, TDestination}"/>.
	/// </summary>
	public sealed class HierarchyCustomMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// Configuration for <see cref="ICanMatchHierarchy{TSource, TDestination}"/> (and the static version) classes
		/// for the matcher.
		/// </summary>
		private readonly CustomMapsConfiguration _canMatchConfiguration;

		/// <summary>
		/// Configuration for <see cref="IHierarchyMatchMap{TSource, TDestination}"/> (and the static version) classes and
		/// <see cref="CustomHierarchyMatchAdditionalMapsOptions"/> additional maps for the matcher.
		/// </summary>
		private readonly CustomMapsConfiguration _matchConfiguration;

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

			var typesToScan = (mapsOptions ?? new CustomMapsOptions()).TypesToScan;
			_canMatchConfiguration = new CustomMapsConfiguration(
				(t, i) => {
					// Hierarchy matchers do not support generic maps
					if (!i.IsGenericType || t.ContainsGenericParameters)
						return false;
					var type = i.GetGenericTypeDefinition();
					return type == typeof(ICanMatchHierarchy<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(ICanMatchHierarchyStatic<,>)
#endif
					;
				},
				typesToScan,
				additionalMapsOptions?._canMaps.Values);
			_matchConfiguration = new CustomMapsConfiguration(
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
				typesToScan,
				additionalMapsOptions?._maps.Values);
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
			return CanMatchInternal(sourceType, destinationType, mappingOptions, out _, out _);
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatchInternal(sourceType, destinationType, mappingOptions, out var map, out var context) || map == null) 
				throw new MapNotFoundException((sourceType, destinationType));
			
			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			try {
				return (bool)map.Invoke(source, destination, context)!;
			}
			catch (MappingException e) {
				throw new MatcherException(e.InnerException!, (sourceType, destinationType));
			}
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if(!CanMatchInternal(sourceType, destinationType, mappingOptions, out var map, out var context) || map == null)
				throw new MapNotFoundException((sourceType, destinationType));
			
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


		private bool CanMatchInternal(
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions,
			out Func<object?, object?, MatchingContext, object?> map,
			out MatchingContext context) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Does not support open generic types because IsAssignableFrom does not work with them
			if (_matchConfiguration.TryGetDoubleMapCustomMatch<MatchingContext>((sourceType, destinationType), Predicate, out map)) {
				context = _contextsCache.GetOrCreate(mappingOptions);

				if (_canMatchConfiguration.TryGetContextMapCustomMatch<MatchingContext>((sourceType, destinationType), Predicate, out var canMatch))
					return (bool)canMatch.Invoke(context)!;
				else
					return true;
			}
			else {
				context = null!;
				return false;
			}


			bool Predicate(KeyValuePair<(Type From, Type To), CustomMap> map) {
				return map.Key.From.IsAssignableFrom(sourceType) && map.Key.To.IsAssignableFrom(destinationType);
			}
		}
	}
}
