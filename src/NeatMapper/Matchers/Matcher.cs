using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches objects by using <see cref="IMatchMap{TSource, TDestination}"/>
	/// </summary>
	public sealed class Matcher : IMatcher {
		readonly CustomMapsConfiguration _configuration;
		readonly CustomMapsConfiguration _hierarchyConfiguration;
		readonly IServiceProvider _serviceProvider;

		public Matcher(
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
				mapsOptions ?? new CustomMapsOptions(),
				additionalMapsOptions?._maps.Values);
			_hierarchyConfiguration = new CustomMapsConfiguration(
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
				mapsOptions ?? new CustomMapsOptions()
			);
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source != null && !sourceType.IsAssignableFrom(source.GetType()))
				throw new ArgumentException($"Object of type {source.GetType().FullName ?? source.GetType().Name} is not assignable to type {sourceType.FullName ?? sourceType.Name}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));
			if (destination != null && !destinationType.IsAssignableFrom(destination.GetType()))
				throw new ArgumentException($"Object of type {destination.GetType().FullName ?? destination.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}", nameof(destination));

			(Type From, Type To) types = (sourceType, destinationType);

			try {
				// Try retrieving a regular map
				var comparer = _configuration.GetMap(types);
				try {
					return (bool)comparer.Invoke(new object[] { source, destination, CreateMatchingContext(mappingOptions) });
				}
				catch (MappingException e) {
					throw new MatcherException(e.InnerException, types);
				}
			}
			catch (MapNotFoundException) {
				// Try retrieving a hierarchy map
				KeyValuePair<(Type From, Type To), CustomMap> map;
				try { 
					map = _hierarchyConfiguration.Maps.First(m =>
						m.Key.From.IsAssignableFrom(types.From) &&
						m.Key.To.IsAssignableFrom(types.To));
				}
				catch {
					throw new MapNotFoundException(types);
				}
				try {
					return (bool)map.Value.Method.Invoke(map.Value.Method.IsStatic ? null : map.Value.Instance ?? CustomMapsConfiguration.CreateOrReturnInstance(map.Value.Method.DeclaringType), new object[] { source, destination, CreateMatchingContext(mappingOptions) });
				}
				catch (Exception e) {
					throw new MatcherException(e, types);
				}
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		MatchingContext CreateMatchingContext(MappingOptions options) {
			var overrideOptions = options?.GetOptions<MatcherOverrideMappingOptions>();
			return new MatchingContext {
				Matcher = overrideOptions?.Matcher ?? this,
				ServiceProvider = overrideOptions?.ServiceProvider ?? _serviceProvider,
				MappingOptions = options ?? MappingOptions.Empty
			};
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
