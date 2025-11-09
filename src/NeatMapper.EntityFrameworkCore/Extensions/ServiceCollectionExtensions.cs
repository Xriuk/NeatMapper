using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NeatMapper.EntityFrameworkCore {
	public static class ServiceCollectionExtensions {
		private class IModelRetriever<TContext> where TContext : DbContext {
			private readonly IServiceProvider _serviceProvider;


			private IModel? _model;
			public IModel Model {
				get {
					if(_model == null) {
						using (var scope = _serviceProvider.CreateScope()) {
							_model = scope.ServiceProvider.GetRequiredService<TContext>().Model;
						}
					}
					return _model;
				}
			}

			public IModelRetriever(IServiceProvider serviceProvider) {
				_serviceProvider = serviceProvider;
			}
			public IModelRetriever(IModel model) {
				_serviceProvider = null!;
				_model = model;
			}
		}


		/// <inheritdoc cref="AddNeatMapperEntityFrameworkCore{TContext}(IServiceCollection, IModel)"/>
		public static IServiceCollection AddNeatMapperEntityFrameworkCore<TContext>(this IServiceCollection services) where TContext : DbContext {
			if (services == null)
				throw new ArgumentNullException(nameof(services));
			if(typeof(TContext) == typeof(DbContext))
				throw new ArgumentException("The provided type must derive from DbContext.");

			TContext instance;
			try {
				instance = ObjectFactory.Create(typeof(TContext)) as TContext
					?? throw new Exception();
				using (instance) { 
					services.AddSingleton(new IModelRetriever<TContext>(instance.Model));
				}
			}
			catch {
				services.AddSingleton(s => new IModelRetriever<TContext>(s));
			}

			return AddNeatMapperEntityFrameworkCoreInternal<TContext>(services);
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
		public static IServiceCollection AddNeatMapperEntityFrameworkCore<TContext>(IServiceCollection services, IModel model) where TContext : DbContext {
			if (services == null)
				throw new ArgumentNullException(nameof(services));
			if (model == null)
				throw new ArgumentNullException(nameof(model));
			if (typeof(TContext) == typeof(DbContext))
				throw new ArgumentException("The provided type must derive from DbContext.");

			services.AddSingleton(new IModelRetriever<TContext>(model));

			return AddNeatMapperEntityFrameworkCoreInternal<TContext>(services);
		}


		private static IServiceCollection AddNeatMapperEntityFrameworkCoreInternal<TContext>(this IServiceCollection services) where TContext : DbContext {
			var mapper = services.FirstOrDefault(s => s.ServiceType == typeof(IMapper))
				?? throw new InvalidOperationException("NeatMapper.EntityFrameworkCore package must be added after the core NeatMapper package.");
			var asyncMapper = services.FirstOrDefault(s => s.ServiceType == typeof(IAsyncMapper))
				?? throw new InvalidOperationException("NeatMapper.EntityFrameworkCore package must be added after the core NeatMapper package.");
			var matcher = services.FirstOrDefault(s => s.ServiceType == typeof(IMatcher))
				?? throw new InvalidOperationException("NeatMapper.EntityFrameworkCore package must be added after the core NeatMapper package.");
			var projector = services.FirstOrDefault(s => s.ServiceType == typeof(IProjector))
				?? throw new InvalidOperationException("NeatMapper.EntityFrameworkCore package must be added after the core NeatMapper package.");

			#region IMatcher
			// Add matcher to composite matcher
			services.AddOptions<CompositeMatcherOptions>()
				.Configure<EntityFrameworkCoreMatcher>((o, m) => {
					o.Matchers.Add(m);
				});

			services.Add(new ServiceDescriptor(
				typeof(EntityFrameworkCoreMatcher),
				s => new EntityFrameworkCoreMatcher(s.GetRequiredService<IModelRetriever<TContext>>().Model, typeof(TContext), s),
				matcher.Lifetime));
			#endregion

			#region IMapper
			// Add mapper to composite mapper
			services.AddOptions<CompositeMapperOptions>()
				.Configure<EntityFrameworkCoreMapper>((o, m) => {
					o.Mappers.Add(m);
				});

			services.Add(new ServiceDescriptor(
				typeof(EntityFrameworkCoreMapper),
				s => new EntityFrameworkCoreMapper(
					s.GetRequiredService<IModelRetriever<TContext>>().Model,
					typeof(TContext),
					s,
					s.GetService<IOptionsMonitor<EntityFrameworkCoreOptions>>()?.CurrentValue,
					s.GetService<IMatcher>(),
					s.GetService<IOptionsMonitor<MergeCollectionsOptions>>()?.CurrentValue),
				mapper.Lifetime));
			#endregion

			#region IAsyncMapper
			// Add mapper to composite mapper
			services.AddOptions<AsyncCompositeMapperOptions>()
				.Configure<AsyncEntityFrameworkCoreMapper>((o, m) => {
					o.Mappers.Add(m);
				});

			services.Add(new ServiceDescriptor(
				typeof(AsyncEntityFrameworkCoreMapper),
				s => new AsyncEntityFrameworkCoreMapper(
					s.GetRequiredService<IModelRetriever<TContext>>().Model,
					typeof(TContext),
					s,
					s.GetService<IOptionsMonitor<EntityFrameworkCoreOptions>>()?.CurrentValue,
					s.GetService<IMatcher>(),
					s.GetService<IOptionsMonitor<MergeCollectionsOptions>>()?.CurrentValue),
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
				s => new EntityFrameworkCoreProjector(s.GetRequiredService<IModelRetriever<TContext>>().Model, typeof(TContext)),
				projector.Lifetime));
			#endregion

			return services;
		}
	}
}
