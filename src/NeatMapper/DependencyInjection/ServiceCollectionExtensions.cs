#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace NeatMapper {
	public static class ServiceCollectionExtensions {
		/// <summary>
		/// Adds NeatMapper services to the services collection:
		/// <list type="bullet">
		/// <item>
		/// <see cref="IMapper"/>, <see cref="IMapperCanMap"/>, <see cref="IMapperFactory"/> -
		/// <see cref="NewMapper"/>, <see cref="MergeMapper"/>, <see cref="ProjectionMapper"/>,
		/// <see cref="NewCollectionMapper"/>, <see cref="MergeCollectionMapper"/>, <see cref="CompositeMapper"/>
		/// (which also contains <see cref="IdentityMapper"/> in addition to all the previous)
		/// </item>
		/// <item>
		/// <see cref="IAsyncMapper"/>, <see cref="IAsyncMapperCanMap"/>, <see cref="IAsyncMapperFactory"/> -
		/// <see cref="AsyncNewMapper"/>, <see cref="AsyncMergeMapper"/>, <see cref="AsyncNewCollectionMapper"/>,
		/// <see cref="AsyncMergeCollectionMapper"/>, <see cref="AsyncCompositeMapper"/>
		/// (which also contains <see cref="AsyncIdentityMapper"/> in addition to all the previous)
		/// </item>
		/// <item>
		/// <see cref="IMatcher"/>, <see cref="IMatcherCanMatch"/>, <see cref="IMatcherFactory"/> -
		/// <see cref="CustomMatcher"/>, <see cref="HierarchyCustomMatcher"/>, <see cref="CompositeMatcher"/>
		/// </item>
		/// <item>
		/// <see cref="IProjector"/>, <see cref="IProjectorCanProject"/> -
		/// <see cref="CustomProjector"/>, <see cref="CollectionProjector"/>, <see cref="CompositeProjector"/>
		/// </item>
		/// </list>
		/// </summary>
		/// <param name="mappersLifetime">
		/// Lifetime of the <see cref="IMapper"/> service (and all the specific mappers registered
		/// to create it).
		/// </param>
		/// <param name="asyncMappersLifetime">
		/// Lifetime of the <see cref="IAsyncMapper"/> service (and all the specific async mappers registered
		/// to create it).
		/// </param>
		/// <param name="matchersLifetime">
		/// Lifetime of the <see cref="IMatcher"/> service (and all the specific matchers registered
		/// to create it).
		/// </param>
		/// <param name="projectorsLifetime">
		/// Lifetime of the <see cref="IProjector"/> service (and all the specific projectors registered
		/// to create it).
		/// </param>
		/// <returns>The same services collection so multiple calls could be chained.</returns>
		public static IServiceCollection AddNeatMapper(this IServiceCollection services,
			ServiceLifetime mappersLifetime = ServiceLifetime.Singleton,
			ServiceLifetime asyncMappersLifetime = ServiceLifetime.Singleton,
			ServiceLifetime matchersLifetime = ServiceLifetime.Singleton,
			ServiceLifetime projectorsLifetime = ServiceLifetime.Singleton) {

			if(services == null)
				throw new ArgumentNullException(nameof(services));

			services.AddOptions();

			#region IMatcher
			// Add matchers to composite matcher
			services.AddOptions<CompositeMatcherOptions>()
				.Configure<CustomMatcher, HierarchyCustomMatcher>((o, m, h) => {
					o.Matchers.Add(m);
					o.Matchers.Add(h);
				});


			// Register matcher services

			// Normal matcher
			services.Add(new ServiceDescriptor(
				typeof(CustomMatcher),
				s => new CustomMatcher(
					s.GetService<IOptionsSnapshot<CustomMapsOptions>>()?.Value,
					s.GetService<IOptionsSnapshot<CustomMatchAdditionalMapsOptions>>()?.Value,
					s),
				matchersLifetime));

			// Hierarchy matcher
			services.Add(new ServiceDescriptor(
				typeof(HierarchyCustomMatcher),
				s => new HierarchyCustomMatcher(
					s.GetService<IOptionsSnapshot<CustomMapsOptions>>()?.Value,
					s.GetService<IOptionsSnapshot<CustomHierarchyMatchAdditionalMapsOptions>>()?.Value,
					s),
				matchersLifetime));

			// Composite matcher
			services.Add(new ServiceDescriptor(
				typeof(CompositeMatcher),
				s => new CompositeMatcher(s.GetService<IOptionsSnapshot<CompositeMatcherOptions>>()?.Value ?? new CompositeMatcherOptions()),
				matchersLifetime));

			// IMatcher, IMatcherCanMatch, IMatcherFactory
			services.Add(new ServiceDescriptor(typeof(IMatcher),			s => s.GetRequiredService<CompositeMatcher>(),	matchersLifetime));
			services.Add(new ServiceDescriptor(typeof(IMatcherCanMatch),	s => s.GetRequiredService<CompositeMatcher>(),	matchersLifetime));
			services.Add(new ServiceDescriptor(typeof(IMatcherFactory), s => s.GetRequiredService<CompositeMatcher>(), matchersLifetime));
			#endregion

			#region IMapper
			// Add mappers to composite mapper
			services.AddOptions<CompositeMapperOptions>()
				.Configure<NewMapper, MergeMapper, ProjectionMapper, IServiceProvider>((o, n, m, p, s) => {
					o.Mappers.Add(IdentityMapper.Instance);

					o.Mappers.Add(n);
					o.Mappers.Add(m);

					// Creating collection mappers with EmptyMapper to avoid recursion, the element mapper will be overridden by composite mapper
					o.Mappers.Add(new NewCollectionMapper(EmptyMapper.Instance));
					o.Mappers.Add(new MergeCollectionMapper(
						EmptyMapper.Instance,
						s.GetService<IMatcher>(),
						s.GetService<IOptionsSnapshot<MergeCollectionsOptions>>()?.Value));

					o.Mappers.Add(p);
				});


			// Register mapper services

			// Normal mappers
			services.Add(new ServiceDescriptor(
				typeof(NewMapper),
				s => new NewMapper(
					s.GetService<IOptionsSnapshot<CustomMapsOptions>>()?.Value,
					s.GetService<IOptionsSnapshot<CustomNewAdditionalMapsOptions>>()?.Value,
					s),
				mappersLifetime));
			services.Add(new ServiceDescriptor(
				typeof(MergeMapper),
				s => new MergeMapper(
					s.GetService<IOptionsSnapshot<CustomMapsOptions>>()?.Value,
					s.GetService<IOptionsSnapshot<CustomMergeAdditionalMapsOptions>>()?.Value,
					s),
				mappersLifetime));
			services.Add(new ServiceDescriptor(
				typeof(ProjectionMapper),
				s => new ProjectionMapper(s.GetRequiredService<IProjector>()),
				mappersLifetime));

			// Collection mappers
			services.Add(new ServiceDescriptor(
				typeof(NewCollectionMapper),
				s => new NewCollectionMapper(s.GetRequiredService<IMapper>()),
				mappersLifetime));
			services.Add(new ServiceDescriptor(
				typeof(MergeCollectionMapper),
				s => new MergeCollectionMapper(
					s.GetRequiredService<IMapper>(),
					s.GetService<IMatcher>(),
					s.GetService<IOptionsSnapshot<MergeCollectionsOptions>>()?.Value),
				mappersLifetime));

			// Composite mapper
			services.Add(new ServiceDescriptor(
				typeof(CompositeMapper),
				s => new CompositeMapper(s.GetService<IOptionsSnapshot<CompositeMapperOptions>>()?.Value.Mappers ?? Array.Empty<IMapper>()),
				mappersLifetime));

			// IMapper, IMapperCanMap, IMapperFactory
			services.Add(new ServiceDescriptor(typeof(IMapper),			s => s.GetRequiredService<CompositeMapper>(),	mappersLifetime));
			services.Add(new ServiceDescriptor(typeof(IMapperCanMap),	s => s.GetRequiredService<CompositeMapper>(),	mappersLifetime));
			services.Add(new ServiceDescriptor(typeof(IMapperFactory),	s => s.GetRequiredService<CompositeMapper>(),	mappersLifetime));
			#endregion

			#region IAsyncMapper
			// Add mappers to composite mapper
			services.AddOptions<AsyncCompositeMapperOptions>()
				.Configure<AsyncNewMapper, AsyncMergeMapper, IServiceProvider>((o, n, m, s) => {
					o.Mappers.Add(AsyncIdentityMapper.Instance);

					o.Mappers.Add(n);
					o.Mappers.Add(m);

					// Creating collection mappers with AsyncEmptyMapper to avoid recursion, the element mapper will be overridden by composite mapper
					var asyncOptions = s.GetService<IOptionsSnapshot<AsyncCollectionMappersOptions>>()?.Value;
					o.Mappers.Add(new AsyncNewCollectionMapper(
						AsyncEmptyMapper.Instance,
						asyncOptions));
					o.Mappers.Add(new AsyncMergeCollectionMapper(
						AsyncEmptyMapper.Instance,
						s.GetService<IMatcher>(),
						asyncOptions,
						s.GetService<IOptionsSnapshot<MergeCollectionsOptions>>()?.Value));
				});


			// Register mapper services

			// Normal mappers
			services.Add(new ServiceDescriptor(
				typeof(AsyncNewMapper),
				s => new AsyncNewMapper(
					s.GetService<IOptionsSnapshot<CustomMapsOptions>>()?.Value,
					s.GetService<IOptionsSnapshot<CustomAsyncNewAdditionalMapsOptions>>()?.Value,
					s),
				asyncMappersLifetime));
			services.Add(new ServiceDescriptor(
				typeof(AsyncMergeMapper),
				s => new AsyncMergeMapper(
					s.GetService<IOptionsSnapshot<CustomMapsOptions>>()?.Value,
					s.GetService<IOptionsSnapshot<CustomAsyncMergeAdditionalMapsOptions>>()?.Value,
					s),
				asyncMappersLifetime));

			// Collection mappers
			services.Add(new ServiceDescriptor(
				typeof(AsyncNewCollectionMapper),
				s => new AsyncNewCollectionMapper(
					s.GetRequiredService<IAsyncMapper>(),
					s.GetService<IOptionsSnapshot<AsyncCollectionMappersOptions>>()?.Value),
				asyncMappersLifetime));
			services.Add(new ServiceDescriptor(
				typeof(AsyncMergeCollectionMapper),
				s => new AsyncMergeCollectionMapper(
					s.GetRequiredService<IAsyncMapper>(),
					s.GetService<IMatcher>(),
					s.GetService<IOptionsSnapshot<AsyncCollectionMappersOptions>>()?.Value,
					s.GetService<IOptionsSnapshot<MergeCollectionsOptions>>()?.Value),
				asyncMappersLifetime));

			// Composite mapper
			services.Add(new ServiceDescriptor(
				typeof(AsyncCompositeMapper),
				s => new AsyncCompositeMapper(s.GetService<IOptionsSnapshot<AsyncCompositeMapperOptions>>()?.Value.Mappers ?? Array.Empty<IAsyncMapper>()),
				asyncMappersLifetime));

			// IAsyncMapper, IAsyncMapperCanMap, IAsyncMapperFactory
			services.Add(new ServiceDescriptor(typeof(IAsyncMapper),		s => s.GetRequiredService<AsyncCompositeMapper>(),	asyncMappersLifetime));
			services.Add(new ServiceDescriptor(typeof(IAsyncMapperCanMap),	s => s.GetRequiredService<AsyncCompositeMapper>(),	asyncMappersLifetime));
			services.Add(new ServiceDescriptor(typeof(IAsyncMapperFactory),	s => s.GetRequiredService<AsyncCompositeMapper>(),	asyncMappersLifetime));
			#endregion

			#region IProjector
			// Add projectors to composite projector
			services.AddOptions<CompositeProjectorOptions>()
				.Configure<CustomProjector>((o, p) => {
					o.Projectors.Add(p);

					// Creating collection mappers with EmptyProjector to avoid recursion, the element projector will be overridden by composite projector
					o.Projectors.Add(new CollectionProjector(EmptyProjector.Instance));
				});


			// Register projector services

			// Normal projectors
			services.Add(new ServiceDescriptor(
				typeof(CustomProjector),
				s => new CustomProjector(
					s.GetService<IOptionsSnapshot<CustomMapsOptions>>()?.Value,
					s.GetService<IOptionsSnapshot<CustomProjectionAdditionalMapsOptions>>()?.Value,
					s),
				projectorsLifetime));

			// Collection projectors
			services.Add(new ServiceDescriptor(
				typeof(CollectionProjector),
				s => new CollectionProjector(s.GetRequiredService<IProjector>()),
				projectorsLifetime));

			// Composite projector
			services.Add(new ServiceDescriptor(
				typeof(CompositeProjector),
				s => new CompositeProjector(s.GetService<IOptionsSnapshot<CompositeProjectorOptions>>()?.Value.Projectors ?? Array.Empty<IProjector>()),
				projectorsLifetime));

			// IProjector, IProjectorCanProject
			services.Add(new ServiceDescriptor(typeof(IProjector),				s => s.GetRequiredService<CompositeProjector>(),	projectorsLifetime));
			services.Add(new ServiceDescriptor(typeof(IProjectorCanProject),	s => s.GetRequiredService<CompositeProjector>(),	projectorsLifetime));
			#endregion

			return services;
		}
	}
}
