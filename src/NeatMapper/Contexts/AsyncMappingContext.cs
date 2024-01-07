#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;

namespace NeatMapper {
	/// <summary>
	/// Contains informations and services for the current asynchronous mapping operation.
	/// </summary>
	public sealed class AsyncMappingContext {
		private readonly Lazy<IAsyncMapper> _mapper;

		public AsyncMappingContext(IServiceProvider serviceProvider, IAsyncMapper mapper, MappingOptions mappingOptions, CancellationToken cancellationToken) :
			this(serviceProvider, mapper, mapper, mappingOptions, cancellationToken) { }
		public AsyncMappingContext(IServiceProvider serviceProvider, IAsyncMapper nestedMapper, IAsyncMapper parentMapper, MappingOptions mappingOptions, CancellationToken cancellationToken) {
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

			var nestedMappingContext = new AsyncNestedMappingContext(parentMapper ?? throw new ArgumentNullException(nameof(parentMapper)));
			var nestedMapperInstance = new AsyncNestedMapper(nestedMapper, o => (o ?? MappingOptions.Empty)
				.ReplaceOrAdd<AsyncNestedMappingContext, FactoryContext>(
					n => n != null ? new AsyncNestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext,
					_ => FactoryContext.Instance));
			_mapper = new Lazy<IAsyncMapper>(() => MappingOptions.GetOptions<FactoryContext>() != null ?
				(IAsyncMapper)new AsyncCachedFactoryMapper(nestedMapperInstance) :
				nestedMapperInstance);

			MappingOptions = mappingOptions ?? throw new ArgumentNullException(nameof(mappingOptions));
			CancellationToken = cancellationToken;
		}


		/// <summary>
		/// Service provider which can be used to retrieve additional services.
		/// </summary>
		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Mapper which can be used for nested mappings. <see cref="MappingOptions"/> are not automatically forwarded.<br/>
		/// The only options forwarded automatically are <see cref="AsyncNestedMappingContext"/> and <see cref="FactoryContext"/>.
		/// </summary>
		public IAsyncMapper Mapper => _mapper.Value;

		/// <summary>
		/// Additional mapping options, contains multiple options of different types,
		/// each mapper/map should try to retrieve its options and use them.
		/// </summary>
		public MappingOptions MappingOptions { get; }

		/// <summary>
		/// Cancellation token of the mapping which should be passed to all the async methods inside the maps.
		/// </summary>
		public CancellationToken CancellationToken { get; }
	}
}
