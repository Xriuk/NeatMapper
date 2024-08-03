using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Transitive {
	/// <summary>
	/// <see cref="IMapper"/> which maps types transitively to existing ones by retrieving all the available maps
	/// from another <see cref="IMapper"/> (via <see cref="IMapperMaps"/> interface), a single merge map is retrieved
	/// and then the retrieved new maps are combined into a graph and the shortest path is solved by using
	/// the Dijkstra algorithm to map the source object, then the merge map is applied.
	/// </summary>
	public sealed class TransitiveMergeMapper : TransitiveMapper, IMapperCanMap, IMapperFactory {
		/// <summary>
		/// Creates a new instance of <see cref="TransitiveMergeMapper"/>.
		/// </summary>
		/// <param name="mapper">
		/// <see cref="IMapper"/> to use to merge map types.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions.Mapper"/>.
		/// </param>
		public TransitiveMergeMapper(IMapper mapper) : base(mapper, (mapper1, mappingOptions) => mapper1.GetNewMaps(mappingOptions)) {} // DEV: move to parent


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

			var mapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			// Retrieve all the merge maps with the selected destination type
			foreach (var mergeMap in mapper.GetMergeMaps(mappingOptions).Where(m => m.To == destinationType)) {
				try {
					// Map source with new map and then merge result with destination
					return mapper.Map(
						mapper.Map(source, sourceType, mergeMap.From, mappingOptions),
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// We cannot map identical types
			if (sourceType == destinationType)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			var mapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			// Retrieve all the merge maps with the selected destination type
			foreach (var mergeMap in mapper.GetMergeMaps(mappingOptions).Where(m => m.To == destinationType)) {
				// Try creating a new maps path to the retrieved source type of the merge map
				if (CanMapNew(sourceType, mergeMap.From, mappingOptions))
					return true;
			}

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

			var mapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;

			var factories = new CachedLazyEnumerable<IMergeMapFactory>(
				mapper.GetMergeMaps(mappingOptions)
					.Where(m => m.To == destinationType)
					.Select(mergeMap => {
						try {
							var newFactory = mapper.MapNewFactory(sourceType, mergeMap.From, mappingOptions);

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
					},
					factories);
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
	}
}
