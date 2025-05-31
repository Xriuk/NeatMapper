using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which maps objects by using <see cref="IAsyncNewMap{TSource, TDestination}"/>,
	/// <see cref="IAsyncMergeMap{TSource, TDestination}"/> (and their static counterparts) and any additional map.
	/// Caches <see cref="AsyncMappingContextOptions"/> for each provided <see cref="MappingOptions"/>,
	/// so that same options will reuse the same context.
	/// </summary>
	public sealed class AsyncCustomMapper : IAsyncMapper, IAsyncMapperFactory, IAsyncMapperMaps {
		/// <<summary>
		/// Configuration for <see cref="ICanMapAsyncNew{TSource, TDestination}"/> (and the static version) classes
		/// for the mapper.
		/// </summary>
		internal readonly CustomMapsConfiguration _canMapNewConfiguration;

		/// <<summary>
		/// Configuration for <see cref="IAsyncNewMap{TSource, TDestination}"/> (and the static version) classes and
		/// <see cref="CustomAsyncNewAdditionalMapsOptions"/> additional maps for the mapper.
		/// </summary>
		internal readonly CustomMapsConfiguration _newMapsConfiguration;

		/// <<summary>
		/// Configuration for <see cref="ICanMapAsyncMerge{TSource, TDestination}"/> (and the static version) classes
		/// for the mapper.
		/// </summary>
		internal readonly CustomMapsConfiguration _canMapMergeConfiguration;

		/// <<summary>
		/// Configuration for <see cref="IAsyncMergeMap{TSource, TDestination}"/> (and the static version) classes and
		/// <see cref="CustomAsyncMergeAdditionalMapsOptions"/> additional maps for the mapper.
		/// </summary>
		internal readonly CustomMapsConfiguration _mergeMapsConfiguration;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> and output <see cref="AsyncMappingContextOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<AsyncMappingContextOptions> _contextOptionsCache;


		/// <summary>
		/// Creates a new instance of <see cref="AsyncCustomMapper"/>.
		/// </summary>
		/// <param name="mapsOptions">Options to retrieve user-defined maps for the mapper, null to ignore.</param>
		/// <param name="additionalNewMapsOptions">Additional user-defined maps for the mapper, null to ignore.</param>
		/// <param name="additionalMergeMapsOptions">Additional user-defined maps for the mapper, null to ignore.</param>
		/// <param name="serviceProvider">
		/// Service provider to be passed to the maps inside <see cref="AsyncMappingContext"/>, 
		/// null to pass an empty service provider.<br/>
		/// Can be overridden during mapping with <see cref="AsyncMapperOverrideMappingOptions"/>.
		/// </param>
		/// <remarks>
		/// At least one between <paramref name="mapsOptions"/>, <paramref name="additionalNewMapsOptions"/>
		/// and <paramref name="additionalMergeMapsOptions"/> should be specified.
		/// </remarks>
		public AsyncCustomMapper(
			CustomMapsOptions? mapsOptions = null,
			CustomAsyncNewAdditionalMapsOptions? additionalNewMapsOptions = null,
			CustomAsyncMergeAdditionalMapsOptions? additionalMergeMapsOptions = null,
			IServiceProvider? serviceProvider = null) {

			var typesToScan = (mapsOptions ?? new CustomMapsOptions()).TypesToScan;
			_canMapNewConfiguration = new CustomMapsConfiguration(
				(_, i) => {
					if (!i.IsGenericType)
						return false;
					var type = i.GetGenericTypeDefinition();
					return type == typeof(ICanMapAsyncNew<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(ICanMapAsyncNewStatic<,>)
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
					return type == typeof(IAsyncNewMap<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(IAsyncNewMapStatic<,>)
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
					return type == typeof(ICanMapAsyncMerge<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(ICanMapAsyncMergeStatic<,>)
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
					return type == typeof(IAsyncMergeMap<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(IAsyncMergeMapStatic<,>)
#endif
					;
				},
				typesToScan,
				additionalMergeMapsOptions?._maps.Values);
			serviceProvider ??= EmptyServiceProvider.Instance;
			_contextOptionsCache = new MappingOptionsFactoryCache<AsyncMappingContextOptions>(options => {
				var overrideOptions = options.GetOptions<AsyncMapperOverrideMappingOptions>();
				return new AsyncMappingContextOptions(
					overrideOptions?.ServiceProvider ?? serviceProvider,
					overrideOptions?.Mapper ?? this,
					this,
					options
				);
			});
		}


		#region IAsyncMapper methods
		public bool CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapAsyncNewInternal(sourceType, destinationType, mappingOptions, out _, out _) ||
				(ObjectFactory.CanCreate(destinationType) && CanMapAsyncMerge(sourceType, destinationType, mappingOptions));
		}

		public bool CanMapAsyncMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapAsyncMergeInternal(sourceType, destinationType, mappingOptions, out _, out _);
		}

		public Task<object?> MapAsync(
			object? source,
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (!CanMapAsyncNewInternal(sourceType, destinationType, mappingOptions, out var map, out var contextOptions) || map == null) {
				// Forward new map to merge by creating a destination
				if (!sourceType.IsGenericTypeDefinition && !destinationType.IsGenericTypeDefinition && ObjectFactory.CanCreate(destinationType))
					return MapAsync(source, sourceType, ObjectFactory.Create(destinationType), destinationType, mappingOptions, cancellationToken);
				else
					throw new MapNotFoundException((sourceType, destinationType));
			}

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			// Not checking the returned type, so that we save an async/await state machine
			return map.Invoke(source, new AsyncMappingContext(contextOptions, cancellationToken));
		}

		public Task<object?> MapAsync(
			object? source,
			Type sourceType,
			object? destination,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (!CanMapAsyncMergeInternal(sourceType, destinationType, mappingOptions, out var map, out var contextOptions) || map == null)
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			// Not checking the returned type, so that we save an async/await state machine
			return map.Invoke(source, destination, new AsyncMappingContext(contextOptions, cancellationToken));
		}
		#endregion

		#region IAsyncMapperFactory methods
		public IAsyncNewMapFactory MapAsyncNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapAsyncNewInternal(sourceType, destinationType, mappingOptions, out var map, out var contextOptions) || map == null)
				return MapAsyncMergeFactory(sourceType, destinationType, mappingOptions).MapAsyncNewFactory();

			// Not checking the returned type, so that we save an async/await state machine
			return new DefaultAsyncNewMapFactory(sourceType, destinationType, (source, cancellationToken) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));

				return map.Invoke(source, new AsyncMappingContext(contextOptions, cancellationToken));
			});
		}

		public IAsyncMergeMapFactory MapAsyncMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapAsyncMergeInternal(sourceType, destinationType, mappingOptions, out var map, out var contextOptions) || map == null)
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultAsyncMergeMapFactory(sourceType, destinationType, (source, destination, cancellationToken) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));
				TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

				// Not checking the returned type, so that we save an async/await state machine
				return map.Invoke(source, destination, new AsyncMappingContext(contextOptions, cancellationToken));
			});
		}
		#endregion

		#region IAsyncMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetAsyncNewMaps(MappingOptions? mappingOptions = null) {
			return _newMapsConfiguration.GetMaps();
		}

		public IEnumerable<(Type From, Type To)> GetAsyncMergeMaps(MappingOptions? mappingOptions = null) {
			return _mergeMapsConfiguration.GetMaps();
		}
		#endregion


		private bool CanMapAsyncNewInternal(
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions,
			out Func<object?, AsyncMappingContext, Task<object?>> map,
			out AsyncMappingContextOptions contextOptions) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition) {
				map = null!;
				contextOptions = null!;

				return _newMapsConfiguration.HasOpenGenericMap((sourceType, destinationType));
			}
			else if (_newMapsConfiguration.TryGetSingleMapAsync((sourceType, destinationType), out map)) {
				contextOptions = _contextOptionsCache.GetOrCreate(mappingOptions);

				if (_canMapNewConfiguration.TryGetContextMap<AsyncMappingContextOptions>((sourceType, destinationType), out var canMapNew))
					return (bool)canMapNew.Invoke(contextOptions)!;
				else
					return true;
			}
			else {
				contextOptions = null!;
				return false;
			}
		}

		private bool CanMapAsyncMergeInternal(
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions,
			out Func<object?, object?, AsyncMappingContext, Task<object?>> map,
			out AsyncMappingContextOptions contextOptions) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition) {
				map = null!;
				contextOptions = null!;

				return _mergeMapsConfiguration.HasOpenGenericMap((sourceType, destinationType));
			}
			else if (_mergeMapsConfiguration.TryGetDoubleMapAsync((sourceType, destinationType), out map)) {
				contextOptions = _contextOptionsCache.GetOrCreate(mappingOptions);

				if (_canMapMergeConfiguration.TryGetContextMap<AsyncMappingContextOptions>((sourceType, destinationType), out var canMapMerge))
					return (bool)canMapMerge.Invoke(contextOptions)!;
				else
					return true;
			}
			else {
				contextOptions = null!;
				return false;
			}
		}
	}
}
