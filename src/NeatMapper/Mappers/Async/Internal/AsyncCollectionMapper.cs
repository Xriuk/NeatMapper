﻿#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Base class for asynchronous mappers which map collections by mapping elements with another
	/// <see cref="IAsyncMapper"/>.
	/// Internal class.
	/// </summary>
	public abstract class AsyncCollectionMapper : IAsyncMapper {
		// Used as a nested mapper too, includes the collection mapper itself
		protected readonly IAsyncMapper _elementsMapper;
		protected readonly AsyncCollectionMappersOptions _asyncCollectionMappersOptions;
		private readonly AsyncNestedMappingContext _nestedMappingContext;

		internal AsyncCollectionMapper(IAsyncMapper elementsMapper, AsyncCollectionMappersOptions asyncCollectionMappersOptions = null) {
			_elementsMapper = new AsyncCompositeMapper(elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper)), this);
			_asyncCollectionMappersOptions = asyncCollectionMappersOptions ?? new AsyncCollectionMappersOptions();
			_nestedMappingContext = new AsyncNestedMappingContext(this);
		}


		public abstract Task<object> MapAsync(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default);
		public abstract Task<object> MapAsync(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default);


		// Will override a mapper if not already overridden
		protected MappingOptions MergeOrCreateMappingOptions(MappingOptions options, bool isRealFactory, out MergeCollectionsMappingOptions mergeCollectionsMappingOptions) {
			mergeCollectionsMappingOptions = options?.GetOptions<MergeCollectionsMappingOptions>();
			return (options ?? MappingOptions.Empty)
				.Replace<MergeCollectionsMappingOptions>(m => new MergeCollectionsMappingOptions(m.RemoveNotMatchedDestinationElements, null))
				.ReplaceOrAdd<AsyncMapperOverrideMappingOptions, AsyncNestedMappingContext, FactoryContext>(
					m => m?.Mapper != null ? m : new AsyncMapperOverrideMappingOptions(_elementsMapper, m?.ServiceProvider),
					n => n != null ? new AsyncNestedMappingContext(this, n) : _nestedMappingContext,
					f => isRealFactory ? FactoryContext.Instance : f);
		}
	}
}
