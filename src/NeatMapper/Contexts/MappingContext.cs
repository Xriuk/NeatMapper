#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

namespace NeatMapper {
	/// <summary>
	/// Contains informations and services for the current mapping operation.
	/// </summary>
	public sealed class MappingContext {
		private readonly Lazy<IMapper> _mapper;

		public MappingContext(IServiceProvider serviceProvider, IMapper mapper, MappingOptions mappingOptions) : this(serviceProvider, mapper, mapper, mappingOptions) {}
		public MappingContext(IServiceProvider serviceProvider, IMapper nestedMapper, IMapper parentMapper, MappingOptions mappingOptions) {
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

			var nestedMappingContext = new NestedMappingContext(parentMapper ?? throw new ArgumentNullException(nameof(parentMapper)));
			var nestedMapperInstance = new NestedMapper(nestedMapper, o => (o ?? MappingOptions.Empty)
				.ReplaceOrAdd<NestedMappingContext, FactoryContext>(
					n => n != null ? new NestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext,
					_ => FactoryContext.Instance));
			_mapper = new Lazy<IMapper>(() => MappingOptions.GetOptions<FactoryContext>() != null ?
				(IMapper)new CachedFactoryMapper(nestedMapperInstance) :
				nestedMapperInstance);

			MappingOptions = mappingOptions ?? throw new ArgumentNullException(nameof(mappingOptions));
		}


		/// <summary>
		/// Service provider which can be used to retrieve additional services.
		/// </summary>
		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Mapper which can be used for nested mappings. <see cref="MappingOptions"/> are not automatically forwarded.<br/>
		/// The only options forwarded automatically are <see cref="NestedMappingContext"/> and <see cref="FactoryContext"/>.
		/// </summary>
		public IMapper Mapper => _mapper.Value;

		/// <summary>
		/// Additional mapping options, contains multiple options of different types,
		/// each mapper/map should try to retrieve its options and use them.
		/// </summary>
		public MappingOptions MappingOptions { get; }
	}
}
