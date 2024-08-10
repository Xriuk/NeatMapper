using Dijkstra.NET.Graph;
using Dijkstra.NET.ShortestPath;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Transitive {
	/// <summary>
	/// <see cref="IMapper"/> which maps types transitively to new ones by retrieving all the available maps
	/// from another <see cref="IMapper"/> (via <see cref="IMapperMaps.GetNewMaps(MappingOptions)"/>),
	/// the retrieved maps are combined into a graph and the shortest path is solved by using
	/// the Dijkstra algorithm.<br/>
	/// The returned factory from <see cref="MapNewFactory(Type, Type, MappingOptions)"/> will be
	/// <see cref="ITransitiveNewMapFactory"/> or a derived type.
	/// </summary>
	public sealed class TransitiveNewMapper : TransitiveMapper, IMapperCanMap, IMapperFactory {
		// Graph and TypeToKeyMap should be safe to read concurrently because they are created once and not edited again
		private class TransitiveGraph {
			public Graph<Type, int> Graph { get; } = new Graph<Type, int>();

			public Dictionary<Type, uint> TypeToKeyMap { get; } = new Dictionary<Type, uint>();
		}


		/// <summary>
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output <see cref="TransitiveGraph"/>.
		/// May return null if no maps are available.
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, TransitiveGraph> _graphsCache =
			new ConcurrentDictionary<MappingOptions, TransitiveGraph>();

		/// <summary>
		/// Cached output <see cref="TransitiveGraph"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have null keys) and <see cref="MappingOptions.Empty"/>,
		/// also provides faster access since locking isn't needed for thread-safety.
		/// May be null if no maps are available.
		/// </summary>
		private readonly TransitiveGraph _graphsCacheNull;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output maps.
		/// Will not return null, but the returned <see cref="ConcurrentDictionary{TKey, TValue}"/>
		/// may return null if no maps are available for the given types.
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, ConcurrentDictionary<(Type From, Type To), IList<Type>>> _typesPathCache =
			new ConcurrentDictionary<MappingOptions, ConcurrentDictionary<(Type From, Type To), IList<Type>>>();

		/// <summary>
		/// Cached input <see cref="MappingOptions"/>  for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have null keys) and <see cref="MappingOptions.Empty"/>,
		/// also provides faster access since locking isn't needed for thread-safety.
		/// Will not be null, but the <see cref="ConcurrentDictionary{TKey, TValue}"/> may return null
		/// if no maps are available for the given types.
		/// </summary>
		private readonly ConcurrentDictionary<(Type From, Type To), IList<Type>> _typesPathCacheNull =
			new ConcurrentDictionary<(Type From, Type To), IList<Type>>();


		/// <summary>
		/// Creates a new instance of <see cref="TransitiveNewMapper"/>.
		/// </summary>
		/// <param name="mapper">
		/// <see cref="IMapper"/> to use to map types.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions.Mapper"/>.
		/// </param>
		/// <param name="transitiveOptions">Options to apply when mapping types.</param>
		public TransitiveNewMapper(IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			TransitiveOptions?
#else
			TransitiveOptions
#endif
			transitiveOptions = null) : base(mapper, transitiveOptions) {

			_graphsCacheNull = GreateGraph(MappingOptions.Empty);
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

			var typesPath = GetOrCreateTypesPath(sourceType, destinationType, mappingOptions)
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

			return GetOrCreateTypesPath(sourceType, destinationType, MergeOrCreateMappingOptions(mappingOptions)) != null;
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

			var typesPath = GetOrCreateTypesPath(sourceType, destinationType, mappingOptions)
				?? throw new MapNotFoundException((sourceType, destinationType));

			var mapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			// Create all the factories for the types (if we fail we dispose the already created ones)
			var factories = new List<INewMapFactory>();
			try { 
				foreach(var types in typesPath.Zip(typesPath.Skip(1), (t1, t2) => (From: t1, To: t2))) {
					factories.Add(mapper.MapNewFactory(types.From, types.To, mappingOptions));
				}

				return new DefaultTransitiveNewMapFactory(
					sourceType, destinationType,
					typesPath,
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
					factories.ToArray());
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


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private TransitiveGraph GetOrCreateGraph(MappingOptions options) {
			if (options == null || options == MappingOptions.Empty)
				return _graphsCacheNull;
			else if (options.Cached)
				return _graphsCache.GetOrAdd(options, GreateGraph);
			else
				return GreateGraph(options);
		}

		private TransitiveGraph GreateGraph(MappingOptions options) {
			var mapper = options.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			// Retrieve types which can be mapped
			var maps = mapper.GetNewMaps(options).Distinct();
			if (!maps.Any())
				return null;

			var graph = new TransitiveGraph();
			foreach (var types in maps) {
				// Ignore types which map to themselves
				if (types.From == types.To)
					continue;

				// Retrieve or add types to the graph
				if (!graph.TypeToKeyMap.TryGetValue(types.From, out var fromKey)) {
					fromKey = graph.Graph.AddNode(types.From);
					graph.TypeToKeyMap.Add(types.From, fromKey);
				}
				if (!graph.TypeToKeyMap.TryGetValue(types.To, out var toKey)) {
					toKey = graph.Graph.AddNode(types.To);
					graph.TypeToKeyMap.Add(types.To, toKey);
				}

				// Connect the types
				graph.Graph.Connect(fromKey, toKey, 1, 0);
			}

			return graph;
		}

		private IList<Type> GetOrCreateTypesPath(Type sourceType, Type destinationType, MappingOptions options) {
			var graph = GetOrCreateGraph(options);
			if (graph == null)
				return null;

			if (options == null || options == MappingOptions.Empty)
				return _typesPathCacheNull.GetOrAdd((sourceType, destinationType), types => CreateTypesPath(types.From, types.To, graph, options));
			else if (options.Cached) {
				return _typesPathCache.GetOrAdd(options, types => new ConcurrentDictionary<(Type From, Type To), IList<Type>>())
					.GetOrAdd((sourceType, destinationType), types => CreateTypesPath(types.From, types.To, graph, options));
			}
			else
				return CreateTypesPath(sourceType, destinationType, graph, options);
		}

		private IList<Type> CreateTypesPath(Type sourceType, Type destinationType, TransitiveGraph graph, MappingOptions options) {
			// Retrieve the graph and source and destination types from it
			if (!graph.TypeToKeyMap.TryGetValue(sourceType, out var sourceKey) ||
				!graph.TypeToKeyMap.TryGetValue(destinationType, out var destinationKey)) {

				return null;
			}

			var length = options.GetOptions<TransitiveMappingOptions>()?.MaxChainLength ?? _transitiveOptions.MaxChainLength;
			if(length < 2)
				return null;

			// Retrieve the path, if any
			var path = graph.Graph.Dijkstra(sourceKey, destinationKey, length-1);
			if (!path.IsFounded)
				return null;

			var typesPath = path.GetPath().Select(p => graph.Graph[p].Item).ToArray();
			if (typesPath.Length < 2 || typesPath.Length > length || typesPath[0] != sourceType || typesPath[typesPath.Length - 1] != destinationType)
				throw new InvalidOperationException("Returned types are invalid");

			return typesPath;
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
