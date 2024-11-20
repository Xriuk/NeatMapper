using Dijkstra.NET.Graph;
using Dijkstra.NET.ShortestPath;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NeatMapper.Transitive {
	internal sealed class GraphCreator {
		// Graph and TypeToKeyMap should be safe to read concurrently because they are created once and not edited again
		private class TransitiveGraph {
			public Graph<Type, int> Graph { get; } = new Graph<Type, int>();

			public Dictionary<Type, uint> TypeToKeyMap { get; } = [];
		}


		private readonly Func<MappingOptions, IEnumerable<(Type From, Type To)>> _mapsRetriever;

		/// <summary>
		/// Options to apply when mapping types.
		/// </summary>
		private readonly TransitiveOptions _transitiveOptions;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output maps.
		/// Will not return null, but the returned <see cref="ConcurrentDictionary{TKey, TValue}"/>
		/// may return null if no maps are available for the given types.
		/// </summary>
		private readonly MappingOptionsFactoryCache<Func<Type, Type, List<Type>?>?> _typesPathCache;


		public GraphCreator(Func<MappingOptions, IEnumerable<(Type From, Type To)>> mapsRetriever, TransitiveOptions? transitiveOptions) {
			_mapsRetriever = mapsRetriever ?? throw new ArgumentNullException(nameof(mapsRetriever));
			_transitiveOptions = transitiveOptions ?? new TransitiveOptions();
			var graphsCache = new MappingOptionsFactoryCache<TransitiveGraph?>(options => {
				// Retrieve types which can be mapped
				var maps = _mapsRetriever.Invoke(options);
				if (!maps.Any())
					return null;

				var graph = new TransitiveGraph();
#pragma warning disable IDE0042
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
#pragma warning restore IDE0042

				return graph;
			});
			_typesPathCache = new MappingOptionsFactoryCache<Func<Type, Type, List<Type>?>?>(options => {
				var graph = graphsCache.GetOrCreate(options);
				if (graph == null)
					return null;

				if (options.Cached) {
					var cache = new ConcurrentDictionary<(Type From, Type To), List<Type>?>();
					return (sourceType, destinationType) => cache.GetOrAdd((sourceType, destinationType), types => CreateTypesPath(types.From, types.To, graph, options));
				}
				else
					return (sourceType, destinationType) => CreateTypesPath(sourceType, destinationType, graph, options);
			});


			List<Type>? CreateTypesPath(Type sourceType, Type destinationType, TransitiveGraph graph, MappingOptions options) {
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

				var typesPath = path.GetPath().Select(p => graph.Graph[p].Item).ToList();
				if (typesPath.Count < 2 || typesPath.Count > length || typesPath[0] != sourceType || typesPath[^1] != destinationType)
					throw new InvalidOperationException("Returned types are invalid");

				return typesPath;
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public List<Type>? GetOrCreateTypesPath(Type sourceType, Type destinationType, MappingOptions? options) {
			return _typesPathCache.GetOrCreate(options)?.Invoke(sourceType, destinationType);
		}
	}
}
