using System;

namespace NeatMapper {
	/// <summary>
	/// Contains informations and services for the current mapping operation.
	/// </summary>
	public sealed class MappingContext {
		private readonly Lazy<IMapper> _mapper;

		public MappingContext(
			IServiceProvider serviceProvider,
			IMapper mapper,
			MappingOptions mappingOptions) :
				this(serviceProvider, mapper, mapper, mappingOptions) {}
		public MappingContext(
			IServiceProvider serviceProvider,
			IMapper nestedMapper,
			IMapper parentMapper,
			MappingOptions mappingOptions) {

			ServiceProvider = serviceProvider
				?? throw new ArgumentNullException(nameof(serviceProvider));

			if(parentMapper == null)
				throw new ArgumentNullException(nameof(parentMapper));

			_mapper = new Lazy<IMapper>(() => {
				var nestedMappingContext = new NestedMappingContext(parentMapper);
				return new NestedMapper(nestedMapper, o => o.ReplaceOrAdd<NestedMappingContext>(
					n => n != null ? new NestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext, o.Cached));
			}, true);

			MappingOptions = mappingOptions
				?? throw new ArgumentNullException(nameof(mappingOptions));
		}


		/// <summary>
		/// Service provider which can be used to retrieve additional services.
		/// </summary>
		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Mapper which can be used for nested mappings. <see cref="MappingOptions"/> are not automatically forwarded.<br/>
		/// The only option forwarded automatically is <see cref="NestedMappingContext"/>.
		/// </summary>
		public IMapper Mapper => _mapper.Value;

		/// <summary>
		/// Additional mapping options, contains multiple options of different types,
		/// each mapper/map should try to retrieve its options and use them.
		/// </summary>
		public MappingOptions MappingOptions { get; }
	}
}
