using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Reflection;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches objects by using <see cref="IHierarchyMatchMap{TSource, TDestination}"/>.
	/// </summary>
	public sealed class HierarchyCustomMatcher : IMatcher, IMatcherCanMatch, IMatcherFactory {
		private readonly CustomMapsConfiguration _configuration;
		private readonly IServiceProvider _serviceProvider;

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

			return MatchFactory(sourceType, destinationType, mappingOptions).Invoke(source, destination);
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

		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?, object?, bool
#else
			object, object, bool
#endif
			> MatchFactory(
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

			(Type From, Type To) types = (sourceType, destinationType);

			KeyValuePair<(Type From, Type To), CustomMap> map;
			try {
				map = _configuration.Maps.First(m =>
					m.Key.From.IsAssignableFrom(types.From) &&
					m.Key.To.IsAssignableFrom(types.To));
			}
			catch {
				throw new MapNotFoundException(types);
			}

			var overrideOptions = mappingOptions?.GetOptions<MatcherOverrideMappingOptions>();
			var parameters = new object[] { null, null, new MatchingContext(
				overrideOptions?.ServiceProvider ?? _serviceProvider,
				overrideOptions?.Matcher ?? this,
				mappingOptions ?? MappingOptions.Empty
			) };
			var instance = map.Value.Method.IsStatic ?
				null :
				(map.Value.Instance ?? ObjectFactory.GetOrCreateCached(map.Value.Method.DeclaringType));

			return (source, destination) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));
				TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

				parameters[0] = source;
				parameters[1] = destination;
				try {
					return (bool)map.Value.Method.Invoke(instance, parameters);
				}
				catch (TargetInvocationException e) {
					if (e.InnerException is TaskCanceledException)
						throw e.InnerException;
					else
						throw new MatcherException(e.InnerException, types);
				}
				catch (MapNotFoundException) {
					throw;
				}
				catch (TaskCanceledException) {
					throw;
				}
				catch (Exception e) {
					throw new MatcherException(e, types);
				}
			};

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
