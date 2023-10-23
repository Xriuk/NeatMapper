using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which delegates mapping to other <see cref="IMapper"/>s, this allows to combine different mapping capabilities.<br/>
	/// Each mapper is invoked in order and the first one to succeed in mapping is returned
	/// </summary>
	public sealed class CompositeMapper : IMapper, IMapperCanMap {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		readonly IList<IMapper> _mappers;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif

		/// <summary>
		/// Creates the mapper by using the provided mappers list
		/// </summary>
		/// <param name="mappers">mappers to delegate the mapping to</param>
		public CompositeMapper(params IMapper[] mappers) : this((IList<IMapper>) mappers) { }

		/// <summary>
		/// Creates the mapper by using the provided mappers list
		/// </summary>
		/// <param name="mappers">mappers to delegate the mapping to</param>
		public CompositeMapper(IList<IMapper> mappers) {
			if (mappers == null)
				throw new ArgumentNullException(nameof(mappers));

			_mappers = new List<IMapper>(mappers);
		}


		#region IMapper methods
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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			foreach (var mapper in _mappers) {
				try {
					return mapper.Map(source, sourceType, destinationType, mappingOptions);
				}
				catch (MapNotFoundException) { }
			}

			throw new MapNotFoundException((sourceType, destinationType));

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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			foreach (var mapper in _mappers) {
				try {
					return mapper.Map(source, sourceType, destination, destinationType, mappingOptions);
				}
				catch (MapNotFoundException) { }
			}

			throw new MapNotFoundException((sourceType, destinationType));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region IMapperCanMap methods
		public bool CanMapNew(Type sourceType, Type destinationType) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Check if any mapper implements IMapperCanMap, if one of them throws it means that the map can be checked only when mapping
			var mappersToIgnore = new List<IMapper>();
			foreach (var mapper in _mappers.OfType<IMapperCanMap>()) {
				try { 
					if(mapper.CanMapNew(sourceType, destinationType))
						return true;
				}
				catch (InvalidOperationException) {
					mappersToIgnore.Add(mapper);
				}
			}

			// Try creating a default source object and try mapping it
			if (mappersToIgnore.Count != _mappers.Count) { 
				object source;
				try {
					source = ObjectFactory.Create(sourceType) ?? throw new Exception(); // Just in case
				}
				catch {
					throw new InvalidOperationException("Cannot verify if the mapper supports the given map because unable to create an object to test it");
				}

				foreach (var mapper in _mappers) {
					// Skip mappers which cannot be checked
					if (mappersToIgnore.Contains(mapper))
						continue;

					try {
						mapper.Map(source, sourceType, destinationType);
						return true;
					}
					catch (MapNotFoundException) {}
				}
			}

			if(mappersToIgnore.Count > 0)
				throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
			else
				return false;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public bool CanMapMerge(Type sourceType, Type destinationType) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Check if any mapper implements IMapperCanMap, if one of them throws it means that the map can be checked only when mapping
			var mappersToIgnore = new List<IMapper>();
			foreach (var mapper in _mappers.OfType<IMapperCanMap>()) {
				try {
					if (mapper.CanMapMerge(sourceType, destinationType))
						return true;
				}
				catch (InvalidOperationException) {
					mappersToIgnore.Add(mapper);
				}
			}

			// Try creating two default source and destination objects and try mapping them
			if (mappersToIgnore.Count != _mappers.Count) {
				object source;
				object destination;
				try {
					source = ObjectFactory.Create(sourceType) ?? throw new Exception(); // Just in case
					destination = ObjectFactory.Create(destinationType) ?? throw new Exception(); // Just in case
				}
				catch {
					throw new InvalidOperationException("Cannot verify if the mapper supports the given map because unable to create the objects to test it");
				}

				foreach (var mapper in _mappers) {
					// Skip mappers which cannot be checked
					if(mappersToIgnore.Contains(mapper))
						continue;

					try {
						mapper.Map(source, sourceType, destination, destinationType);
						return true;
					}
					catch (MapNotFoundException) { }
				}
			}

			if (mappersToIgnore.Count > 0)
				throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
			else
				return false;
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		MappingOptions MergeOrCreateMappingOptions(MappingOptions options) {
			var overrideOptions = options?.GetOptions<MapperOverrideMappingOptions>();
			if(overrideOptions == null){
				overrideOptions = new MapperOverrideMappingOptions();
				if(options != null)
					options = new MappingOptions(options.AsEnumerable().Concat(new[] { overrideOptions }));
				else
					options = new MappingOptions(new[] { overrideOptions });
			}

			overrideOptions.Mapper = this;

			return options;
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
