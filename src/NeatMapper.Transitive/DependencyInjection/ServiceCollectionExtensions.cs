#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace NeatMapper.Transitive {
	public static class ServiceCollectionExtensions {
		/// <summary>
		/// Adds Transitive mappers and projector to the services collection.
		/// The lifetime of the services will match the ones specified in the core NeatMapper package.
		/// </summary>
		/// <remarks>
		/// Must be called after adding the core NeatMapper package with
		/// <see cref="NeatMapper.ServiceCollectionExtensions.AddNeatMapper(IServiceCollection, ServiceLifetime, ServiceLifetime, ServiceLifetime, ServiceLifetime)"/>.
		/// </remarks>
		/// <returns>The same services collection so multiple calls could be chained.</returns>
		public static IServiceCollection AddNeatMapperTransitive(this IServiceCollection services)  {
			if (services == null)
				throw new ArgumentNullException(nameof(services));

			var mapper = services.FirstOrDefault(s => s.ServiceType == typeof(IMapper))
				?? throw new InvalidOperationException("Transitive mappers must be added after the core package.");

			#region IMapper
			// Add mappers to composite mapper
			// Creating mappers with EmptyMapper to avoid recursion, the nested mapper will be overridden by composite mapper
			services.AddOptions<CompositeMapperOptions>()
				.Configure(o => {
					o.Mappers.Add(new TransitiveNewMapper(EmptyMapper.Instance));
					o.Mappers.Add(new TransitiveMergeMapper(EmptyMapper.Instance));
				});

			services.Add(new ServiceDescriptor(
				typeof(TransitiveNewMapper),
				s => new TransitiveNewMapper(
					s.GetRequiredService<IMapper>(),
					s.GetService<IOptionsSnapshot<TransitiveOptions>>()?.Value),
				mapper.Lifetime));
			services.Add(new ServiceDescriptor(
				typeof(TransitiveMergeMapper),
				s => new TransitiveMergeMapper(
					s.GetRequiredService<IMapper>(),
					s.GetService<IOptionsSnapshot<TransitiveOptions>>()?.Value),
				mapper.Lifetime));
			#endregion

			return services;
		}
	}
}
