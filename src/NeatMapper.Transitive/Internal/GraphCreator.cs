#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using Dijkstra.NET.Graph;
using Dijkstra.NET.ShortestPath;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Transitive {
	internal sealed class GraphCreator {
		// Graph and TypeToKeyMap should be safe to read concurrently because they are created once and not edited again
		private class TransitiveGraph {
			public Graph<Type, int> Graph { get; } = new Graph<Type, int>();

			public Dictionary<Type, uint> TypeToKeyMap { get; } = new Dictionary<Type, uint>();
		}


		private readonly Func<MappingOptions, IEnumerable<(Type From, Type To)>> _mapsRetriever;

		/// <summary>
		/// Options to apply when mapping types.
		/// </summary>
		private readonly TransitiveOptions _transitiveOptions;

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


		public GraphCreator(Func<MappingOptions, IEnumerable<(Type From, Type To)>> mapsRetriever, TransitiveOptions transitiveOptions) {
			_mapsRetriever = mapsRetriever ?? throw new ArgumentNullException(nameof(mapsRetriever));
			_transitiveOptions = transitiveOptions ?? new TransitiveOptions();
			_graphsCacheNull = GreateGraph(MappingOptions.Empty);
		}


		private TransitiveGraph GetOrCreateGraph(MappingOptions options) {
			if (options == null || options == MappingOptions.Empty)
				return _graphsCacheNull;
			else if (options.Cached)
				return _graphsCache.GetOrAdd(options, GreateGraph);
			else
				return GreateGraph(options);
		}

		private TransitiveGraph GreateGraph(MappingOptions options) {
			// Retrieve types which can be mapped
			var maps = _mapsRetriever.Invoke(options);
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

		public IList<Type> GetOrCreateTypesPath(Type sourceType, Type destinationType, MappingOptions options) {
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
			if (length < 2)
				return null;

			// Retrieve the path, if any
			var path = graph.Graph.Dijkstra(sourceKey, destinationKey, length - 1);
			if (!path.IsFounded)
				return null;

			var typesPath = path.GetPath().Select(p => graph.Graph[p].Item).ToArray();
			if (typesPath.Length < 2 || typesPath.Length > length || typesPath[0] != sourceType || typesPath[typesPath.Length - 1] != destinationType)
				throw new InvalidOperationException("Returned types are invalid");

			return typesPath;
		}
	}
}
