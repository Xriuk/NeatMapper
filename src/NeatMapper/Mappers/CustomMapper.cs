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
		/// Configuration for <see cref="ICanMapNew{TSource, TDestination}"/> (and the static version) classes
		/// for the mapper.
		/// </summary>
		internal readonly CustomMapsConfiguration _canMapNewConfiguration;

		/// <summary>
		/// Configuration for <see cref="INewMap{TSource, TDestination}"/> (and the static version) classes and
		/// <see cref="CustomNewAdditionalMapsOptions"/> additional maps for the mapper.
		/// </summary>
		internal readonly CustomMapsConfiguration _newMapsConfiguration;

		/// <summary>
		/// Configuration for <see cref="ICanMapMerge{TSource, TDestination}"/> (and the static version) classes
		/// for the mapper.
		/// </summary>
		internal readonly CustomMapsConfiguration _canMapMergeConfiguration;

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
		/// Creates a new instance of <see cref="CustomMapper"/>.
		/// </summary>
		/// <param name="mapsOptions">Options to retrieve user-defined maps for the mapper, null to ignore.</param>
		/// <param name="additionalNewMapsOptions">Additional user-defined maps for the mapper, null to ignore.</param>
		/// <param name="additionalMergeMapsOptions">Additional user-defined maps for the mapper, null to ignore.</param>
		/// <param name="serviceProvider">
		/// Service provider to be passed to the maps inside <see cref="MappingContext"/>, 
		/// null to pass an empty service provider.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions.ServiceProvider"/>.
		/// </param>
		/// <remarks>
		/// At least one between <paramref name="mapsOptions"/>, <paramref name="additionalNewMapsOptions"/>
		/// and <paramref name="additionalMergeMapsOptions"/> should be specified.
		/// </remarks>
		public CustomMapper(
			CustomMapsOptions? mapsOptions = null,
			CustomNewAdditionalMapsOptions? additionalNewMapsOptions = null,
			CustomMergeAdditionalMapsOptions? additionalMergeMapsOptions = null,
			IServiceProvider? serviceProvider = null) {

			var typesToScan = (mapsOptions ?? new CustomMapsOptions()).TypesToScan;
			_canMapNewConfiguration = new CustomMapsConfiguration(
				(_, i) => {
					if (!i.IsGenericType)
						return false;
					var type = i.GetGenericTypeDefinition();
					return type == typeof(ICanMapNew<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(ICanMapNewStatic<,>)
#endif
					;
				},
				typesToScan,
				additionalNewMapsOptions?._canMaps.Values);
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
				typesToScan,
				additionalNewMapsOptions?._maps.Values);
			_canMapMergeConfiguration = new CustomMapsConfiguration(
				(_, i) => {
					if (!i.IsGenericType)
						return false;
					var type = i.GetGenericTypeDefinition();
					return type == typeof(ICanMapMerge<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(ICanMapMergeStatic<,>)
#endif
					;
				},
				typesToScan,
				additionalMergeMapsOptions?._canMaps.Values);
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
				typesToScan,
				additionalMergeMapsOptions?._maps.Values);
			serviceProvider ??= EmptyServiceProvider.Instance;
			_contextsCache = new MappingOptionsFactoryCache<MappingContext>(options => {
				var overrideOptions = options.GetOptions<MapperOverrideMappingOptions>();
				return new MappingContext(
					overrideOptions?.ServiceProvider ?? serviceProvider,
					overrideOptions?.Mapper ?? this,
					this,
					options
				);
			});
		}


		#region IMapper methods
		public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapNewInternal(sourceType, destinationType, mappingOptions, out _, out _) ||
				(!sourceType.IsGenericTypeDefinition && !destinationType.IsGenericTypeDefinition &&
					ObjectFactory.CanCreate(destinationType) && CanMapMerge(sourceType, destinationType, mappingOptions));
		}

		public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapMergeInternal(sourceType, destinationType, mappingOptions, out _, out _);
		}

		public object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if(!CanMapNewInternal(sourceType, destinationType, mappingOptions, out var map, out var context) || map == null) {
				// Forward new map to merge by creating a destination
				if (!sourceType.IsGenericTypeDefinition && !destinationType.IsGenericTypeDefinition && ObjectFactory.CanCreate(destinationType))
					return Map(source, sourceType, ObjectFactory.Create(destinationType), destinationType, mappingOptions);
				else
					throw new MapNotFoundException((sourceType, destinationType));
			}

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			var result = map.Invoke(source, context);

			// Should not happen
			TypeUtils.CheckObjectType(result, destinationType);

			return result;
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapMergeInternal(sourceType, destinationType, mappingOptions, out var map, out var context) || map == null)
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			var result = map.Invoke(source, destination, context);

			// Should not happen
			TypeUtils.CheckObjectType(result, destinationType);

			return result;
		}
		#endregion

		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapNewInternal(sourceType, destinationType, mappingOptions, out var map, out var context) || map == null)
				return MapMergeFactory(sourceType, destinationType, mappingOptions).MapNewFactory();

			return new DefaultNewMapFactory(
				sourceType, destinationType,
				source => {
					TypeUtils.CheckObjectType(source, sourceType, nameof(source));

					var result = map.Invoke(source, context);

					// Should not happen
					TypeUtils.CheckObjectType(result, destinationType);

					return result;
				});
		}

		public IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapMergeInternal(sourceType, destinationType, mappingOptions, out var map, out var context) || map == null)
				throw new MapNotFoundException((sourceType, destinationType));

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
		public IEnumerable<(Type From, Type To)> GetNewMaps(MappingOptions? mappingOptions = null) {
			return _newMapsConfiguration.GetMaps();
		}

		public IEnumerable<(Type From, Type To)> GetMergeMaps(MappingOptions? mappingOptions = null) {
			return _mergeMapsConfiguration.GetMaps();
		}
		#endregion


		private bool CanMapNewInternal(
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions,
			out Func<object?, MappingContext, object?> map,
			out MappingContext context) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition) { 
				map = null!;
				context = null!;

				return _newMapsConfiguration.HasOpenGenericMap((sourceType, destinationType));
			}
			else if (_newMapsConfiguration.TryGetSingleMap<MappingContext>((sourceType, destinationType), out map)) {
				context = _contextsCache.GetOrCreate(mappingOptions);

				if (_canMapNewConfiguration.TryGetContextMap<MappingContext>((sourceType, destinationType), out var canMapNew))
					return (bool)canMapNew.Invoke(context)!;
				else
					return true;
			}
			else {
				context = null!;
				return false;
			}
		}

		private bool CanMapMergeInternal(
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions,
			out Func<object?, object?, MappingContext, object?> map,
			out MappingContext context) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition) {
				map = null!;
				context = null!;

				return _mergeMapsConfiguration.HasOpenGenericMap((sourceType, destinationType));
			}
			if (_mergeMapsConfiguration.TryGetDoubleMap<MappingContext>((sourceType, destinationType), out map)) {
				context = _contextsCache.GetOrCreate(mappingOptions);

				if (_canMapMergeConfiguration.TryGetContextMap<MappingContext>((sourceType, destinationType), out var canMapMerge))
					return (bool)canMapMerge.Invoke(context)!;
				else
					return true;
			}
			else {
				context = null!;
				return false;
			}
		}
	}
}
