﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which delegates mapping to other <see cref="IMapper"/>s,
	/// this allows to combine different mapping capabilities.<br/>
	/// For new maps, if no mapper can map the types a destination object is created and merge maps are tried.
	/// </summary>
	public sealed class CompositeMapper : IMapper, IMapperFactory, IMapperMaps {
		/// <summary>
		/// List of <see cref="IMapper"/>s to be tried in order when mapping types.
		/// </summary>
		private readonly IReadOnlyList<IMapper> _mappers;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapper"/>.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public CompositeMapper(params IMapper[] mappers) :
			this((IList<IMapper>)mappers ?? throw new ArgumentNullException(nameof(mappers))) { }
		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapper"/>.
		/// </summary>
		/// <param name="mappers">Mappers to delegate the mapping to.</param>
		public CompositeMapper(IList<IMapper> mappers) {
			_mappers = new List<IMapper>(mappers ?? throw new ArgumentNullException(nameof(mappers)));
			var nestedMappingContext = new NestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<MapperOverrideMappingOptions, NestedMappingContext>(
				m => m?.Mapper != null ? m : new MapperOverrideMappingOptions(this, m?.ServiceProvider),
				n => n != null ? new NestedMappingContext(this, n) : nestedMappingContext, options.Cached));
		}


		#region IMapper methods
		public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			return _mappers.Any(m => m.CanMapNew(sourceType, destinationType, mappingOptions)) ||
				(ObjectFactory.CanCreate(destinationType) && _mappers.Any(m => m.CanMapMerge(sourceType, destinationType, mappingOptions)));
		}

		public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			return _mappers.Any(m => m.CanMapMerge(sourceType, destinationType, mappingOptions));
		}

		public object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if(sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			var newMappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var mapper = _mappers.FirstOrDefault(m => m.CanMapNew(sourceType, destinationType, newMappingOptions));
			if (mapper != null)
				return mapper.Map(source, sourceType, destinationType, newMappingOptions);

			if (!ObjectFactory.CanCreate(destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			return Map(source, sourceType, ObjectFactory.Create(destinationType), destinationType, mappingOptions);
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var mapper = _mappers.FirstOrDefault(m => m.CanMapMerge(sourceType, destinationType, mappingOptions));
			if (mapper != null)
				return mapper.Map(source, sourceType, destination, destinationType, mappingOptions);
			else
				throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion

		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			// Try retrieving a new factory
			IMapper? validMapper = null;
			foreach(var mapper in _mappers) {
				if(mapper.CanMapNew(sourceType, destinationType, mappingOptions)) {
					if(mapper is IMapperFactory factory)
						return factory.MapNewFactory(sourceType, destinationType, mappingOptions);
					else
						validMapper ??= mapper;
				}
			}

			// If we can map the types we return Map wrapped in a delegate
			if (validMapper != null) { 
				return new DefaultNewMapFactory(
					sourceType, destinationType,
					source => validMapper.Map(source, sourceType, destinationType, mappingOptions));
			}

			// Check if we can forward to merge map by creating a destination object
			if (ObjectFactory.CanCreate(destinationType)) { 
				// Try retrieving a merge factory
				foreach (var mapper in _mappers) {
					if (mapper.CanMapMerge(sourceType, destinationType, mappingOptions)) {
						if (mapper is IMapperFactory factory)
							return factory.MapMergeFactory(sourceType, destinationType, mappingOptions).MapNewFactory();
						else
							validMapper ??= mapper;
					}
				}

				// If we can map the types we return Map wrapped in a delegate
				if (validMapper != null) { 
					var destinationFactory = ObjectFactory.CreateFactory(destinationType);

					return new DefaultNewMapFactory(
						sourceType, destinationType,
						source => validMapper.Map(source, sourceType, destinationFactory.Invoke(), destinationType, mappingOptions));
				}
			}

			throw new MapNotFoundException((sourceType, destinationType));
		}

		public IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			// Try retrieving a merge factory
			IMapper? validMapper = null;
			foreach (var mapper in _mappers) {
				if (mapper.CanMapMerge(sourceType, destinationType, mappingOptions)) {
					if (mapper is IMapperFactory factory)
						return factory.MapMergeFactory(sourceType, destinationType, mappingOptions);
					else
						validMapper ??= mapper;
				}
			}

			// If we can map the types we return Map wrapped in a delegate
			if (validMapper != null) { 
				return new DefaultMergeMapFactory(
					sourceType, destinationType,
					(source, destination) => validMapper.Map(source, sourceType, destination, destinationType, mappingOptions));
			}
			else
				throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion

		#region IMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetNewMaps(MappingOptions? mappingOptions = null) {
			// Supports both new and merge maps (where destination objects can be created)
			return _mappers.SelectMany(m => m.GetNewMaps(mappingOptions))
				.Concat(GetMergeMaps(mappingOptions).Where(m => ObjectFactory.CanCreate(m.To)));
		}

		public IEnumerable<(Type From, Type To)> GetMergeMaps(MappingOptions? mappingOptions = null) {
			return _mappers.SelectMany(m => m.GetMergeMaps(mappingOptions));
		}
		#endregion
	}
}
