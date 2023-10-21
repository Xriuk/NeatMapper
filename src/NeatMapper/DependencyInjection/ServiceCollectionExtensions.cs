#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace NeatMapper {
	public static class ServiceCollectionExtensions {
		/// <summary>
		/// Adds <see cref="IMatcher"/> and <see cref="IMapper"/> to the services collection.<br/>
		/// To configure them you can use Configure&lt;MapperConfigurationOptions&gt;(...) and Configure&ltMapperOptions&gt;(...)
		/// </summary>
		/// <param name="mappersLifetime">lifetime of the <see cref="IMapper"/> service (and all the specific mappers registered to create it)</param>
		/// <returns>the same services collection so multiple calls could be chained</returns>
		public static IServiceCollection AddNeatMapper(this IServiceCollection services, ServiceLifetime mappersLifetime = ServiceLifetime.Scoped) {
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			// Configure composite mappers
			services.AddOptions();

			// Added to all options
			services.AddTransient<IConfigureOptions<CompositeMapperOptions>>(
				s => new ConfigureNamedOptions<CompositeMapperOptions, NewMapper, MergeMapper>(
					null,
					s.GetRequiredService<NewMapper>(),
					s.GetRequiredService<MergeMapper>(),
					(o, n, m) => {
						o.Mappers.Add(n);
						o.Mappers.Add(m);
					}
				)
			);
			// Added only to IMapper options
			services.AddOptions<CompositeMapperOptions>().Configure<NewCollectionMapper, MergeCollectionMapper>((o, n, m) => {
				o.Mappers.Add(n);
				o.Mappers.Add(m);
			});
			
			// Register mapper services

			// Normal mappers
			services.Add(new ServiceDescriptor(
				typeof(NewMapper),
				s => new NewMapper(
					s.GetService<IOptions<CustomMapsOptions>>()?.Value,
					s.GetService<IOptions<CustomNewAdditionalMapsOptions>>()?.Value,
					s),
				mappersLifetime));
			services.Add(new ServiceDescriptor(
				typeof(MergeMapper),
				s => new MergeMapper(
					s.GetService<IOptions<CustomMapsOptions>>()?.Value,
					s.GetService<IOptions<CustomMergeAdditionalMapsOptions>>()?.Value,
					s),
				mappersLifetime));

			// Collection mappers
			services.Add(new ServiceDescriptor(
				typeof(NewCollectionMapper),
				s => new NewCollectionMapper(
					new CompositeMapper(s.GetService<IOptionsMonitor<CompositeMapperOptions>>()?.Get(CompositeMapperOptions.Base).Mappers ?? Array.Empty<IMapper>()),
					s),
				mappersLifetime));
			services.Add(new ServiceDescriptor(
				typeof(MergeCollectionMapper),
				s => new MergeCollectionMapper(
					new CompositeMapper(s.GetService<IOptionsMonitor<CompositeMapperOptions>>()?.Get(CompositeMapperOptions.Base).Mappers ?? Array.Empty<IMapper>()),
					s.GetService<IMatcher>(),
					s.GetService<IOptions<MergeCollectionsOptions>>()?.Value,
					s),
				mappersLifetime));

			// IMapper (composite mapper)
			services.Add(new ServiceDescriptor(
				typeof(IMapper),
				s => new CompositeMapper(s.GetService<IOptions<CompositeMapperOptions>>()?.Value.Mappers ?? Array.Empty<IMapper>()),
				mappersLifetime));

			return services;
		}
	}
}
