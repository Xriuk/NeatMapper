using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace NeatMapper.Transitive {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which maps types transitively and asynchronously by retrieving all the available maps
	/// from another <see cref="IAsyncMapper"/> (via <see cref="IAsyncMapperMaps.GetAsyncNewMaps(MappingOptions)"/> and
	/// <see cref="IAsyncMapperMaps.GetAsyncMergeMaps(MappingOptions)"/>), the retrieved maps are combined into a graph
	/// and the shortest path is solved by using the Dijkstra algorithm.
	/// </summary>
	public sealed class AsyncTransitiveMapper : IAsyncMapper, IAsyncMapperFactory {
		/// <summary>
		/// <see cref="IAsyncMapper"/> which is used to map the types, will be also provided as a nested mapper
		/// in <see cref="AsyncMapperOverrideMappingOptions"/> (if not already present).
		/// </summary>
		private readonly IAsyncMapper _mapper;

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
		/// Creates a new instance of <see cref="AsyncTransitiveMapper"/>.
		/// </summary>
		/// <param name="mapper">
		/// <see cref="IAsyncMapper"/> to use to merge map types.<br/>
		/// Can be overridden during mapping with <see cref="AsyncMapperOverrideMappingOptions.Mapper"/>.
		/// </param>
		/// <param name="transitiveOptions">
		/// Options to apply when mapping types.<br/>
		/// Can be overridden during mapping with <see cref="TransitiveMappingOptions"/>.
		/// </param>
		public AsyncTransitiveMapper(IAsyncMapper mapper, TransitiveOptions? transitiveOptions = null) {
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_transitiveOptions = transitiveOptions ?? new TransitiveOptions();
			_graphCreator = new GraphCreator(mappingOptions =>
				(mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper
					?? _mapper).GetAsyncNewMaps(mappingOptions).Distinct(), _transitiveOptions);
			var nestedMappingContext = new AsyncNestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<AsyncNestedMappingContext>(
				n => n != null ? new AsyncNestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext, options.Cached));
			var mergeNewTransitiveOptions = new TransitiveMappingOptions(_transitiveOptions.MaxChainLength - 1);
			_mergeNewOptionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<TransitiveMappingOptions>(
				t => t?.MaxChainLength != null ? new TransitiveMappingOptions(Math.Max(t.MaxChainLength.Value - 1, 0)) : mergeNewTransitiveOptions, options.Cached));
		}


		#region IAsyncMapper methods
		public bool CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// We cannot map identical types
			if (sourceType == destinationType)
				throw new MapNotFoundException((sourceType, destinationType));

			return _graphCreator.GetOrCreateTypesPath(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions)) != null;
		}

		public bool CanMapAsyncMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapMergeInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
		}

		public async Task<object?> MapAsync(
			object? source,
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

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

			var mapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			try {
				object? result = source;
#pragma warning disable IDE0042
				foreach (var types in typesPath.Zip(typesPath.Skip(1), (t1, t2) => (From: t1, To: t2))) {
					result = await mapper.MapAsync(result, types.From, types.To, mappingOptions, cancellationToken);
				}
#pragma warning restore IDE0042
				return result;
			}
			catch (OperationCanceledException) {
				throw;
			}
			catch (Exception e) {
				throw new MappingException(e, (sourceType, destinationType));
			}
		}

		public async Task<object?> MapAsync(
			object? source,
			Type sourceType,
			object? destination,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (!CanMapMergeInternal(sourceType, destinationType, ref mappingOptions, out var mergeMapFrom, out var newMappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			var mapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			if (mergeMapFrom == sourceType) { 
				return await mapper.MapAsync(
					source,
					sourceType,
					destination,
					destinationType,
					mappingOptions,
					cancellationToken);
			}
			else {
				// Map source with new map and then merge result with destination
				return await mapper.MapAsync(
					await mapper.MapAsync(
						source,
						sourceType,
						mergeMapFrom,
						newMappingOptions,
						cancellationToken),
					mergeMapFrom,
					destination,
					destinationType,
					mappingOptions,
					cancellationToken);
			}
		}
		#endregion

		#region IAsyncMapperFactory methods
		public IAsyncNewMapFactory MapAsyncNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
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

			var mapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			// Create all the factories for the types (if we fail we dispose the already created ones)
			var factories = new IAsyncNewMapFactory[typesPath.Count - 1];
			try {
				var i = 0;
#pragma warning disable IDE0042
				foreach (var types in typesPath.Zip(typesPath.Skip(1), (t1, t2) => (From: t1, To: t2))) {
					factories[i] = mapper.MapAsyncNewFactory(types.From, types.To, mappingOptions);
					i++;
				}
#pragma warning restore IDE0042

				return new DisposableAsyncNewMapFactory(
					sourceType, destinationType,
					async (source, cancellationToken) => {
						try {
							object? result = source;
							foreach (var factory in factories) {
								result = await factory.Invoke(result, cancellationToken);
							}
							return result;
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
		}

		public IAsyncMergeMapFactory MapAsyncMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapMergeInternal(sourceType, destinationType, ref mappingOptions, out var mergeMapFrom, out var newMappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			var mapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			if (mergeMapFrom == sourceType) 
				return mapper.MapAsyncMergeFactory(sourceType, destinationType, mappingOptions);
			else {
				// Map source with new map and then merge result with destination
				var newFactory = mapper.MapAsyncNewFactory(sourceType, mergeMapFrom, newMappingOptions);

				try {
					var mergeFactory = mapper.MapAsyncMergeFactory(mergeMapFrom, destinationType, mappingOptions);

					try {
						return new DisposableAsyncMergeMapFactory(
							sourceType, destinationType,
							async (source, destination, cancellationToken) => await mergeFactory.Invoke(await newFactory.Invoke(source, cancellationToken), destination, cancellationToken),
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
		}
		#endregion


		private bool CanMapMergeInternal(
			Type sourceType,
			Type destinationType,
			[NotNullWhen(true)] ref MappingOptions? mappingOptions,
			out Type mergeMapFrom,
			out MappingOptions newMappingOptions) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			newMappingOptions = null!;

			// We cannot map identical types
			if (sourceType == destinationType) {
				mergeMapFrom = null!;
				return false;
			}

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var length = mappingOptions.GetOptions<TransitiveMappingOptions>()?.MaxChainLength ?? _transitiveOptions.MaxChainLength;
			if (length < 2) {
				mergeMapFrom = null!;
				return false;
			}

			var mapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			// Retrieve all the merge maps with the selected destination type
#pragma warning disable IDE0042
			foreach (var mergeMap in mapper.GetAsyncMergeMaps(mappingOptions).Distinct().Where(m => m.To == destinationType)) {
				// 2 new map + 1 merge map
				if (mergeMap.From == sourceType) {
					mergeMapFrom = sourceType;
					return true;
				}
				else if (length < 2 + 1)
					continue;

				newMappingOptions ??= _mergeNewOptionsCache.GetOrCreate(mappingOptions);

				// Try creating a new maps path to the retrieved source type of the merge map
				if (mapper.CanMapAsyncNew(sourceType, mergeMap.From, newMappingOptions)) { 
					mergeMapFrom = mergeMap.From;
					return true;
				}
			}
#pragma warning restore IDE0042

			mergeMapFrom = null!;
			return false;
		}
	}
}
