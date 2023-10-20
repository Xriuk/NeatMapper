using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NeatMapper.Configuration;
using System;
using System.Linq;

namespace NeatMapper {
	public static class ServiceCollectionExtensions {
		/// <summary>
		/// Adds <see cref="IMatcher"/> and <see cref="IMapper"/> to the services collection.<br/>
		/// To configure them you can use Configure&lt;MapperConfigurationOptions&gt;(...) and Configure&ltMapperOptions&gt;(...)
		/// </summary>
		/// <param name="mapperLifetime">lifetime of the <see cref="IMapper"/> service</param>
		/// <param name="matcherLifetime">lifetime of the <see cref="IMatcher"/> service</param>
		/// <returns>the same services collection so multiple calls could be chained</returns>
		public static IServiceCollection AddNeatMapper(this IServiceCollection services, ServiceLifetime mapperLifetime = ServiceLifetime.Scoped, ServiceLifetime matcherLifetime = ServiceLifetime.Scoped) {
			if(services == null)
				throw new ArgumentNullException(nameof(services));
			
			services.AddOptions();
			
			bool hasMatcher = services.Any(s => s.ServiceType == typeof(IMatcher));
			
			services.Add(new ServiceDescriptor(
				typeof(Mapper),
				s => new Mapper(
					s.GetService<IOptions<MapperConfigurationOptions>>()?.Value ?? new MapperConfigurationOptions(),
					s.GetService<IOptions<CustomMapperOptions>>()?.Value,
					s),
				hasMatcher || (mapperLifetime == matcherLifetime && (mapperLifetime == ServiceLifetime.Singleton || mapperLifetime == ServiceLifetime.Scoped)) ?
					mapperLifetime :
					ServiceLifetime.Transient
			));
			services.Add(new ServiceDescriptor(
				typeof(IMapper),
				s => s.GetRequiredService<Mapper>(),
				mapperLifetime
			));

			// In case it wasn't already registered by NeatMapper.Async.DependencyInjection
			if (!hasMatcher) {
				services.Add(new ServiceDescriptor(
					typeof(IMatcher),
					s => s.GetRequiredService<Mapper>(),
					matcherLifetime
				));
			}

			return services;
		}
	}
}
