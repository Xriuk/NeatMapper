using System;
using System.Linq;

namespace NeatMapper.Transitive {
	/// <summary>
	/// <see cref="IMapper"/> which maps types transitively by retrieving all the available maps
	/// from another <see cref="IMapper"/> (via <see cref="IMapperMaps.GetNewMaps(MappingOptions)"/> and
	/// <see cref="IMapperMaps.GetMergeMaps(MappingOptions)"/>), the retrieved maps are combined into a graph
	/// and the shortest path is solved by using the Dijkstra algorithm.
	/// </summary>
	public sealed class TransitiveMapper : IMapper, IMapperFactory {
		/// <summary>
		/// <see cref="IMapper"/> which is used to map the types, will be also provided as a nested mapper
		/// in <see cref="MapperOverrideMappingOptions"/> (if not already present).
		/// </summary>
		private readonly IMapper _mapper;

		/// <summary>
		/// Options to apply when mapping types.
		/// </summary>
		private readonly TransitiveOptions _transitiveOptions;

		/// <summary>
		/// Instance used to create and retrieve type chains.
		/// </summary>
		private readonly GraphCreator _graphCreator;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _mergeNewOptionsCache;


		/// <summary>
		/// Creates a new instance of <see cref="TransitiveMapper"/>.
		/// </summary>
		/// <param name="mapper">
		/// <see cref="IMapper"/> to use to merge map types.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions.Mapper"/>.
		/// </param>
		/// <param name="transitiveOptions">
		/// Options to apply when mapping types.<br/>
		/// Can be overridden during mapping with <see cref="TransitiveMappingOptions"/>.
		/// </param>
		public TransitiveMapper(IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			TransitiveOptions?
#else
			TransitiveOptions
#endif
			transitiveOptions = null) {

			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_transitiveOptions = transitiveOptions ?? new TransitiveOptions();
			_graphCreator = new GraphCreator(mappingOptions =>
				(mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
					?? _mapper).GetNewMaps(mappingOptions).Distinct(), _transitiveOptions);
			var nestedMappingContext = new NestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<NestedMappingContext>(
				n => n != null ? new NestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext, options.Cached));
			var mergeNewTransitiveOptions = new TransitiveMappingOptions(_transitiveOptions.MaxChainLength - 1);
			_mergeNewOptionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<TransitiveMappingOptions>(
				t => t?.MaxChainLength != null ? new TransitiveMappingOptions(Math.Max(t.MaxChainLength.Value - 1, 0)) : mergeNewTransitiveOptions, options.Cached));
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

			// We cannot map identical types
			if (sourceType == destinationType)
				throw new MapNotFoundException((sourceType, destinationType));

			return _graphCreator.GetOrCreateTypesPath(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions)) != null;
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

			return CanMapMergeInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
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

			// We cannot map identical types
			if (sourceType == destinationType)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var typesPath = _graphCreator.GetOrCreateTypesPath(sourceType, destinationType, mappingOptions)
				?? throw new MapNotFoundException((sourceType, destinationType));

			var mapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			try {
				return typesPath.Zip(typesPath.Skip(1), (t1, t2) => (From: t1, To: t2))
					.Aggregate(source, (result, types) => mapper.Map(result, types.From, types.To, mappingOptions));
			}
			catch (OperationCanceledException) {
				throw;
			}
			catch (Exception e) {
				throw new MappingException(e, (sourceType, destinationType));
			}

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

			if (!CanMapMergeInternal(sourceType, destinationType, ref mappingOptions, out var mergeMapFrom, out var newMappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			var mapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			if (mergeMapFrom == sourceType) { 
				return mapper.Map(
					source,
					sourceType,
					destination,
					destinationType,
					mappingOptions);
			}
			else {
				// Map source with new map and then merge result with destination
				return mapper.Map(
					mapper.Map(
						source,
						sourceType,
						mergeMapFrom,
						newMappingOptions),
					mergeMapFrom,
					destination,
					destinationType,
					mappingOptions);
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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

			// We cannot map identical types
			if (sourceType == destinationType)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var typesPath = _graphCreator.GetOrCreateTypesPath(sourceType, destinationType, mappingOptions)
				?? throw new MapNotFoundException((sourceType, destinationType));

			var mapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			// Create all the factories for the types (if we fail we dispose the already created ones)
			var factories = new INewMapFactory[typesPath.Count - 1];
			try {
				var i = 0;
				foreach (var types in typesPath.Zip(typesPath.Skip(1), (t1, t2) => (From: t1, To: t2))) {
					factories[i] = mapper.MapNewFactory(types.From, types.To, mappingOptions);
					i++;
				}

				return new DisposableNewMapFactory(
					sourceType, destinationType,
					source => {
						try {
							return factories.Aggregate(source, (result, factory) => factory.Invoke(result));
						}
						catch (OperationCanceledException) {
							throw;
						}
						catch (Exception e) {
							throw new MappingException(e, (sourceType, destinationType));
						}
					},
					factories);
			}
			catch {
				foreach (var factory in factories) {
					factory?.Dispose();
				}
				throw;
			}

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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (!CanMapMergeInternal(sourceType, destinationType, ref mappingOptions, out var mergeMapFrom, out var newMappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			var mapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			if (mergeMapFrom == sourceType) 
				return mapper.MapMergeFactory(sourceType, destinationType, mappingOptions);
			else {
				// Map source with new map and then merge result with destination
				var newFactory = mapper.MapNewFactory(sourceType, mergeMapFrom, newMappingOptions);

				try {
					var mergeFactory = mapper.MapMergeFactory(mergeMapFrom, destinationType, mappingOptions);

					try {
						return new DisposableMergeMapFactory(
							sourceType, destinationType,
							(source, destination) => mergeFactory.Invoke(newFactory.Invoke(source), destination),
							newFactory, mergeFactory);
					}
					catch {
						mergeFactory.Dispose();
						throw;
					}
				}
				catch {
					newFactory.Dispose();
					throw;
				}
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private bool CanMapMergeInternal(
			Type sourceType,
			Type destinationType,
			ref MappingOptions mappingOptions,
			out Type mergeMapFrom,
			out MappingOptions newMappingOptions) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			newMappingOptions = null;

			// We cannot map identical types
			if (sourceType == destinationType) {
				mergeMapFrom = null;
				return false;
			}

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var length = mappingOptions.GetOptions<TransitiveMappingOptions>()?.MaxChainLength ?? _transitiveOptions.MaxChainLength;
			if (length < 2) {
				mergeMapFrom = null;
				return false;
			}

			var mapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			// Retrieve all the merge maps with the selected destination type
			foreach (var mergeMap in mapper.GetMergeMaps(mappingOptions).Distinct().Where(m => m.To == destinationType)) {
				// 2 new map + 1 merge map
				if (mergeMap.From == sourceType) {
					mergeMapFrom = sourceType;
					return true;
				}
				else if (length < 2 + 1)
					continue;

				if (newMappingOptions == null)
					newMappingOptions = _mergeNewOptionsCache.GetOrCreate(mappingOptions);

				// Try creating a new maps path to the retrieved source type of the merge map
				if (mapper.CanMapNew(sourceType, mergeMap.From, newMappingOptions)) { 
					mergeMapFrom = mergeMap.From;
					return true;
				}
			}

			mergeMapFrom = null;
			return false;
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
