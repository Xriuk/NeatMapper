#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace NeatMapper.EntityFrameworkCore {
	public static class ServiceCollectionExtensions {
		/// <inheritdoc cref="AddNeatMapperEntityFrameworkCore{TContext}(IServiceCollection, IModel)"/>
		public static IServiceCollection AddNeatMapperEntityFrameworkCore<TContext>(this IServiceCollection services) where TContext : DbContext {
			if (services == null)
				throw new ArgumentNullException(nameof(services));
			if(typeof(TContext) == typeof(DbContext))
				throw new ArgumentException("The provided type must derive from DbContext.");

			
			TContext instance;
			try {
				instance = ObjectFactory.Create(typeof(TContext)) as TContext;
			}
			catch {
				if(!services.Any(s => s.ServiceType == typeof(TContext)))
					throw new InvalidOperationException("Entity Framework Core mappers must be added after the EF Core package.");

				using (var serviceProvider = services.BuildServiceProvider())
				using (var scope = serviceProvider.CreateScope()) {
					return services.AddNeatMapperEntityFrameworkCore<TContext>(scope.ServiceProvider.GetRequiredService<TContext>().Model);
				}
			}
			using (instance) {
				return services.AddNeatMapperEntityFrameworkCore<TContext>(instance.Model);
			}
		}

		/// <summary>
		/// Adds Entity Framework Core mappers, matcher and projector to the services collection.
		/// The lifetime of the services will match the ones specified in the core NeatMapper package.
		/// </summary>
		/// <remarks>
		/// Must be called after adding the core NeatMapper package with
		/// <see cref="NeatMapper.ServiceCollectionExtensions.AddNeatMapper(IServiceCollection, ServiceLifetime, ServiceLifetime, ServiceLifetime, ServiceLifetime)"/>,
		/// and also after adding the EF Core package with
		/// <see cref="EntityFrameworkServiceCollectionExtensions.AddDbContext{TContext}(IServiceCollection, ServiceLifetime, ServiceLifetime)"/> or other overloads.
		/// </remarks>
		/// <typeparam name="TContext">Type of the DbContext to use with the mapper.</typeparam>
		/// <returns>The same services collection so multiple calls could be chained.</returns>
		public static IServiceCollection AddNeatMapperEntityFrameworkCore<TContext>(this IServiceCollection services, IModel model) where TContext : DbContext {
			if (services == null)
				throw new ArgumentNullException(nameof(services));
			if (model == null)
				throw new ArgumentNullException(nameof(model));
			if (typeof(TContext) == typeof(DbContext))
				throw new ArgumentException("The provided type must derive from DbContext.");

			var mapper = services.FirstOrDefault(s => s.ServiceType == typeof(IMapper))
				?? throw new InvalidOperationException("Entity Framework Core mappers must be added after the core package.");
			var asyncMapper = services.FirstOrDefault(s => s.ServiceType == typeof(IAsyncMapper))
				?? throw new InvalidOperationException("Entity Framework Core mappers must be added after the core package.");
			var matcher = services.FirstOrDefault(s => s.ServiceType == typeof(IMatcher))
				?? throw new InvalidOperationException("Entity Framework Core mappers must be added after the core package.");
			var projector = services.FirstOrDefault(s => s.ServiceType == typeof(IProjector))
				?? throw new InvalidOperationException("Entity Framework Core mappers must be added after the core package.");

			#region IMatcher
			// Add matcher to composite matcher
			services.AddOptions<CompositeMatcherOptions>()
				.Configure<EntityFrameworkCoreMatcher>((o, m) => {
					o.Matchers.Add(m);
				});

			services.Add(new ServiceDescriptor(
				typeof(EntityFrameworkCoreMatcher),
				s => new EntityFrameworkCoreMatcher(model, typeof(TContext), s),
				matcher.Lifetime));
			#endregion

			#region IMapper
			// Add mapper to composite mapper
			services.AddOptions<CompositeMapperOptions>()
				.Configure<EntityFrameworkCoreMapper>((o, m) => {
					// Try adding before collection mappers
					var collectionMapper = o.Mappers.OfType<CollectionMapper>().FirstOrDefault();
					if (collectionMapper != null)
						o.Mappers.Insert(o.Mappers.IndexOf(collectionMapper), m);
					else
						o.Mappers.Add(m);
				});

			services.Add(new ServiceDescriptor(
				typeof(EntityFrameworkCoreMapper),
				s => new EntityFrameworkCoreMapper(
					model,
					typeof(TContext),
					s,
					s.GetService<IOptionsSnapshot<EntityFrameworkCoreOptions>>()?.Value,
					s.GetService<IMatcher>(),
					s.GetService<IOptionsSnapshot<MergeCollectionsOptions>>()?.Value),
				mapper.Lifetime));
			#endregion

			#region IAsyncMapper
			// Add mapper to composite mapper
			services.AddOptions<AsyncCompositeMapperOptions>()
				.Configure<AsyncEntityFrameworkCoreMapper>((o, m) => {
					// Try adding before collection mappers
					var collectionMapper = o.Mappers.OfType<AsyncCollectionMapper>().FirstOrDefault();
					if (collectionMapper != null)
						o.Mappers.Insert(o.Mappers.IndexOf(collectionMapper), m);
					else
						o.Mappers.Add(m);
				});

			services.Add(new ServiceDescriptor(
				typeof(AsyncEntityFrameworkCoreMapper),
				s => new AsyncEntityFrameworkCoreMapper(
					model,
					typeof(TContext),
					s,
					s.GetService<IOptionsSnapshot<EntityFrameworkCoreOptions>>()?.Value,
					s.GetService<IMatcher>(),
					s.GetService<IOptionsSnapshot<MergeCollectionsOptions>>()?.Value),
				asyncMapper.Lifetime));
			#endregion

			#region IProjector
			// Add projector to composite projector
			services.AddOptions<CompositeProjectorOptions>()
				.Configure<EntityFrameworkCoreProjector>((o, p) => {
					o.Projectors.Add(p);
				});

			services.Add(new ServiceDescriptor(
				typeof(EntityFrameworkCoreProjector),
				s => new EntityFrameworkCoreProjector(model, typeof(TContext)),
				projector.Lifetime));
			#endregion

			return services;
		}
	}
}
