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
		/// <item><see cref="IMapper"/></item>
		/// <item><see cref="IAsyncMapper"/></item>
		/// <item><see cref="IMatcher"/></item>
		/// <item><see cref="IProjector"/></item>
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

			// IMatcher (composite matcher)
			services.Add(new ServiceDescriptor(
				typeof(IMatcher),
				s => new CompositeMatcher(s.GetService<IOptionsSnapshot<CompositeMatcherOptions>>()?.Value ?? new CompositeMatcherOptions()),
				matchersLifetime));
			#endregion

			#region IMapper
			// Add mappers to composite mapper
			services.AddOptions<CompositeMapperOptions>()
				.Configure<NewMapper, MergeMapper, ProjectionMapper, IServiceProvider>((o, n, m, p, s) => {
					o.Mappers.Add(n);
					o.Mappers.Add(m);

					// Creating collection mappers with EmptyMapper to avoid recursion, the element mapper will be overridden by composite mapper
					o.Mappers.Add(new NewCollectionMapper(EmptyMapper.Instance));
					o.Mappers.Add(new MergeCollectionMapper(
						EmptyMapper.Instance,
						s.GetService<IMatcher>(),
						s.GetService<IOptionsSnapshot<MergeCollectionsOptions>>()?.Value,
						s));

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
					s.GetService<IOptionsSnapshot<MergeCollectionsOptions>>()?.Value,
					s),
				mappersLifetime));

			// IMapper (composite mapper)
			services.Add(new ServiceDescriptor(
				typeof(IMapper),
				s => new CompositeMapper(s.GetService<IOptionsSnapshot<CompositeMapperOptions>>()?.Value.Mappers ?? Array.Empty<IMapper>()),
				mappersLifetime));
			#endregion

			#region IAsyncMapper
			// Add mappers to composite mapper
			services.AddOptions<AsyncCompositeMapperOptions>()
				.Configure<AsyncNewMapper, AsyncMergeMapper, IServiceProvider>((o, n, m, s) => {
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
						s.GetService<IOptionsSnapshot<MergeCollectionsOptions>>()?.Value,
						s));
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
					s.GetService<IOptionsSnapshot<MergeCollectionsOptions>>()?.Value,
					s),
				asyncMappersLifetime));

			// IAsyncMapper (composite mapper)
			services.Add(new ServiceDescriptor(
				typeof(IAsyncMapper),
				s => new AsyncCompositeMapper(s.GetService<IOptionsSnapshot<AsyncCompositeMapperOptions>>()?.Value.Mappers ?? Array.Empty<IAsyncMapper>()),
				asyncMappersLifetime));
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

			// IProjector (composite projector)
			services.Add(new ServiceDescriptor(
				typeof(IProjector),
				s => new CompositeProjector(s.GetService<IOptionsSnapshot<CompositeProjectorOptions>>()?.Value.Projectors ?? Array.Empty<IProjector>()),
				projectorsLifetime));
			#endregion

			return services;
		}
	}
}
