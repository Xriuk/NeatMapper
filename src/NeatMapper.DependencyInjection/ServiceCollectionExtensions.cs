using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NeatMapper;
using NeatMapper.Configuration;

namespace Microsoft.Extensions.DependencyInjection {
	public static class ServiceCollectionExtensions {
		/// <summary>
		/// Adds <see cref="IMatcher"/> and <see cref="IMapper"/> to the services collection.<br/>
		/// To configure them you can use Configure<MapperConfigurationOptions>(...)
		/// </summary>
		/// <param name="mapperLifetime">lifetime of the <see cref="IMapper"/> service</param>
		/// <param name="matcherLifetime">lifetime of the <see cref="IMatcher"/> service</param>
		/// <returns>the same services collection so multiple calls could be chained</returns>
		public static IServiceCollection AddNeatMapper(this IServiceCollection services, ServiceLifetime mapperLifetime = ServiceLifetime.Scoped, ServiceLifetime matcherLifetime = ServiceLifetime.Scoped) {
			services.TryAdd(new ServiceDescriptor(
				typeof(IMatcher),
				s => new Mapper(s.GetRequiredService<IOptions<MapperConfigurationOptions>>().Value, s),
				matcherLifetime
			));
			services.Add(new ServiceDescriptor(
				typeof(IMapper),
				s => new Mapper(s.GetRequiredService<IOptions<MapperConfigurationOptions>>().Value, s),
				mapperLifetime
			));

			return services;
		}
	}
}
