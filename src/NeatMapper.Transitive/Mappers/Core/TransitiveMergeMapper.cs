using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NeatMapper.Transitive {
	/// <summary>
	/// <see cref="IMapper"/> which maps types transitively to existing ones by retrieving all the available maps
	/// from another <see cref="IMapper"/> (via <see cref="IMapperMaps.GetMergeMaps(MappingOptions)"/>),
	/// the retrieved maps are then tried in order by mapping the source to the merge map source type and then applying
	/// the merge map itself.
	/// </summary>
	public sealed class TransitiveMergeMapper : TransitiveMapper, IMapperCanMap, IMapperFactory {
		/// <summary>
		/// Options to apply to new maps when mapping types, these will have
		/// <see cref="TransitiveOptions.MaxChainLength"/> with one level less.
		/// </summary>
		private readonly TransitiveMappingOptions _newTransitiveOptions;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output <see cref="MappingOptions"/> (with 
		///	<see cref="MappingOptions.Cached"/> also set to <see langword="true"/>).
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MappingOptions> _optionsNewCache =
			new ConcurrentDictionary<MappingOptions, MappingOptions>();

		/// <summary>
		/// Cached output <see cref="MappingOptions"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have null keys) and <see cref="MappingOptions.Empty"/> inputs,
		/// also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		private readonly MappingOptions _optionsNewCacheNull;


		/// <summary>
		/// Creates a new instance of <see cref="TransitiveMergeMapper"/>.
		/// </summary>
		/// <param name="mapper">
		/// <see cref="IMapper"/> to use to merge map types.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions.Mapper"/>.
		/// </param>
		/// <param name="transitiveOptions">
		/// Options to apply when mapping types.<br/>
		/// Can be overridden during mapping with <see cref="TransitiveMappingOptions"/>.
		/// </param>
		public TransitiveMergeMapper(IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			TransitiveOptions?
#else
			TransitiveOptions
#endif
			transitiveOptions = null) : base(mapper, transitiveOptions) {

			_optionsNewCacheNull = MergeNewMappingOptions(MappingOptions.Empty);
			_newTransitiveOptions = new TransitiveMappingOptions((transitiveOptions ?? new TransitiveOptions()).MaxChainLength - 1);
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

			// Not mapping new
			throw new MapNotFoundException((sourceType, destinationType));
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

			var length = mappingOptions.GetOptions<TransitiveMappingOptions>()?.MaxChainLength ?? _transitiveOptions.MaxChainLength;
			if (length < 2)
				throw new MapNotFoundException((sourceType, destinationType));

			var mapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			// Retrieve all the merge maps with the selected destination type
			MappingOptions newMappingOptions = null;
			foreach (var mergeMap in mapper.GetMergeMaps(mappingOptions).Distinct().Where(m => m.To == destinationType)) {
				// 2 new map + 1 merge map
				if(mergeMap.From == sourceType) {
					// Since we have unique maps if this one throws MapNotFoundException we cannot try any other map
					return mapper.Map(source, sourceType, destination, destinationType, mappingOptions);
				}
				else if (length < 2 + 1)
					continue;

				if(newMappingOptions == null)
					newMappingOptions = MergeOrCreateNewMappingOptions(mappingOptions);

				try {
					// Map source with new map and then merge result with destination
					return mapper.Map(
						mapper.Map(source, sourceType, mergeMap.From, newMappingOptions),
						mergeMap.From,
						destination,
						destinationType,
						mappingOptions);
				}
				catch (MapNotFoundException) {
					// Not throw, will try other merge maps and will throw at the end if needed
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception e) {
					throw new MappingException(e, (sourceType, destinationType));
				}
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

			return false;
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// We cannot map identical types
			if (sourceType == destinationType)
				return false;

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			var length = mappingOptions.GetOptions<TransitiveMappingOptions>()?.MaxChainLength ?? _transitiveOptions.MaxChainLength;
			if (length < 2)
				return false;

			var mapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			// Retrieve all the merge maps with the selected destination type
			MappingOptions newMappingOptions = null;
			foreach (var mergeMap in mapper.GetMergeMaps(mappingOptions).Distinct().Where(m => m.To == destinationType)) {
				// 2 new map + 1 merge map
				if (mergeMap.From == sourceType)
					return true;
				else if (length < 2 + 1)
					continue;

				if (newMappingOptions == null)
					newMappingOptions = MergeOrCreateNewMappingOptions(mappingOptions);

				// Try creating a new maps path to the retrieved source type of the merge map
				if (mapper.CanMapNew(sourceType, mergeMap.From, newMappingOptions))
					return true;
			}

			return false;

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

			// Not mapping new
			throw new MapNotFoundException((sourceType, destinationType));
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// We cannot map identical types
			if (sourceType == destinationType)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			var length = mappingOptions.GetOptions<TransitiveMappingOptions>()?.MaxChainLength ?? _transitiveOptions.MaxChainLength;
			if (length < 2)
				throw new MapNotFoundException((sourceType, destinationType));

			var newMappingOptions = MergeOrCreateNewMappingOptions(mappingOptions);

			var mapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			var factories = new CachedLazyEnumerable<IMergeMapFactory>(
				mapper.GetMergeMaps(mappingOptions)
					.Where(m => m.To == destinationType)
					.Select(mergeMap => {
						if(mergeMap.From == sourceType)
							return mapper.MapMergeFactory(mergeMap.From, mergeMap.To, mappingOptions);

						try {
							var newFactory = mapper.MapNewFactory(sourceType, mergeMap.From, newMappingOptions);

							try {
								var mergeFactory = mapper.MapMergeFactory(mergeMap.From, mergeMap.To, mappingOptions);

								try {
									return new DisposableMergeMapFactory(sourceType, destinationType,
										(source, destination) => {
											try {
												return mergeFactory.Invoke(newFactory.Invoke(source), destination);
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
						catch (MapNotFoundException) {
							return null;
						}
					})
					.Where(factory => factory != null));

			try {
				if (!factories.Any())
					throw new MapNotFoundException((sourceType, destinationType));

				return new DisposableMergeMapFactory(sourceType, destinationType,
					(source, destination) => {
						// Try using the factories, if any
						foreach (var factory in factories) {
							try {
								return factory.Invoke(source, destination);
							}
							catch (MapNotFoundException) { }
						}

						throw new MapNotFoundException((sourceType, destinationType));
					}, factories);
			}
			catch {
				factories.Dispose();
				throw;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private MappingOptions MergeOrCreateNewMappingOptions(MappingOptions options) {
			if (options == null || options == MappingOptions.Empty)
				return _optionsNewCacheNull;
			else if (options.Cached)
				return _optionsNewCache.GetOrAdd(options, MergeNewMappingOptions);
			else
				return MergeNewMappingOptions(options);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MappingOptions MergeNewMappingOptions(MappingOptions options) {
			return options.ReplaceOrAdd<TransitiveMappingOptions>(
				t => t?.MaxChainLength != null ? new TransitiveMappingOptions(Math.Max(t.MaxChainLength.Value - 1, 0)) : _newTransitiveOptions, options.Cached);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
