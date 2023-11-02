#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NeatMapper.EntityFrameworkCore {
	public static class EntityFrameworkCoreServiceCollectionExtensions {
		public static IServiceCollection AddEntitiesMaps<TContext>(this IServiceCollection services, ServiceLifetime mapperLifetime = ServiceLifetime.Singleton) where TContext : DbContext {
			if (services == null)
				throw new ArgumentNullException(nameof(services));

			if (typeof(TContext).GetConstructor(Type.EmptyTypes) != null) {
				using(var instance = (TContext)Activator.CreateInstance(typeof(TContext))) {
					return services.AddEntitiesMaps<TContext>(instance.Model, mapperLifetime);
				}
			}
			else { 
				using (var serviceProvider = services.BuildServiceProvider()) {
					using(var scope = serviceProvider.CreateScope()) {
						return services.AddEntitiesMaps<TContext>(scope.ServiceProvider.GetRequiredService<TContext>().Model, mapperLifetime);
					}
				}
			}
		}

		public static IServiceCollection AddEntitiesMaps<TContext>(this IServiceCollection services, IModel model, ServiceLifetime mapperLifetime = ServiceLifetime.Singleton) where TContext : DbContext {
			if (services == null)
				throw new ArgumentNullException(nameof(services));
			if (model == null)
				throw new ArgumentNullException(nameof(model));

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
					s),
				mapperLifetime));
			#endregion

			return services;
		}
	}
}
