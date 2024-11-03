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
		public static IServiceCollection AddNeatMapperTransitive(this IServiceCollection services) {
			if (services == null)
				throw new ArgumentNullException(nameof(services));

			var mapper = services.FirstOrDefault(s => s.ServiceType == typeof(IMapper))
				?? throw new InvalidOperationException("Transitive mappers must be added after the core package.");
			var asyncMapper = services.FirstOrDefault(s => s.ServiceType == typeof(IAsyncMapper))
				?? throw new InvalidOperationException("Transitive mappers must be added after the core package.");
			var projector = services.FirstOrDefault(s => s.ServiceType == typeof(IProjector))
				?? throw new InvalidOperationException("Transitive mappers must be added after the core package.");

			#region IMapper
			// Add mapper to composite mapper
			// Creating mapper with EmptyMapper to avoid recursion, the nested mapper will be overridden by composite mapper
			services.AddOptions<CompositeMapperOptions>()
				.Configure(o => {
					o.Mappers.Add(new TransitiveMapper(EmptyMapper.Instance));
				});

			services.Add(new ServiceDescriptor(
				typeof(TransitiveMapper),
				s => new TransitiveMapper(
					s.GetRequiredService<IMapper>(),
					s.GetService<IOptionsSnapshot<TransitiveOptions>>()?.Value),
				mapper.Lifetime));
			#endregion

			#region IAsyncMapper
			// Add async mapper to composite async mapper
			// Creating mapper with AsyncEmptyMapper to avoid recursion, the nested mapper will be overridden by composite mapper
			services.AddOptions<AsyncCompositeMapperOptions>()
				.Configure(o => {
					o.Mappers.Add(new AsyncTransitiveMapper(AsyncEmptyMapper.Instance));
				});

			services.Add(new ServiceDescriptor(
				typeof(AsyncTransitiveMapper),
				s => new AsyncTransitiveMapper(
					s.GetRequiredService<IAsyncMapper>(),
					s.GetService<IOptionsSnapshot<TransitiveOptions>>()?.Value),
				asyncMapper.Lifetime));
			#endregion

			#region IProjector
			// Add projector to composite projector
			// Creating projector with EmptyProjector to avoid recursion, the nested projector will be overridden by composite projector
			services.AddOptions<CompositeProjectorOptions>()
				.Configure(o => {
					o.Projectors.Add(new TransitiveProjector(EmptyProjector.Instance));
				});

			services.Add(new ServiceDescriptor(
				typeof(TransitiveProjector),
				s => new TransitiveProjector(
					s.GetRequiredService<IProjector>(),
					s.GetService<IOptionsSnapshot<TransitiveOptions>>()?.Value),
				projector.Lifetime));
			#endregion

			return services;
		}
	}
}
