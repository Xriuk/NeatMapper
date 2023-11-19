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
		/// <param name="asyncMappersLifetime">Lifetime of the <see cref="IAsyncMapper"/> service (and all the specific async mappers registered to create it)</param>
		/// <param name="matchersLifetime">Lifetime of the <see cref="IMatcher"/> service</param>
		/// <returns>The same services collection so multiple calls could be chained</returns>
		public static IServiceCollection AddNeatMapper(this IServiceCollection services,
			ServiceLifetime mappersLifetime = ServiceLifetime.Singleton,
			ServiceLifetime asyncMappersLifetime = ServiceLifetime.Singleton,
			ServiceLifetime matchersLifetime = ServiceLifetime.Singleton) {

			if(services == null)
				throw new ArgumentNullException(nameof(services));

			services.AddOptions();

			#region IMatcher
			// Added to all options
			services.AddTransient<IConfigureOptions<CompositeMatcherOptions>>(
				s => new ConfigureNamedOptions<CompositeMatcherOptions, CustomMatcher, HierarchyCustomMatcher>(
					null,
					s.GetRequiredService<CustomMatcher>(),
					s.GetRequiredService<HierarchyCustomMatcher>(),
					(o, m, h) => {
						o.Matchers.Add(m);
						o.Matchers.Add(h);
					}
				)
			);

			// Register matcher services

			// Normal matcher
			services.Add(new ServiceDescriptor(
				typeof(CustomMatcher),
				s => new CustomMatcher(
					s.GetService<IOptions<CustomMapsOptions>>()?.Value,
					s.GetService<IOptions<CustomMatchAdditionalMapsOptions>>()?.Value,
					s),
				matchersLifetime));

			// Hierarchy matcher
			services.Add(new ServiceDescriptor(
				typeof(HierarchyCustomMatcher),
				s => new HierarchyCustomMatcher(
					s.GetService<IOptions<CustomMapsOptions>>()?.Value,
					s.GetService<IOptions<CustomHierarchyMatchAdditionalMapsOptions>>()?.Value,
					s),
				matchersLifetime));

			// IMatcher (composite matcher)
			services.Add(new ServiceDescriptor(
				typeof(IMatcher),
				s => new CompositeMatcher(s.GetService<IOptions<CompositeMatcherOptions>>()?.Value.Matchers ?? Array.Empty<IMatcher>()),
				matchersLifetime));
			#endregion

			#region IMapper
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

			// Add the projection mapper last to all options
			services.AddTransient<IConfigureOptions<CompositeMapperOptions>>(
				s => new ConfigureNamedOptions<CompositeMapperOptions, ProjectionMapper>(
					null,
					s.GetRequiredService<ProjectionMapper>(),
					(o, p) => {
						o.Mappers.Add(p);
					}
				)
			);


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
			services.Add(new ServiceDescriptor(
				typeof(ProjectionMapper),
				s => new ProjectionMapper(s.GetRequiredService<IProjector>()),
				mappersLifetime));

			// Collection mappers
			services.Add(new ServiceDescriptor(
				typeof(NewCollectionMapper),
				s => new NewCollectionMapper(new CompositeMapper(s.GetService<IOptionsFactory<CompositeMapperOptions>>()?.Create(CompositeMapperOptions.Base).Mappers ?? Array.Empty<IMapper>())),
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
			#endregion

			#region IAsyncMapper
			// Added to all options
			services.AddTransient<IConfigureOptions<AsyncCompositeMapperOptions>>(
				s => new ConfigureNamedOptions<AsyncCompositeMapperOptions, AsyncNewMapper, AsyncMergeMapper>(
					null,
					s.GetRequiredService<AsyncNewMapper>(),
					s.GetRequiredService<AsyncMergeMapper>(),
					(o, n, m) => {
						o.Mappers.Add(n);
						o.Mappers.Add(m);
					}
				)
			);
			// Added only to IAsyncMapper options
			services.AddTransient<IConfigureOptions<AsyncCompositeMapperOptions>, ConfigureCollectionsAsyncCompositeMapperOptions>();

			// Register mapper services

			// Normal mappers
			services.Add(new ServiceDescriptor(
				typeof(AsyncNewMapper),
				s => new AsyncNewMapper(
					s.GetService<IOptions<CustomMapsOptions>>()?.Value,
					s.GetService<IOptions<CustomAsyncNewAdditionalMapsOptions>>()?.Value,
					s),
				asyncMappersLifetime));
			services.Add(new ServiceDescriptor(
				typeof(AsyncMergeMapper),
				s => new AsyncMergeMapper(
					s.GetService<IOptions<CustomMapsOptions>>()?.Value,
					s.GetService<IOptions<CustomAsyncMergeAdditionalMapsOptions>>()?.Value,
					s),
				asyncMappersLifetime));

			// Collection mappers
			services.Add(new ServiceDescriptor(
				typeof(AsyncNewCollectionMapper),
				s => new AsyncNewCollectionMapper(
					new AsyncCompositeMapper(s.GetService<IOptionsFactory<AsyncCompositeMapperOptions>>()?.Create(AsyncCompositeMapperOptions.Base).Mappers ?? Array.Empty<IAsyncMapper>()),
					s.GetService<IOptions<AsyncCollectionMappersOptions>>()?.Value,
					s),
				asyncMappersLifetime));
			services.Add(new ServiceDescriptor(
				typeof(AsyncMergeCollectionMapper),
				s => new AsyncMergeCollectionMapper(
					new AsyncCompositeMapper(s.GetService<IOptionsFactory<AsyncCompositeMapperOptions>>()?.Create(AsyncCompositeMapperOptions.Base).Mappers ?? Array.Empty<IAsyncMapper>()),
					s.GetService<IMatcher>(),
					s.GetService<IOptions<AsyncCollectionMappersOptions>>()?.Value,
					s.GetService<IOptions<MergeCollectionsOptions>>()?.Value,
					s),
				asyncMappersLifetime));

			// IAsyncMapper (composite mapper)
			services.Add(new ServiceDescriptor(
				typeof(IAsyncMapper),
				s => new AsyncCompositeMapper(s.GetService<IOptions<AsyncCompositeMapperOptions>>()?.Value.Mappers ?? Array.Empty<IAsyncMapper>()),
				asyncMappersLifetime));
			#endregion

			#region IProjector

			#endregion

			return services;
		}
	}
}
