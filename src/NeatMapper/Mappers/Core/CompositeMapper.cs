using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which delegates mapping to other <see cref="IMapper"/>s,
	/// this allows to combine different mapping capabilities.<br/>
	/// Each mapper is invoked in order and the first one to succeed in mapping is returned.<br/>
	/// For new maps, if no mapper can map the types a destination object is created and merge maps are tried.
	/// </summary>
	public sealed class CompositeMapper : IMapper, IMapperCanMap, IMapperFactory {
		private readonly IList<IMapper> _mappers;
		private readonly NestedMappingContext _nestedMappingContext;

		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapper"/>.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public CompositeMapper(params IMapper[] mappers) : this((IList<IMapper>) mappers) { }

		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapper"/>.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public CompositeMapper(IList<IMapper> mappers) {
			if (mappers == null)
				throw new ArgumentNullException(nameof(mappers));

			_mappers = new List<IMapper>(mappers);
			_nestedMappingContext = new NestedMappingContext(this);
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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions, false);

			// Try new map
			foreach (var mapper in _mappers) {
				try {
					return mapper.Map(source, sourceType, destinationType, mappingOptions);
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

			return Map(source, sourceType, destination, destinationType, mappingOptions);

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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions, false);

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
		public bool CanMapNew(
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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions, false);

			// Check if any mapper implements IMapperCanMap, if one of them throws it means that the map can be checked only when mapping
			var undeterminateMappers = new List<IMapper>();
			foreach (var mapper in _mappers.OfType<IMapperCanMap>()) {
				try { 
					if(mapper.CanMapNew(sourceType, destinationType, mappingOptions))
						return true;
				}
				catch (InvalidOperationException) {
					undeterminateMappers.Add(mapper);
				}
			}

			// Try creating a default source object and try mapping it
			var mappersLeft = _mappers.Where(m => !(m is IMapperCanMap) || undeterminateMappers.IndexOf(m) != -1);
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
						mapper.Map(source, sourceType, destinationType, mappingOptions);
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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions, false);

			// Check if any mapper implements IMapperCanMap, if one of them throws it means that the map can be checked only when mapping
			var undeterminateMappers = new List<IMapper>();
			foreach (var mapper in _mappers.OfType<IMapperCanMap>()) {
				try {
					if (mapper.CanMapMerge(sourceType, destinationType, mappingOptions))
						return true;
				}
				catch (InvalidOperationException) {
					undeterminateMappers.Add(mapper);
				}
			}

			// Try creating two default source and destination objects and try mapping them
			var mappersLeft = _mappers.Where(m => !(m is IMapperCanMap) || undeterminateMappers.IndexOf(m) != -1);
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
						mapper.Map(source, sourceType, destination, destinationType, mappingOptions);
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

		#region IMapperFactory methods
		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?, object?
#else
			object, object
#endif
			> MapNewFactory(
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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions, true);

			// Check if any mapper implements IMapperFactory
			var unavailableMappers = new List<IMapper>();
			foreach (var mapper in _mappers.OfType<IMapperFactory>()) {
				try {
					return mapper.MapNewFactory(sourceType, destinationType, mappingOptions);
				}
				catch (MapNotFoundException) {
					unavailableMappers.Add(mapper);
				}
			}

			// Check if any mapper can map the types
			foreach (var mapper in _mappers.OfType<IMapperCanMap>()) {
				try {
					if (!mapper.CanMapNew(sourceType, destinationType))
						unavailableMappers.Add(mapper);
				}
				catch { }
			}

			// Return the default map wrapped
			var mappersLeft = _mappers.Except(unavailableMappers).ToArray();
			if(mappersLeft.Length == 0)
				throw new MapNotFoundException((sourceType, destinationType));
			else { 
				return source => {
					foreach(var mapper in mappersLeft) {
						try {
							return mapper.Map(source, sourceType, destinationType, mappingOptions);
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

					foreach (var mapper in mappersLeft) {
						try {
							return mapper.Map(source, sourceType, destination, destinationType, mappingOptions);
						}
						catch (MapNotFoundException) { }
					}

					throw new MapNotFoundException((sourceType, destinationType));
				};
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?, object?, object?
#else
			object, object, object
#endif
			> MapMergeFactory(
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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions, true);

			// Check if any mapper implements IMapperFactory
			var unavailableMappers = new List<IMapper>();
			foreach (var mapper in _mappers.OfType<IMapperFactory>()) {
				try {
					return mapper.MapMergeFactory(sourceType, destinationType, mappingOptions);
				}
				catch (MapNotFoundException) {
					unavailableMappers.Add(mapper);
				}
			}

			// Check if any mapper can map the types
			foreach (var mapper in _mappers.OfType<IMapperCanMap>()) {
				try {
					if (!mapper.CanMapMerge(sourceType, destinationType))
						unavailableMappers.Add(mapper);
				}
				catch { }
			}

			// Return the default map wrapped
			var mappersLeft = _mappers.Except(unavailableMappers).ToArray();
			if (mappersLeft.Length == 0)
				throw new MapNotFoundException((sourceType, destinationType));
			else
				return (source, destination) => {
					foreach (var mapper in mappersLeft) {
						try {
							return mapper.Map(source, sourceType, destination, destinationType, mappingOptions);
						}
						catch (MapNotFoundException) { }
					}

					throw new MapNotFoundException((sourceType, destinationType));
				};

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		// Will override a mapper if not already overridden
		MappingOptions MergeOrCreateMappingOptions(MappingOptions options, bool isRealFactory) {
			return (options ?? MappingOptions.Empty).ReplaceOrAdd<MapperOverrideMappingOptions, NestedMappingContext, FactoryContext>(
				m => m?.Mapper != null ? m : new MapperOverrideMappingOptions(this, m?.ServiceProvider),
				n => n != null ? new NestedMappingContext(this, n) : _nestedMappingContext,
				f => isRealFactory ? FactoryContext.Instance : f);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
