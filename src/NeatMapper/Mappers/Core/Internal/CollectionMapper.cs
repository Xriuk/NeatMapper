#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

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
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		protected readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		internal protected CollectionMapper(IMapper elementsMapper) {
			_elementsMapper = new CompositeMapper(elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper)), this);
			var nestedMappingContext = new NestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<MapperOverrideMappingOptions, NestedMappingContext>(
				m => m?.Mapper != null ? m : new MapperOverrideMappingOptions(_elementsMapper, m?.ServiceProvider),
				n => n != null ? new NestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext, options.Cached));
		}


		public abstract bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract bool CanMapNew(Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract object Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract object Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null);
	}
}
