using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which delegates mapping to other <see cref="IAsyncMapper"/>s,
	/// this allows to combine different mapping capabilities.<br/>
	/// Each mapper is invoked in order and the first one to succeed in mapping is returned.<br/>
	/// For new maps, if no mapper can map the types a destination object is created and merge maps are tried.
	/// </summary>
	public sealed class AsyncCompositeMapper : IAsyncMapper, IAsyncMapperCanMap {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private readonly IList<IAsyncMapper> _mappers;
		private readonly AsyncNestedMappingContext _nestedMappingContext;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif

		/// <summary>
		/// Creates the mapper by using the provided mappers list.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public AsyncCompositeMapper(params IAsyncMapper[] mappers) : this((IList<IAsyncMapper>) mappers) { }

		/// <summary>
		/// Creates the mapper by using the provided mappers list.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public AsyncCompositeMapper(IList<IAsyncMapper> mappers) {
			if (mappers == null)
				throw new ArgumentNullException(nameof(mappers));

			_mappers = new List<IAsyncMapper>(mappers);
			_nestedMappingContext = new AsyncNestedMappingContext(this);
		}


		#region IAsyncMapper methods
		public async Task<
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			// Try new map
			foreach (var mapper in _mappers) {
				try {
					return await mapper.MapAsync(source, sourceType, destinationType, mappingOptions, cancellationToken);
				}
				catch (MapNotFoundException) { }
			}

			// Try creating a destination and forward to merge map
			object destination;
			try {
				destination = ObjectFactory.Create(destinationType);
			}
			catch (ObjectCreationException) {
				throw new MapNotFoundException((sourceType, destinationType));
			}

			return await MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public async Task<
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			foreach (var mapper in _mappers) {
				try {
					return await mapper.MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken);
				}
				catch (MapNotFoundException) { }
			}

			throw new MapNotFoundException((sourceType, destinationType));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region IAsyncMapperCanMap methods
		public async Task<bool> CanMapAsyncNew(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			// Check if any mapper implements IAsyncMapperCanMap, if one of them throws it means that the map can be checked only when mapping
			var undeterminateMappers = new List<IAsyncMapper>();
			foreach (var mapper in _mappers.OfType<IAsyncMapperCanMap>()) {
				try { 
					if(await mapper.CanMapAsyncNew(sourceType, destinationType, mappingOptions, cancellationToken))
						return true;
				}
				catch (InvalidOperationException) {
					undeterminateMappers.Add(mapper);
				}
			}

			// Try creating a default source object and try mapping it
			var mappersLeft = _mappers.Where(m => !(m is IAsyncMapperCanMap) || undeterminateMappers.IndexOf(m) != -1);
			if (mappersLeft.Any()) {
				object source;
				try {
					source = ObjectFactory.GetOrCreateCached(sourceType) ?? throw new Exception(); // Just in case
				}
				catch {
					throw new InvalidOperationException("Cannot verify if the mapper supports the given map because unable to create an object to test it");
				}

				foreach (var mapper in mappersLeft) {
					try {
						await mapper.MapAsync(source, sourceType, destinationType, mappingOptions, cancellationToken);
						return true;
					}
					catch (MapNotFoundException) {}
				}
			}

			if(undeterminateMappers.Count > 0)
				throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
			else
				return false;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public async Task<bool> CanMapAsyncMerge(
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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			// Check if any mapper implements IAsyncMapperCanMap, if one of them throws it means that the map can be checked only when mapping
			var undeterminateMappers = new List<IAsyncMapper>();
			foreach (var mapper in _mappers.OfType<IAsyncMapperCanMap>()) {
				try {
					if (await mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions, cancellationToken))
						return true;
				}
				catch (InvalidOperationException) {
					undeterminateMappers.Add(mapper);
				}
			}

			// Try creating two default source and destination objects and try mapping them
			var mappersLeft = _mappers.Where(m => !(m is IAsyncMapperCanMap) || undeterminateMappers.IndexOf(m) != -1);
			if (mappersLeft.Any()) {
				object source;
				object destination;
				try {
					source = ObjectFactory.GetOrCreateCached(sourceType) ?? throw new Exception(); // Just in case
					destination = ObjectFactory.Create(destinationType) ?? throw new Exception(); // Just in case
				}
				catch {
					throw new InvalidOperationException("Cannot verify if the mapper supports the given map because unable to create the objects to test it");
				}

				foreach (var mapper in mappersLeft) {
					try {
						await mapper.MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken);
						return true;
					}
					catch (MapNotFoundException) { }
				}
			}

			if (undeterminateMappers.Count > 0)
				throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
			else
				return false;
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		// Will override a mapper if not already overridden
		MappingOptions MergeOrCreateMappingOptions(MappingOptions options) {
			return (options ?? MappingOptions.Empty).ReplaceOrAdd<AsyncMapperOverrideMappingOptions, AsyncNestedMappingContext>(
				m => m?.Mapper != null ? m : new AsyncMapperOverrideMappingOptions(this, m?.ServiceProvider),
				n => n != null ? new AsyncNestedMappingContext(this, n) : _nestedMappingContext);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
