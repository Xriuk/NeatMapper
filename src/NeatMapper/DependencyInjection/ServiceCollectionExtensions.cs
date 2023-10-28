#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NeatMapper.DependencyInjection.Internal;
using System;

namespace NeatMapper {
	public static class ServiceCollectionExtensions {
		/// <summary>
		/// Adds <see cref="IMatcher"/> and <see cref="IMapper"/> to the services collection.<br/>
		/// To configure them you can use <see cref="OptionsServiceCollectionExtensions.ConfigureAll{TOptions}(IServiceCollection, Action{TOptions})"/>
		/// to configure <see cref="CompositeMapperOptions"/>, and <see cref="OptionsServiceCollectionExtensions.Configure{TOptions}(IServiceCollection, Action{TOptions})"/>
		/// to configure all the other options
		/// </summary>
		/// <param name="mappersLifetime">Lifetime of the <see cref="IMapper"/> service (and all the specific mappers registered to create it)</param>
		/// <param name="matchersLifetime">Lifetime of the <see cref="IMatcher"/> service</param>
		/// <returns>The same services collection so multiple calls could be chained</returns>
		public static IServiceCollection AddNeatMapper(this IServiceCollection services, ServiceLifetime mappersLifetime = ServiceLifetime.Singleton, ServiceLifetime matchersLifetime = ServiceLifetime.Singleton) {
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			// Configure composite mappers
			services.AddOptions();
			services.TryAddMatchers(matchersLifetime);

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
			services.AddTransient<IConfigureOptions<CompositeMapperOptions>, ConfigureCollectionsCompositeMapperOptions>();
			
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
					new CompositeMapper(s.GetService<IOptionsFactory<CompositeMapperOptions>>()?.Create(CompositeMapperOptions.Base).Mappers ?? Array.Empty<IMapper>()),
					s),
				mappersLifetime));
			services.Add(new ServiceDescriptor(
				typeof(MergeCollectionMapper),
				s => new MergeCollectionMapper(
					new CompositeMapper(s.GetService<IOptionsFactory<CompositeMapperOptions>>()?.Create(CompositeMapperOptions.Base).Mappers ?? Array.Empty<IMapper>()),
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

		private static IServiceCollection TryAddMatchers(this IServiceCollection services, ServiceLifetime matchersLifetime = ServiceLifetime.Scoped) {
			if (services == null)
				throw new ArgumentNullException(nameof(services));

			// Configure composite matchers
			services.AddOptions();

			// Added to all options
			services.TryAddTransient<IConfigureOptions<CompositeMatcherOptions>>(
				s => new ConfigureNamedOptions<CompositeMatcherOptions, Matcher>(
					null,
					s.GetRequiredService<Matcher>(),
					(o, m) => o.Matchers.Add(m)
				)
			);

			// Register matcher services

			// Normal matcher
			services.TryAdd(new ServiceDescriptor(
				typeof(Matcher),
				s => new Matcher(
					s.GetService<IOptions<CustomMapsOptions>>()?.Value,
					s.GetService<IOptions<CustomMatchAdditionalMapsOptions>>()?.Value,
					s),
				matchersLifetime));

			// IMapper (composite mapper)
			services.TryAdd(new ServiceDescriptor(
				typeof(IMatcher),
				s => new CompositeMatcher(s.GetService<IOptions<CompositeMatcherOptions>>()?.Value.Matchers ?? Array.Empty<IMatcher>()),
				matchersLifetime));

			return services;
		}
	}
}
