#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

namespace NeatMapper {
	/// <summary>
	/// Base class for mappers which map collections by mapping elements with another <see cref="IMapper"/>
	/// </summary>
	public abstract class CustomCollectionMapper : IMapper {
		// Used as a nested mapper too, includes the collection mapper itself
		protected readonly IMapper _elementsMapper;
		protected readonly IServiceProvider _serviceProvider;

		internal CustomCollectionMapper(IMapper elementsMapper, IServiceProvider serviceProvider = null) {
			_elementsMapper = new CompositeMapper(elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper)), this);
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
		}


		public abstract object Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract object Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null);
	}
}
