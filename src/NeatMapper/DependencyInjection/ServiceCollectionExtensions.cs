using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace NeatMapper {
	public static class ServiceCollectionExtensions {
		/// <summary>
		/// Adds NeatMapper services to the services collection:
		/// <para>
		/// Matchers:
		/// <list type="bullet">
		/// <item><see cref="IMatcher"/> and <see cref="IMatcherFactory"/></item>
		/// <item>
		/// <see cref="CustomMatcher"/>, <see cref="HierarchyCustomMatcher"/> and <see cref="CompositeMatcher"/>
		/// which also contains, in addition to all the previous:
		/// <list type="bullet">
		/// <item><see cref="EquatableMatcher"/></item>
		/// <item>EqualityOperatorsMatcher for (.NET 7+)</item>
		/// <item><see cref="ObjectEqualsMatcher"/></item>
		/// </list>
		/// </item>
		/// </list>
		/// </para>
		/// <para>
		/// Mappers:
		/// <list type="bullet">
		/// <item><see cref="IMapper"/> and <see cref="IMapperFactory"/></item>
		/// <item>
		/// <see cref="CustomMapper"/>, <see cref="ProjectionMapper"/>, <see cref="CollectionMapper"/>
		/// and <see cref="CompositeMapper"/> which also contains, in addition to all the previous:
		/// <list type="bullet">
		/// <item><see cref="TypeConverterMapper"/></item>
		/// <item><see cref="ConvertibleMapper"/></item>
		/// </list>
		/// </item>
		/// </list>
		/// </para>
		/// <para>
		/// Async mappers:
		/// <list type="bullet">
		/// <item><see cref="IAsyncMapper"/> and <see cref="IAsyncMapperFactory"/></item>
		/// <item>
		/// <see cref="AsyncCustomMapper"/>, <see cref="AsyncCollectionMapper"/> and <see cref="AsyncCompositeMapper"/>
		/// which also contains all the previous.
		/// </item>
		/// </list>
		/// </para>
		/// <para>
		/// Projectors:
		/// <list type="bullet">
		/// <item><see cref="IProjector"/></item>
		/// <item>
		/// <see cref="CustomProjector"/>, <see cref="CollectionProjector"/> and <see cref="CompositeProjector"/>
		/// which also contains all the previous.
		/// </item>
		/// </list>
		/// </para>
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
				})
				.PostConfigure(o => {
					o.Matchers.Add(EquatableMatcher.Instance);
#if NET7_0_OR_GREATER
					o.Matchers.Add(EqualityOperatorsMatcher.Instance);
#endif
					o.Matchers.Add(ObjectEqualsMatcher.Instance);
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
			services.Add(new ServiceDescriptor(typeof(IMatcherFactory),		s => s.GetRequiredService<CompositeMatcher>(),	matchersLifetime));
			#endregion

			#region IMapper
			// Add mappers to composite mapper
			services.AddOptions<CompositeMapperOptions>()
				.Configure<CustomMapper, ProjectionMapper, IServiceProvider>((o, c, p, s) => {
					o.Mappers.Add(c);
					o.Mappers.Add(p);
				})
				.PostConfigure<IServiceProvider>((o, s) => {
					o.Mappers.Add(TypeConverterMapper.Instance);
					o.Mappers.Add(ConvertibleMapper.Instance);

					// Creating collection mapper with EmptyMapper to avoid recursion, the element mapper will be overridden by composite mapper
					o.Mappers.Add(new CollectionMapper(
						EmptyMapper.Instance,
						s.GetService<IMatcher>(),
						s.GetService<IOptionsSnapshot<MergeCollectionsOptions>>()?.Value));
				});


			// Register mapper services

			// Normal mappers
			services.Add(new ServiceDescriptor(
				typeof(CustomMapper),
				s => new CustomMapper(
					s.GetService<IOptionsSnapshot<CustomMapsOptions>>()?.Value,
					s.GetService<IOptionsSnapshot<CustomNewAdditionalMapsOptions>>()?.Value,
					s.GetService<IOptionsSnapshot<CustomMergeAdditionalMapsOptions>>()?.Value,
					s),
				mappersLifetime));
			services.Add(new ServiceDescriptor(
				typeof(ProjectionMapper),
				s => new ProjectionMapper(s.GetRequiredService<IProjector>()),
				mappersLifetime));

			// Collection mappers
			services.Add(new ServiceDescriptor(
				typeof(CollectionMapper),
				s => new CollectionMapper(
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
			services.Add(new ServiceDescriptor(typeof(IMapperFactory),	s => s.GetRequiredService<CompositeMapper>(),	mappersLifetime));
			#endregion

			#region IAsyncMapper
			// Add mappers to composite mapper
			services.AddOptions<AsyncCompositeMapperOptions>()
				.Configure<AsyncCustomMapper, IServiceProvider>((o, c, s) => {
					o.Mappers.Add(c);
				})
				.PostConfigure<IServiceProvider>((o, s) => {
					// Creating collection mapper with AsyncEmptyMapper to avoid recursion, the element mapper will be overridden by composite mapper
					o.Mappers.Add(new AsyncCollectionMapper(
						AsyncEmptyMapper.Instance,
						s.GetService<IOptionsSnapshot<AsyncCollectionMappersOptions>>()?.Value,
						s.GetService<IMatcher>(),
						s.GetService<IOptionsSnapshot<MergeCollectionsOptions>>()?.Value));
				});


			// Register mapper services

			// Normal mappers
			services.Add(new ServiceDescriptor(
				typeof(AsyncCustomMapper),
				s => new AsyncCustomMapper(
					s.GetService<IOptionsSnapshot<CustomMapsOptions>>()?.Value,
					s.GetService<IOptionsSnapshot<CustomAsyncNewAdditionalMapsOptions>>()?.Value,
					s.GetService<IOptionsSnapshot<CustomAsyncMergeAdditionalMapsOptions>>()?.Value,
					s),
				asyncMappersLifetime));

			// Collection mappers
			services.Add(new ServiceDescriptor(
				typeof(AsyncCollectionMapper),
				s => new AsyncCollectionMapper(
					s.GetRequiredService<IAsyncMapper>(),
					s.GetService<IOptionsSnapshot<AsyncCollectionMappersOptions>>()?.Value,
					s.GetService<IMatcher>(),
					s.GetService<IOptionsSnapshot<MergeCollectionsOptions>>()?.Value),
				asyncMappersLifetime));

			// Composite mapper
			services.Add(new ServiceDescriptor(
				typeof(AsyncCompositeMapper),
				s => new AsyncCompositeMapper(s.GetService<IOptionsSnapshot<AsyncCompositeMapperOptions>>()?.Value.Mappers ?? Array.Empty<IAsyncMapper>()),
				asyncMappersLifetime));

			// IAsyncMapper, IAsyncMapperCanMap, IAsyncMapperFactory
			services.Add(new ServiceDescriptor(typeof(IAsyncMapper),		s => s.GetRequiredService<AsyncCompositeMapper>(),	asyncMappersLifetime));
			services.Add(new ServiceDescriptor(typeof(IAsyncMapperFactory),	s => s.GetRequiredService<AsyncCompositeMapper>(),	asyncMappersLifetime));
			#endregion

			#region IProjector
			// Add projectors to composite projector
			services.AddOptions<CompositeProjectorOptions>()
				.Configure<CustomProjector>((o, p) => {
					o.Projectors.Add(p);
				})
				.PostConfigure(o => {
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
			#endregion

			return services;
		}
	}
}
