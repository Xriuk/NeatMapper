#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using Dijkstra.NET.Graph;
using Dijkstra.NET.ShortestPath;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NeatMapper.Transitive {
	/// <summary>
	/// <see cref="IMapper"/> which maps types transitively by retrieving all the available maps from another
	/// <see cref="IMapper"/> (via <see cref="IMapperMaps"/> interface), the retrieved maps are combined into
	/// a graph and the shortest path is solved by using the Dijkstra algorithm.<br/>
	/// Internal class.
	/// </summary>
	public abstract class TransitiveMapper : IMapper {
		// Graph and TypeToKeyMap should be safe to read concurrently because they are created once and not edited again
		private class TransitiveGraph {
			public Graph<Type, int> Graph { get; } = new Graph<Type, int>();

			public Dictionary<Type, uint> TypeToKeyMap { get; } = new Dictionary<Type, uint>();
		}


		/// <summary>
		/// <see cref="IMapper"/> which is used to map the types, will be also provided as a nested mapper
		/// in <see cref="MapperOverrideMappingOptions"/> (if not already present).
		/// </summary>
		protected readonly IMapper _mapper;

		/// <summary>
		/// Delegate used to select types to map and their cost, types can mapped multiple times, with different costs.
		/// </summary>
		private readonly Func<IMapper, MappingOptions, IEnumerable<(Type From, Type To)>> _mapsSelector;

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
		/// Cached nested context with no parents.
		/// </summary>
		private readonly NestedMappingContext _nestedMappingContext;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output <see cref="MappingOptions"/> (with 
		///	<see cref="MappingOptions.Cached"/> also set to <see langword="true"/>).
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MappingOptions> _optionsCache =
			new ConcurrentDictionary<MappingOptions, MappingOptions>();

		/// <summary>
		/// Cached output <see cref="MappingOptions"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have null keys) and <see cref="MappingOptions.Empty"/> inputs,
		/// also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		private readonly MappingOptions _optionsCacheNull;


		internal TransitiveMapper(IMapper mapper, Func<IMapper, MappingOptions, IEnumerable<(Type From, Type To)>> mapsSelector) {
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_mapsSelector = mapsSelector ?? throw new ArgumentNullException(nameof(mapsSelector));

			_graphsCacheNull = GreateGraph(MappingOptions.Empty);
			_nestedMappingContext = new NestedMappingContext(this);
			_optionsCacheNull = MergeMappingOptions(MappingOptions.Empty);
		}


		public abstract object Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract object Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null);


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
			var maps = _mapsSelector.Invoke(mapper, options).Distinct();
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

		protected IList<Type> GetOrCreateTypesPath(Type sourceType, Type destinationType, MappingOptions options) {
			var graph = GetOrCreateGraph(options);
			if(graph == null)
				return null;

			if (options == null || options == MappingOptions.Empty) 
				return _typesPathCacheNull.GetOrAdd((sourceType, destinationType), types => CreateTypesPath(types.From, types.To, graph));
			else if (options.Cached) { 
				return _typesPathCache.GetOrAdd(options, types => new ConcurrentDictionary<(Type From, Type To), IList<Type>>())
					.GetOrAdd((sourceType, destinationType), types => CreateTypesPath(types.From, types.To, graph));
			}
			else
				return CreateTypesPath(sourceType, destinationType, graph);
		}

		private IList<Type> CreateTypesPath(Type sourceType, Type destinationType, TransitiveGraph graph) {
			// Retrieve the graph and source and destination types from it
			if (!graph.TypeToKeyMap.TryGetValue(sourceType, out var sourceKey) ||
				!graph.TypeToKeyMap.TryGetValue(destinationType, out var destinationKey)) {

				return null;
			}

			// Retrieve the path, if any
			var path = graph.Graph.Dijkstra(sourceKey, destinationKey);
			if (!path.IsFounded)
				return null;

			var typesPath = path.GetPath().Select(p => graph.Graph[p].Item).ToArray();
			if(typesPath.Length < 2 || typesPath[0] != sourceType || typesPath[typesPath.Length-1] != destinationType)
				throw new InvalidOperationException("Returned types are invalid");

			return typesPath;
		}

		protected MappingOptions MergeOrCreateMappingOptions(MappingOptions options) {
			if (options == null || options == MappingOptions.Empty)
				return _optionsCacheNull;
			else if (options.Cached)
				return _optionsCache.GetOrAdd(options, MergeMappingOptions);
			else
				return MergeMappingOptions(options);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MappingOptions MergeMappingOptions(MappingOptions options) {
			return options.ReplaceOrAdd<NestedMappingContext>(
				n => n != null ? new NestedMappingContext(_nestedMappingContext.ParentMapper, n) : _nestedMappingContext, options.Cached);
		}
	}
}
