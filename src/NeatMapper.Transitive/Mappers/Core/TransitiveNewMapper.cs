using System;
using System.Linq;

namespace NeatMapper.Transitive {
	/// <summary>
	/// <see cref="IMapper"/> which maps types transitively to new ones by retrieving all the available maps
	/// from another <see cref="IMapper"/> (via <see cref="IMapperMaps.GetNewMaps(MappingOptions)"/>),
	/// the retrieved maps are combined into a graph and the shortest path is solved by using
	/// the Dijkstra algorithm.
	/// </summary>
	public sealed class TransitiveNewMapper : TransitiveMapper, IMapperCanMap, IMapperFactory {
		/// <summary>
		/// Instance used to create and retrieve type chains.
		/// </summary>
		private readonly GraphCreator _graphCreator;


		/// <summary>
		/// Creates a new instance of <see cref="TransitiveNewMapper"/>.
		/// </summary>
		/// <param name="mapper">
		/// <see cref="IMapper"/> to use to map types.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions.Mapper"/>.
		/// </param>
		/// <param name="transitiveOptions">
		/// Options to apply when mapping types.<br/>
		/// Can be overridden during mapping with <see cref="TransitiveMappingOptions"/>.
		/// </param>
		public TransitiveNewMapper(IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			TransitiveOptions?
#else
			TransitiveOptions
#endif
			transitiveOptions = null) : base(mapper, transitiveOptions) {

			_graphCreator = new GraphCreator(mappingOptions =>
				(mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
					?? _mapper).GetNewMaps(mappingOptions).Distinct(), transitiveOptions);
		}


		#region IMapper methods
		override public
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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			var typesPath = _graphCreator.GetOrCreateTypesPath(sourceType, destinationType, mappingOptions)
				?? throw new MapNotFoundException((sourceType, destinationType));

			var mapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			try { 
				return typesPath.Zip(typesPath.Skip(1), (t1, t2) => (From: t1, To: t2))
					.Aggregate(source, (result, types) => mapper.Map(result, types.From, types.To, mappingOptions));
			}
			catch (MapNotFoundException) {
				throw new MapNotFoundException((sourceType, destinationType));
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

		override public
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

			// Not mapping merge
			throw new MapNotFoundException((sourceType, destinationType));
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// We cannot map identical types
			if (sourceType == destinationType)
				throw new MapNotFoundException((sourceType, destinationType));

			return _graphCreator.GetOrCreateTypesPath(sourceType, destinationType, MergeOrCreateMappingOptions(mappingOptions)) != null;
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

			return false;
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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			var typesPath = _graphCreator.GetOrCreateTypesPath(sourceType, destinationType, mappingOptions)
				?? throw new MapNotFoundException((sourceType, destinationType));

			var mapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			// Create all the factories for the types (if we fail we dispose the already created ones)
			var factories = new INewMapFactory[typesPath.Count - 1];
			try { 
				var i = 0;
				foreach(var types in typesPath.Zip(typesPath.Skip(1), (t1, t2) => (From: t1, To: t2))) {
					factories[i] = mapper.MapNewFactory(types.From, types.To, mappingOptions);
					i++;
				}

				return new DisposableNewMapFactory(
					sourceType, destinationType,
					source => {
						try {
							return factories.Aggregate(source, (result, factory) => factory.Invoke(result));
						}
						catch (MapNotFoundException) {
							throw new MapNotFoundException((sourceType, destinationType));
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
					factory.Dispose();
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

			// Not mapping merge
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion
	}
}
