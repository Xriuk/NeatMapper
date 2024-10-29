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
		/// Configuration for <see cref="IAsyncNewMap{TSource, TDestination}"/> (and the static version) classes and
		/// <see cref="CustomAsyncNewAdditionalMapsOptions"/> additional maps for the mapper.
		/// </summary>
		internal readonly CustomMapsConfiguration _newMapsConfiguration;

		/// <<summary>
		/// Configuration for <see cref="IAsyncMergeMap{TSource, TDestination}"/> (and the static version) classes and
		/// <see cref="CustomAsyncMergeAdditionalMapsOptions"/> additional maps for the mapper.
		/// </summary>
		internal readonly CustomMapsConfiguration _mergeMapsConfiguration;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> and output <see cref="AsyncMappingContextOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<AsyncMappingContextOptions> _contextsOptionsCache;


		/// <summary>
		/// Creates a new instance of <see cref="AsyncCustomMapper"/>.<br/>
		/// At least one between <paramref name="mapsOptions"/>, <paramref name="additionalNewMapsOptions"/>
		/// and <paramref name="additionalMergeMapsOptions"/> should be specified.
		/// </summary>
		/// <param name="mapsOptions">Options to retrieve user-defined maps for the mapper, null to ignore.</param>
		/// <param name="additionalNewMapsOptions">Additional user-defined maps for the mapper, null to ignore.</param>
		/// <param name="additionalMergeMapsOptions">Additional user-defined maps for the mapper, null to ignore.</param>
		/// <param name="serviceProvider">
		/// Service provider to be passed to the maps inside <see cref="AsyncMappingContext"/>, 
		/// null to pass an empty service provider.<br/>
		/// Can be overridden during mapping with <see cref="AsyncMapperOverrideMappingOptions"/>.
		/// </param>
		public AsyncCustomMapper(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomMapsOptions?
#else
			CustomMapsOptions
#endif
			mapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomAsyncNewAdditionalMapsOptions?
#else
			CustomAsyncNewAdditionalMapsOptions
#endif
			additionalNewMapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomAsyncMergeAdditionalMapsOptions?
#else
			CustomAsyncMergeAdditionalMapsOptions
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
					return type == typeof(IAsyncNewMap<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(IAsyncNewMapStatic<,>)
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
					return type == typeof(IAsyncMergeMap<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(IAsyncMergeMapStatic<,>)
#endif
					;
				},
				(mapsOptions ?? new CustomMapsOptions()).TypesToScan,
				additionalMergeMapsOptions?._maps.Values
			);
			_contextsOptionsCache = new MappingOptionsFactoryCache<AsyncMappingContextOptions>(options => {
				var overrideOptions = options.GetOptions<AsyncMapperOverrideMappingOptions>();
				return new AsyncMappingContextOptions(
					overrideOptions?.ServiceProvider ?? serviceProvider ?? EmptyServiceProvider.Instance,
					overrideOptions?.Mapper ?? this,
					this,
					options
				);
			});
		}


		#region IAsyncMapper methods
		public bool CanMapAsyncNew(
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

			return _newMapsConfiguration.TryGetSingleMapAsync((sourceType, destinationType), out _) ||
				(ObjectFactory.CanCreate(destinationType) && CanMapAsyncMerge(sourceType, destinationType, mappingOptions));
		}

		public bool CanMapAsyncMerge(
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

			return _mergeMapsConfiguration.TryGetDoubleMapAsync((sourceType, destinationType), out _);
		}

		public Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> MapAsync(
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
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			if (!_newMapsConfiguration.TryGetSingleMapAsync((sourceType, destinationType), out var map)) {
				// Forward new map to merge by creating a destination
				if (ObjectFactory.CanCreate(destinationType))
					return MapAsync(source, sourceType, ObjectFactory.Create(destinationType), destinationType, mappingOptions, cancellationToken);
				else
					throw new MapNotFoundException((sourceType, destinationType));
			}

			var contextOptions = _contextsOptionsCache.GetOrCreate(mappingOptions);

			// Not checking the returned type, so that we save an async/await state machine
			return map.Invoke(source, new AsyncMappingContext(contextOptions, cancellationToken));
		}

		public Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> MapAsync(
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
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (!_mergeMapsConfiguration.TryGetDoubleMapAsync((sourceType, destinationType), out var map))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			var contextOptions = _contextsOptionsCache.GetOrCreate(mappingOptions);

			// Not checking the returned type, so that we save an async/await state machine
			return map.Invoke(source, destination, new AsyncMappingContext(contextOptions, cancellationToken));
		}
		#endregion

		#region IAsyncMapperFactory methods
		public IAsyncNewMapFactory MapAsyncNewFactory(
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

			if (!_newMapsConfiguration.TryGetSingleMapAsync((sourceType, destinationType), out var map))
				return MapAsyncMergeFactory(sourceType, destinationType, mappingOptions).MapAsyncNewFactory();

			var contextOptions = _contextsOptionsCache.GetOrCreate(mappingOptions);

			// Not checking the returned type, so that we save an async/await state machine
			return new DefaultAsyncNewMapFactory(sourceType, destinationType, (source, cancellationToken) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));

				return map.Invoke(source, new AsyncMappingContext(contextOptions, cancellationToken));
			});
		}

		public IAsyncMergeMapFactory MapAsyncMergeFactory(
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

			if (!_mergeMapsConfiguration.TryGetDoubleMapAsync((sourceType, destinationType), out var map))
				throw new MapNotFoundException((sourceType, destinationType));

			var contextOptions = _contextsOptionsCache.GetOrCreate(mappingOptions);

			return new DefaultAsyncMergeMapFactory(sourceType, destinationType, (source, destination, cancellationToken) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));
				TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

				// Not checking the returned type, so that we save an async/await state machine
				return map.Invoke(source, destination, new AsyncMappingContext(contextOptions, cancellationToken));
			});
		}
		#endregion

		#region IAsyncMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetAsyncNewMaps(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return _newMapsConfiguration.GetMaps();
		}

		public IEnumerable<(Type From, Type To)> GetAsyncMergeMaps(
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
