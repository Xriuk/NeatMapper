﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which maps objects by using <see cref="IAsyncMergeMap{TSource, TDestination}"/>.<br/>
	/// Supports both merge and new maps (by creating a destination object and forwarding the calls to merge map
	/// where possible).<br/>
	/// Caches <see cref="AsyncMappingContextOptions"/> for each provided <see cref="MappingOptions"/>, so that same options
	/// will share the same context.
	/// </summary>
	public sealed class AsyncMergeMapper : AsyncCustomMapper, IAsyncMapperFactory, IAsyncMapperMaps {
		/// <summary>
		/// Creates a new instance of <see cref="AsyncMergeMapper"/>.<br/>
		/// At least one between <paramref name="mapsOptions"/> and <paramref name="additionalMapsOptions"/>
		/// should be specified.
		/// </summary>
		/// <param name="mapsOptions">Options to retrieve user-defined maps for the mapper, null to ignore.</param>
		/// <param name="additionalMapsOptions">Additional user-defined maps for the mapper, null to ignore.</param>
		/// <param name="serviceProvider">
		/// Service provider to be passed to the maps inside <see cref="AsyncMappingContext"/>, 
		/// null to pass an empty service provider.<br/>
		/// Can be overridden during mapping with <see cref="AsyncMapperOverrideMappingOptions"/>.
		/// </param>
		public AsyncMergeMapper(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomMapsOptions?
#else
			CustomMapsOptions
#endif
			mapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomAsyncMergeAdditionalMapsOptions?
#else
			CustomAsyncMergeAdditionalMapsOptions
#endif
			additionalMapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) :

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			base(new CustomMapsConfiguration(
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
					additionalMapsOptions?._maps.Values
				),
				serviceProvider) { }

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


		#region IAsyncMapper methods
		override public Task<bool> CanMapAsyncNew(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			// Source type null checked in CanMapAsyncMerge
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (!ObjectFactory.CanCreate(destinationType))
				return Task.FromResult(false);

			return CanMapAsyncMerge(sourceType, destinationType, mappingOptions, cancellationToken);
		}

		override public Task<bool> CanMapAsyncMerge(
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

			try {
				_configuration.GetDoubleMapAsync((sourceType, destinationType));
				return Task.FromResult(true);
			}
			catch (MapNotFoundException) {
				return Task.FromResult(false);
			}
		}

		override public Task<
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

			// Forward new map to merge by creating a destination
			object destination;
			try {
				destination = ObjectFactory.Create(destinationType);
			}
			catch (ObjectCreationException) {
				throw new MapNotFoundException((sourceType, destinationType));
			}

			return MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken);
		}

		override public async Task<
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

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			var map = _configuration.GetDoubleMapAsync((sourceType, destinationType));
			var context = _contextsCache.GetOrCreate(mappingOptions);
			var result = await map.Invoke(source, destination, new AsyncMappingContext(context, cancellationToken));

			// Should not happen
			TypeUtils.CheckObjectType(result, destinationType);

			return result;
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

			// Forward new map to merge by creating a destination
			Func<object> destinationFactory;
			try {
				destinationFactory = ObjectFactory.CreateFactory(destinationType);
			}
			catch (ObjectCreationException) {
				throw new MapNotFoundException((sourceType, destinationType));
			}

			return MapAsyncMergeFactory(sourceType, destinationType, mappingOptions).MapAsyncNewFactory();
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

			var map = _configuration.GetDoubleMapAsync((sourceType, destinationType));
			var contextOptions = _contextsCache.GetOrCreate(mappingOptions);

			return new DefaultAsyncMergeMapFactory(sourceType, destinationType, async (source, destination, cancellationToken) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));
				TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

				var result = await map.Invoke(source, destination, new AsyncMappingContext(contextOptions, cancellationToken));

				// Should not happen
				TypeUtils.CheckObjectType(result, destinationType);

				return result;
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

			return GetAsyncMergeMaps().Where(m => ObjectFactory.CanCreate(m.To));
		}

		public IEnumerable<(Type From, Type To)> GetAsyncMergeMaps(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return _configuration.GetMaps();
		}
		#endregion
	}
}
