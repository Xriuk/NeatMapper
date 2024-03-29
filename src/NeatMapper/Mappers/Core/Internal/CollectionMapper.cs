﻿#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

namespace NeatMapper {
	/// <summary>
	/// Base class for mappers which map collections by mapping elements with another <see cref="IMapper"/>.
	/// Internal class.
	/// </summary>
	public abstract class CollectionMapper : IMapper {
		// Used as a nested mapper too, includes the collection mapper itself
		protected readonly IMapper _elementsMapper;
		private readonly NestedMappingContext _nestedMappingContext;

		internal CollectionMapper(IMapper elementsMapper) {
			_elementsMapper = new CompositeMapper(elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper)), this);
			_nestedMappingContext = new NestedMappingContext(this);
		}


		public abstract object Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract object Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null);


		// Will override a mapper if not already overridden
		protected MappingOptions MergeOrCreateMappingOptions(MappingOptions options, bool isRealFactory, out MergeCollectionsMappingOptions mergeCollectionsMappingOptions) {
			// DEV: replace below with TryAdd (which should not alter options if nothing changes)
			mergeCollectionsMappingOptions = options?.GetOptions<MergeCollectionsMappingOptions>();
			return (options ?? MappingOptions.Empty)
				.Replace<MergeCollectionsMappingOptions>(m => new MergeCollectionsMappingOptions(m.RemoveNotMatchedDestinationElements, null))
				.ReplaceOrAdd<MapperOverrideMappingOptions, NestedMappingContext, FactoryContext>(
					m => m?.Mapper != null ? m : new MapperOverrideMappingOptions(_elementsMapper, m?.ServiceProvider),
					n => n != null ? new NestedMappingContext(_nestedMappingContext.ParentMapper, n) : _nestedMappingContext,
					f => isRealFactory ? FactoryContext.Instance : f);
		}
	}
}
