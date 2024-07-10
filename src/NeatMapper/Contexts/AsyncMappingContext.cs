#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;

namespace NeatMapper {
	/// <summary>
	/// Contains informations and services for the current asynchronous mapping operation.
	/// </summary>
	public sealed class AsyncMappingContextOptions {
		private readonly Lazy<IAsyncMapper> _mapper;

		public AsyncMappingContextOptions(
			IServiceProvider serviceProvider,
			IAsyncMapper mapper,
			MappingOptions mappingOptions) :
				this(serviceProvider, mapper, mapper, mappingOptions) { }
		public AsyncMappingContextOptions(
			IServiceProvider serviceProvider,
			IAsyncMapper nestedMapper,
			IAsyncMapper parentMapper,
			MappingOptions mappingOptions) {

			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

			if (parentMapper == null)
				throw new ArgumentNullException(nameof(parentMapper));

			_mapper = new Lazy<IAsyncMapper>(() => {
				var nestedMappingContext = new AsyncNestedMappingContext(parentMapper);
				return new AsyncNestedMapper(nestedMapper, o => (o ?? MappingOptions.Empty)
					.ReplaceOrAdd<AsyncNestedMappingContext>(
						n => n != null ? new AsyncNestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext, (o ?? MappingOptions.Empty).Cached));
			}, true);

			MappingOptions = mappingOptions ?? throw new ArgumentNullException(nameof(mappingOptions));
		}


		/// <summary>
		/// Service provider which can be used to retrieve additional services.
		/// </summary>
		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Mapper which can be used for nested mappings. <see cref="MappingOptions"/> are not automatically forwarded.<br/>
		/// The only option forwarded automatically is <see cref="AsyncNestedMappingContext"/>.
		/// </summary>
		public IAsyncMapper Mapper => _mapper.Value;

		/// <summary>
		/// Additional mapping options, contains multiple options of different types,
		/// each mapper/map should try to retrieve its options and use them.
		/// </summary>
		public MappingOptions MappingOptions { get; }
	}

	/// <summary>
	/// Contains informations and services for the current asynchronous mapping operation.
	/// This is an optimized value type which allows to share the same <see cref="AsyncMappingContextOptions"/>
	/// while changing <see cref="CancellationToken"/> for each map.
	/// </summary>
	public readonly struct AsyncMappingContext {
		private readonly AsyncMappingContextOptions _options;

		public AsyncMappingContext(AsyncMappingContextOptions options, CancellationToken cancellationToken) {
			_options = options;
			CancellationToken = cancellationToken;
		}


		/// <summary>
		/// Service provider which can be used to retrieve additional services.
		/// </summary>
		public 
#if NET5_0_OR_GREATER
			readonly 
#endif
			IServiceProvider ServiceProvider => _options.ServiceProvider;

		/// <summary>
		/// Mapper which can be used for nested mappings. <see cref="MappingOptions"/> are not automatically forwarded.<br/>
		/// The only option forwarded automatically is <see cref="AsyncNestedMappingContext"/>.
		/// </summary>
		public
#if NET5_0_OR_GREATER
			readonly
#endif
			IAsyncMapper Mapper => _options.Mapper;

		/// <summary>
		/// Additional mapping options, contains multiple options of different types,
		/// each mapper/map should try to retrieve its options and use them.
		/// </summary>
		public
#if NET5_0_OR_GREATER
			readonly
#endif
			MappingOptions MappingOptions => _options.MappingOptions;

		/// <summary>
		/// Cancellation token of the mapping which should be passed to all the async methods inside the maps.
		/// </summary>
		public
#if NET5_0_OR_GREATER
			readonly
#endif
			CancellationToken CancellationToken { get; }
	}
}
