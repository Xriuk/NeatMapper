using Microsoft.Extensions.Options;
using System;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches objects by using <see cref="IMatchMap{TSource, TDestination}"/>.
	/// </summary>
	public sealed class CustomMatcher : IMatcher, IMatcherCanMatch, IMatcherFactory {
		readonly CustomMapsConfiguration _configuration;
		readonly IServiceProvider _serviceProvider;

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

			try {
				// Try retrieving a regular map
				_configuration.GetMap((sourceType, destinationType));
				return true;
			}
			catch (MapNotFoundException) {
				return false;
			}
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

			var comparer = _configuration.GetMap(types);
			var overrideOptions = mappingOptions?.GetOptions<MatcherOverrideMappingOptions>();
			var parameters = new object[] { null, null, new MatchingContext(
				overrideOptions?.ServiceProvider ?? _serviceProvider,
				overrideOptions?.Matcher ?? this,
				mappingOptions ?? MappingOptions.Empty
			) };

			return (source, destination) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));
				TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

				parameters[0] = source;
				parameters[1] = destination;
				try {
					return (bool)comparer.Invoke(parameters);
				}
				catch (MappingException e) {
					throw new MatcherException(e.InnerException, types);
				}
			};

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
