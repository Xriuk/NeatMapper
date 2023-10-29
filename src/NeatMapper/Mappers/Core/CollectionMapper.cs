#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// Base class for mappers which map collections by mapping elements with another <see cref="IMapper"/>
	/// </summary>
	public abstract class CollectionMapper : IMapper {
		// Used as a nested mapper too, includes the collection mapper itself
		protected readonly IMapper _elementsMapper;
		protected readonly IServiceProvider _serviceProvider;

		internal CollectionMapper(IMapper elementsMapper, IServiceProvider serviceProvider = null) {
			_elementsMapper = new CompositeMapper(elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper)), this);
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
		}


		public abstract object Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract object Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null);


		// Will override a mapper if not already overridden
		protected MappingOptions MergeOrCreateMappingOptions(MappingOptions options, out MergeCollectionsMappingOptions mergeCollectionsMappingOptions) {
			var overrideOptions = options?.GetOptions<MapperOverrideMappingOptions>();
			mergeCollectionsMappingOptions = options?.GetOptions<MergeCollectionsMappingOptions>();
			if (overrideOptions == null) {
				overrideOptions = new MapperOverrideMappingOptions();
				if (options != null) {
					options = new MappingOptions(options.AsEnumerable().Select(o => {
						if (o is MergeCollectionsMappingOptions merge) {
							var mergeOpts = new MergeCollectionsMappingOptions(merge) {
								Matcher = null
							};
							return mergeOpts;
						}
						else
							return o;
					}).Concat(new[] { overrideOptions }));
				}
				else
					options = new MappingOptions(new[] { overrideOptions });
			}
			else {
				options = new MappingOptions(options.AsEnumerable().Select(o => {
					if (o is MergeCollectionsMappingOptions merge) {
						var mergeOpts = new MergeCollectionsMappingOptions(merge) {
							Matcher = null
						};
						return mergeOpts;
					}
					else
						return o;
				}));
			}

			if(overrideOptions.Mapper == null)
				overrideOptions.Mapper = _elementsMapper;

			return options;
		}
	}
}
