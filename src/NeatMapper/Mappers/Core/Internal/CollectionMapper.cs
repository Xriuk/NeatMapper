#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	/// <summary>
	/// Base class for mappers which map collections by mapping elements with another <see cref="IMapper"/>.
	/// Internal class.
	/// </summary>
	public abstract class CollectionMapper : IMapper {
		/// <summary>
		/// <see cref="IMapper"/> which is used to map the elements of the collections, will be also provided
		/// as a nested mapper in <see cref="MapperOverrideMappingOptions"/> (if not already present).
		/// </summary>
		protected readonly IMapper _elementsMapper;

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


		internal CollectionMapper(IMapper elementsMapper) {
			_elementsMapper = new CompositeMapper(elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper)), this);
			_nestedMappingContext = new NestedMappingContext(this);
			_optionsCacheNull = MergeMappingOptions(MappingOptions.Empty);
		}


		public abstract object Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract object Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null);


		// Will override the mapper if not already overridden
		protected MappingOptions MergeOrCreateMappingOptions(MappingOptions options, out MergeCollectionsMappingOptions mergeCollectionsMappingOptions) {
			if(options == null || options == MappingOptions.Empty) {
				mergeCollectionsMappingOptions = null;
				return _optionsCacheNull;
			}
			else {
				mergeCollectionsMappingOptions = options.GetOptions<MergeCollectionsMappingOptions>();
				if(options.Cached)
					return _optionsCache.GetOrAdd(options, MergeMappingOptions);
				else
					return MergeMappingOptions(options);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MappingOptions MergeMappingOptions(MappingOptions options) {
			// Caching only ReplaceOrAdd (if options are cached aswell) as the first Replace is discarded
			return options
				.Replace<MergeCollectionsMappingOptions>(m => new MergeCollectionsMappingOptions(m.RemoveNotMatchedDestinationElements, null), false)
				.ReplaceOrAdd<MapperOverrideMappingOptions, NestedMappingContext>(
					m => m?.Mapper != null ? m : new MapperOverrideMappingOptions(_elementsMapper, m?.ServiceProvider),
					n => n != null ? new NestedMappingContext(_nestedMappingContext.ParentMapper, n) : _nestedMappingContext, options.Cached);
		}
	}
}
