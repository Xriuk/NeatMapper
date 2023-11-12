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
		/// <inheritdoc cref="AddNeatMapperEntityFrameworkCore{TContext}(IServiceCollection, IModel, ServiceLifetime, ServiceLifetime, ServiceLifetime)"/>
		public static IServiceCollection AddNeatMapperEntityFrameworkCore<TContext>(this IServiceCollection services,
			ServiceLifetime mapperLifetime = ServiceLifetime.Singleton,
			ServiceLifetime asyncMapperLifetime = ServiceLifetime.Singleton,
			ServiceLifetime matcherLifetime = ServiceLifetime.Singleton) where TContext : DbContext {

			if (services == null)
				throw new ArgumentNullException(nameof(services));

			if (typeof(TContext).GetConstructor(Type.EmptyTypes) != null) {
				using(var instance = (TContext)Activator.CreateInstance(typeof(TContext))) {
					return services.AddNeatMapperEntityFrameworkCore<TContext>(instance.Model, mapperLifetime, asyncMapperLifetime, matcherLifetime);
				}
			}
			else { 
				using (var serviceProvider = services.BuildServiceProvider()) {
					using(var scope = serviceProvider.CreateScope()) {
						return services.AddNeatMapperEntityFrameworkCore<TContext>(scope.ServiceProvider.GetRequiredService<TContext>().Model, mapperLifetime, asyncMapperLifetime, matcherLifetime);
					}
				}
			}
		}

		/// <summary>
		/// Adds <see cref="IMatcher"/> and <see cref="IMapper"/> to the services collection.<br/>
		/// To configure them you can use <see cref="OptionsServiceCollectionExtensions.ConfigureAll{TOptions}(IServiceCollection, Action{TOptions})"/>
		/// to configure <see cref="CompositeMapperOptions"/>, and <see cref="OptionsServiceCollectionExtensions.Configure{TOptions}(IServiceCollection, Action{TOptions})"/>
		/// to configure all the other options
		/// </summary>
		/// <typeparam name="TContext">Type of the DbContext to use with the mapper</typeparam>
		/// <param name="mapperLifetime">Lifetime of the <see cref="EntityFrameworkCoreMapper"/> service</param>
		/// <param name="asyncMapperLifetime">Lifetime of the <see cref="AsyncEntityFrameworkCoreMapper"/> service</param>
		/// <param name="matcherLifetime">Lifetime of the <see cref="EntityFrameworkCoreMatcher"/> service</param>
		/// <returns>The same services collection so multiple calls could be chained</returns>
		public static IServiceCollection AddNeatMapperEntityFrameworkCore<TContext>(this IServiceCollection services,
			IModel model,
			ServiceLifetime mapperLifetime = ServiceLifetime.Singleton,
			ServiceLifetime asyncMapperLifetime = ServiceLifetime.Singleton,
			ServiceLifetime matcherLifetime = ServiceLifetime.Singleton) where TContext : DbContext {

			if (services == null)
				throw new ArgumentNullException(nameof(services));
			if (model == null)
				throw new ArgumentNullException(nameof(model));

			#region IMatcher
			// Added to all options
			services.AddTransient<IConfigureOptions<CompositeMatcherOptions>>(
				s => new ConfigureNamedOptions<CompositeMatcherOptions, EntityFrameworkCoreMatcher>(
					null,
					s.GetRequiredService<EntityFrameworkCoreMatcher>(),
					(o, m) => o.Matchers.Add(m)
				)
			);

			services.Add(new ServiceDescriptor(
				typeof(EntityFrameworkCoreMatcher),
				s => new EntityFrameworkCoreMatcher(model),
				matcherLifetime));
			#endregion

			#region IMapper
			// Added to all options
			services.AddTransient<IConfigureOptions<CompositeMapperOptions>>(
				s => new ConfigureNamedOptions<CompositeMapperOptions, EntityFrameworkCoreMapper>(
					null,
					s.GetRequiredService<EntityFrameworkCoreMapper>(),
					(o, e) => {
						// Try adding before collection mappers
						var collectionMapper = o.Mappers.OfType<CollectionMapper>().FirstOrDefault();
						if(collectionMapper != null)
							o.Mappers.Insert(o.Mappers.IndexOf(collectionMapper), e);
						else
							o.Mappers.Add(e);
					}
				)
			);

			services.Add(new ServiceDescriptor(
				typeof(EntityFrameworkCoreMapper),
				s => new EntityFrameworkCoreMapper(
					model,
					typeof(TContext),
					s,
					s.GetService<IOptions<EntityFrameworkCoreOptions>>()?.Value,
					s.GetService<IMatcher>(),
					s.GetService<IOptions<MergeCollectionsOptions>>()?.Value),
				mapperLifetime));
			#endregion

			#region IAsyncMapper
			// Added to all options
			services.AddTransient<IConfigureOptions<AsyncCompositeMapperOptions>>(
				s => new ConfigureNamedOptions<AsyncCompositeMapperOptions, AsyncEntityFrameworkCoreMapper>(
					null,
					s.GetRequiredService<AsyncEntityFrameworkCoreMapper>(),
					(o, e) => {
						// Try adding before collection mappers
						var collectionMapper = o.Mappers.OfType<AsyncCollectionMapper>().FirstOrDefault();
						if (collectionMapper != null)
							o.Mappers.Insert(o.Mappers.IndexOf(collectionMapper), e);
						else
							o.Mappers.Add(e);
					}
				)
			);

			services.Add(new ServiceDescriptor(
				typeof(AsyncEntityFrameworkCoreMapper),
				s => new AsyncEntityFrameworkCoreMapper(
					model,
					typeof(TContext),
					s,
					s.GetService<IOptions<EntityFrameworkCoreOptions>>()?.Value,
					s.GetService<IMatcher>(),
					s.GetService<IOptions<MergeCollectionsOptions>>()?.Value),
				asyncMapperLifetime));
			#endregion

			return services;
		}
	}
}
