using System;
using System.Collections.Generic;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which maps objects by using <see cref="INewMap{TSource, TDestination}"/>,
	/// <see cref="IMergeMap{TSource, TDestination}"/> (and their static counterparts) and any additional map.
	/// Caches <see cref="MappingContext"/> for each provided <see cref="MappingOptions"/>, so that same options
	/// will reuse the same context.
	/// </summary>
	public sealed class CustomMapper : IMapper, IMapperFactory, IMapperMaps {
		/// <summary>
		/// Configuration for <see cref="INewMap{TSource, TDestination}"/> (and the static version) classes and
		/// <see cref="CustomNewAdditionalMapsOptions"/> additional maps for the mapper.
		/// </summary>
		internal readonly CustomMapsConfiguration _newMapsConfiguration;

		/// <summary>
		/// Configuration for <see cref="IMergeMap{TSource, TDestination}"/> (and the static version) classes and
		/// <see cref="CustomMergeAdditionalMapsOptions"/> additional maps for the mapper.
		/// </summary>
		internal readonly CustomMapsConfiguration _mergeMapsConfiguration;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> and output <see cref="MappingContext"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingContext> _contextsCache;


		/// <summary>
		/// Creates a new instance of <see cref="CustomMapper"/>.<br/>
		/// At least one between <paramref name="mapsOptions"/>, <paramref name="additionalNewMapsOptions"/>
		/// and <paramref name="additionalMergeMapsOptions"/> should be specified.
		/// </summary>
		/// <param name="mapsOptions">Options to retrieve user-defined maps for the mapper, null to ignore.</param>
		/// <param name="additionalNewMapsOptions">Additional user-defined maps for the mapper, null to ignore.</param>
		/// <param name="additionalMergeMapsOptions">Additional user-defined maps for the mapper, null to ignore.</param>
		/// <param name="serviceProvider">
		/// Service provider to be passed to the maps inside <see cref="MappingContext"/>, 
		/// null to pass an empty service provider.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions.ServiceProvider"/>.
		/// </param>
		public CustomMapper(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomMapsOptions?
#else
			CustomMapsOptions
#endif
			mapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomNewAdditionalMapsOptions?
#else
			CustomNewAdditionalMapsOptions
#endif
			additionalNewMapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomMergeAdditionalMapsOptions?
#else
			CustomMergeAdditionalMapsOptions
#endif
			additionalMergeMapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) {

			_newMapsConfiguration = new CustomMapsConfiguration(
				(_, i) => {
					if (!i.IsGenericType)
						return false;
					var type = i.GetGenericTypeDefinition();
					return type == typeof(INewMap<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(INewMapStatic<,>)
#endif
					;
				},
				(mapsOptions ?? new CustomMapsOptions()).TypesToScan,
				additionalNewMapsOptions?._maps.Values
			);
			_mergeMapsConfiguration = new CustomMapsConfiguration(
				(_, i) => {
					if (!i.IsGenericType)
						return false;
					var type = i.GetGenericTypeDefinition();
					return type == typeof(IMergeMap<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(IMergeMapStatic<,>)
#endif
					;
				},
				(mapsOptions ?? new CustomMapsOptions()).TypesToScan,
				additionalMergeMapsOptions?._maps.Values
			);
			_contextsCache = new MappingOptionsFactoryCache<MappingContext>(options => {
				var overrideOptions = options.GetOptions<MapperOverrideMappingOptions>();
				return new MappingContext(
					overrideOptions?.ServiceProvider ?? serviceProvider ?? EmptyServiceProvider.Instance,
					overrideOptions?.Mapper ?? this,
					this,
					options
				);
			});
		}


		#region IMapper methods
		public bool CanMapNew(
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

			return _newMapsConfiguration.TryGetSingleMap<MappingContext>((sourceType, destinationType), out _) ||
				(ObjectFactory.CanCreate(destinationType) && CanMapMerge(sourceType, destinationType, mappingOptions));
		}

		public bool CanMapMerge(
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

			return _mergeMapsConfiguration.TryGetDoubleMap<MappingContext>((sourceType, destinationType), out _);
		}

		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			Map(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
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

			if (!_newMapsConfiguration.TryGetSingleMap<MappingContext>((sourceType, destinationType), out var map)) {
				// Forward new map to merge by creating a destination
				if (ObjectFactory.CanCreate(destinationType))
					return Map(source, sourceType, ObjectFactory.Create(destinationType), destinationType, mappingOptions);
				else
					throw new MapNotFoundException((sourceType, destinationType));
			}

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			var context = _contextsCache.GetOrCreate(mappingOptions);

			var result = map.Invoke(source, context);

			// Should not happen
			TypeUtils.CheckObjectType(result, destinationType);

			return result;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			Map(
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (!_mergeMapsConfiguration.TryGetDoubleMap<MappingContext>((sourceType, destinationType), out var map))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			var context = _contextsCache.GetOrCreate(mappingOptions);

			var result = map.Invoke(source, destination, context);

			// Should not happen
			TypeUtils.CheckObjectType(result, destinationType);

			return result;
		}
		#endregion

		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(
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

			if (!_newMapsConfiguration.TryGetSingleMap<MappingContext>((sourceType, destinationType), out var map))
				return MapMergeFactory(sourceType, destinationType, mappingOptions).MapNewFactory();

			var context = _contextsCache.GetOrCreate(mappingOptions);

			return new DefaultNewMapFactory(
				sourceType, destinationType,
				source => {
					TypeUtils.CheckObjectType(source, sourceType, nameof(source));

					var result = map.Invoke(source, context);

					// Should not happen
					TypeUtils.CheckObjectType(result, destinationType);

					return result;
				});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public IMergeMapFactory MapMergeFactory(
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

			if (!_mergeMapsConfiguration.TryGetDoubleMap<MappingContext>((sourceType, destinationType), out var map))
				throw new MapNotFoundException((sourceType, destinationType));

			var context = _contextsCache.GetOrCreate(mappingOptions);

			return new DefaultMergeMapFactory(
				sourceType, destinationType,
				(source, destination) => {
					TypeUtils.CheckObjectType(source, sourceType, nameof(source));
					TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

					var result = map.Invoke(source, destination, context);

					// Should not happen
					TypeUtils.CheckObjectType(result, destinationType);

					return result;
				});
		}
		#endregion

		#region IMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetNewMaps(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return _newMapsConfiguration.GetMaps();
		}

		public IEnumerable<(Type From, Type To)> GetMergeMaps(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return _mergeMapsConfiguration.GetMaps();
		}
		#endregion
	}
}
