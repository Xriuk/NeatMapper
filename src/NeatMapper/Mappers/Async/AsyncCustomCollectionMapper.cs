#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Base class for asynchronous mappers which map collections by mapping elements with another <see cref="IAsyncMapper"/>
	/// </summary>
	public abstract class AsyncCustomCollectionMapper : IAsyncMapper {
		// Used as a nested mapper too, includes the collection mapper itself
		protected readonly IAsyncMapper _elementsMapper;
		protected readonly IServiceProvider _serviceProvider;

		internal AsyncCustomCollectionMapper(IAsyncMapper elementsMapper, IServiceProvider serviceProvider = null) {
			_elementsMapper = new AsyncCompositeMapper(elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper)), this);
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
		}


		public abstract Task<object> MapAsync(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default);
		public abstract Task<object> MapAsync(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default);
	}
}
