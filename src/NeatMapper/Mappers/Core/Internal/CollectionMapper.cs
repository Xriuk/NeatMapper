#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Concurrent;

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
		/// Cached input <see cref="MappingOptions"/> and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MappingOptions> _optionsCache =
			new ConcurrentDictionary<MappingOptions, MappingOptions>();

		/// <summary>
		/// Cached output <see cref="MappingOptions"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have a null key).
		/// </summary>
		private readonly MappingOptions _optionsCacheNull;


		internal CollectionMapper(IMapper elementsMapper) {
			_elementsMapper = new CompositeMapper(elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper)), this);
			_nestedMappingContext = new NestedMappingContext(this);
			_optionsCacheNull = MergeOrCreateMappingOptions(MappingOptions.Empty, out _);
		}


		public abstract object Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract object Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null);


		// Will override the mapper if not already overridden
		protected MappingOptions MergeOrCreateMappingOptions(MappingOptions options, out MergeCollectionsMappingOptions mergeCollectionsMappingOptions) {
			if(options == null) {
				mergeCollectionsMappingOptions = null;
				return _optionsCacheNull;
			}
			else {
				mergeCollectionsMappingOptions = options.GetOptions<MergeCollectionsMappingOptions>();
				return _optionsCache.GetOrAdd(options, opts => opts
					.Replace<MergeCollectionsMappingOptions>(m => new MergeCollectionsMappingOptions(m.RemoveNotMatchedDestinationElements, null))
					.ReplaceOrAdd<MapperOverrideMappingOptions, NestedMappingContext>(
						m => m?.Mapper != null ? m : new MapperOverrideMappingOptions(_elementsMapper, m?.ServiceProvider),
						n => n != null ? new NestedMappingContext(_nestedMappingContext.ParentMapper, n) : _nestedMappingContext));
			}
		}
	}
}
