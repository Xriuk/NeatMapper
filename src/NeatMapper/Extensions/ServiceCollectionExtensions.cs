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
		/// <see cref="CustomMatcher"/>, <see cref="HierarchyCustomMatcher"/>, <see cref="NullableMatcher"/>,
		/// <see cref="CollectionMatcher"/> and <see cref="CompositeMatcher"/> which also contains,
		/// in addition to all the previous:
		/// <list type="bullet">
		/// <item><see cref="EquatableMatcher"/></item>
		/// <item>EqualityOperatorsMatcher for (.NET 7+)</item>
		/// <item><see cref="StructuralEquatableMatcher"/></item>
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
		/// <see cref="CustomMapper"/>, <see cref="ProjectionMapper"/>, <see cref="EnumMapper"/>,
		/// <see cref="CopyMapper"/>, <see cref="NullableMapper"/>, <see cref="CollectionMapper"/>
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
		/// <see cref="AsyncCustomMapper"/>, <see cref="AsyncNullableMapper"/>,
		/// <see cref="AsyncCollectionMapper"/> and <see cref="AsyncCompositeMapper"/>
		/// which also contains all the previous.
		/// </item>
		/// </list>
		/// </para>
		/// <para>
		/// Projectors:
		/// <list type="bullet">
		/// <item><see cref="IProjector"/></item>
		/// <item>
		/// <see cref="CustomProjector"/>, <see cref="NullableProjector"/>,
		/// <see cref="CollectionProjector"/> and <see cref="CompositeProjector"/>
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
				.PostConfigure<IServiceProvider>((o, s) => {
					o.Matchers.Add(EquatableMatcher.Instance);
#if NET7_0_OR_GREATER
					o.Matchers.Add(EqualityOperatorsMatcher.Instance);
#endif

					// Creating nullable and collection matchers with EmptyMatcher to avoid recursion,
					// the element matcher will be overridden by composite matcher
					o.Matchers.Add(new NullableMatcher(EmptyMatcher.Instance));
					o.Matchers.Add(new CollectionMatcher(
						EmptyMatcher.Instance,
						s.GetService<IOptionsMonitor<CollectionMatchersOptions>>()?.CurrentValue));

					// Last because they are fallbacks
					o.Matchers.Add(StructuralEquatableMatcher.Instance);
					o.Matchers.Add(ObjectEqualsMatcher.Instance);
				});


			// Register matcher services

			// Normal matcher
			services.Add(new ServiceDescriptor(
				typeof(CustomMatcher),
				s => new CustomMatcher(
					s.GetService<IOptionsMonitor<CustomMapsOptions>>()?.CurrentValue,
					s.GetService<IOptionsMonitor<CustomMatchAdditionalMapsOptions>>()?.CurrentValue,
					s),
				matchersLifetime));

			// Hierarchy matcher
			services.Add(new ServiceDescriptor(
				typeof(HierarchyCustomMatcher),
				s => new HierarchyCustomMatcher(
					s.GetService<IOptionsMonitor<CustomMapsOptions>>()?.CurrentValue,
					s.GetService<IOptionsMonitor<CustomHierarchyMatchAdditionalMapsOptions>>()?.CurrentValue,
					s),
				matchersLifetime));

			// Collection matcher
			services.Add(new ServiceDescriptor(
				typeof(CollectionMatcher),
				s => new CollectionMatcher(
					s.GetRequiredService<IMatcher>(),
					s.GetService<IOptionsMonitor<CollectionMatchersOptions>>()?.CurrentValue),
				matchersLifetime));

			// Composite matcher
			services.Add(new ServiceDescriptor(
				typeof(CompositeMatcher),
				s => new CompositeMatcher(s.GetService<IOptionsMonitor<CompositeMatcherOptions>>()?.CurrentValue ?? new CompositeMatcherOptions()),
				matchersLifetime));

			// IMatcher, IMatcherFactory
			services.Add(new ServiceDescriptor(typeof(IMatcher),			s => s.GetRequiredService<CompositeMatcher>(),	matchersLifetime));
			services.Add(new ServiceDescriptor(typeof(IMatcherFactory),		s => s.GetRequiredService<CompositeMatcher>(),	matchersLifetime));
			#endregion

			#region IMapper
			// Add mappers to composite mapper
			services.AddOptions<CompositeMapperOptions>()
				.Configure<CustomMapper, ProjectionMapper, EnumMapper>((o, c, p, e) => {
					o.Mappers.Add(c);
					o.Mappers.Add(p);
					o.Mappers.Add(e);
				})
				.PostConfigure<IServiceProvider>((o, s) => {
					o.Mappers.Add(TypeConverterMapper.Instance);
					o.Mappers.Add(ConvertibleMapper.Instance);

					// Creating copy, nullable and collection mappers with EmptyMapper to avoid recursion,
					// the element mapper will be overridden by composite mapper
					o.Mappers.Add(new CopyMapper(EmptyMapper.Instance, s.GetService<IOptionsMonitor<CopyMapperOptions>>()?.CurrentValue));
					o.Mappers.Add(new NullableMapper(EmptyMapper.Instance));
					o.Mappers.Add(new CollectionMapper(
						EmptyMapper.Instance,
						s.GetService<IMatcher>(),
						s.GetService<IOptionsMonitor<MergeCollectionsOptions>>()?.CurrentValue));
				});


			// Register mapper services

			// Normal mapper
			services.Add(new ServiceDescriptor(
				typeof(CustomMapper),
				s => new CustomMapper(
					s.GetService<IOptionsMonitor<CustomMapsOptions>>()?.CurrentValue,
					s.GetService<IOptionsMonitor<CustomNewAdditionalMapsOptions>>()?.CurrentValue,
					s.GetService<IOptionsMonitor<CustomMergeAdditionalMapsOptions>>()?.CurrentValue,
					s),
				mappersLifetime));

			// Projection mapper
			services.Add(new ServiceDescriptor(
				typeof(ProjectionMapper),
				s => new ProjectionMapper(s.GetRequiredService<IProjector>()),
				mappersLifetime));

			// Enum mapper
			services.Add(new ServiceDescriptor(
				typeof(EnumMapper),
				s => new EnumMapper(s.GetService<IOptionsMonitor<EnumMapperOptions>>()?.CurrentValue),
				mappersLifetime));

			// Copy mapper
			services.Add(new ServiceDescriptor(
				typeof(CopyMapper),
				s => new CopyMapper(s.GetRequiredService<IMapper>(), s.GetService<IOptionsMonitor<CopyMapperOptions>>()?.CurrentValue),
				mappersLifetime));

			// Nullable mapper
			services.Add(new ServiceDescriptor(
				typeof(NullableMapper),
				s => new NullableMapper(s.GetRequiredService<IMapper>()),
				mappersLifetime));

			// Collection mapper
			services.Add(new ServiceDescriptor(
				typeof(CollectionMapper),
				s => new CollectionMapper(
					s.GetRequiredService<IMapper>(),
					s.GetService<IMatcher>(),
					s.GetService<IOptionsMonitor<MergeCollectionsOptions>>()?.CurrentValue),
				mappersLifetime));

			// Composite mapper
			services.Add(new ServiceDescriptor(
				typeof(CompositeMapper),
				s => new CompositeMapper(s.GetService<IOptionsMonitor<CompositeMapperOptions>>()?.CurrentValue.Mappers ?? []),
				mappersLifetime));

			// IMapper, IMapperFactory
			services.Add(new ServiceDescriptor(typeof(IMapper),			s => s.GetRequiredService<CompositeMapper>(),	mappersLifetime));
			services.Add(new ServiceDescriptor(typeof(IMapperFactory),	s => s.GetRequiredService<CompositeMapper>(),	mappersLifetime));
			#endregion

			#region IAsyncMapper
			// Add mappers to composite mapper
			services.AddOptions<AsyncCompositeMapperOptions>()
				.Configure<AsyncCustomMapper, IServiceProvider>((o, c, s) => {
					o.Mappers.Add(c);
				})
				.PostConfigure<IMapper, IServiceProvider>((o, m, s) => {
					// Creating nullable and collection mappers with AsyncEmptyMapper to avoid recursion,
					// the element mapper will be overridden by composite mapper
					o.Mappers.Add(new AsyncNullableMapper(AsyncEmptyMapper.Instance));
					o.Mappers.Add(new AsyncCollectionMapper(
						AsyncEmptyMapper.Instance,
						s.GetService<IOptionsMonitor<AsyncCollectionMappersOptions>>()?.CurrentValue,
						s.GetService<IMatcher>(),
						s.GetService<IOptionsMonitor<MergeCollectionsOptions>>()?.CurrentValue));
					o.Mappers.Add(new AsyncIMapperWrapperMapper(m));
				});


			// Register mapper services

			// Normal mapper
			services.Add(new ServiceDescriptor(
				typeof(AsyncCustomMapper),
				s => new AsyncCustomMapper(
					s.GetService<IOptionsMonitor<CustomMapsOptions>>()?.CurrentValue,
					s.GetService<IOptionsMonitor<CustomAsyncNewAdditionalMapsOptions>>()?.CurrentValue,
					s.GetService<IOptionsMonitor<CustomAsyncMergeAdditionalMapsOptions>>()?.CurrentValue,
					s),
				asyncMappersLifetime));

			// Collection mapper
			services.Add(new ServiceDescriptor(
				typeof(AsyncCollectionMapper),
				s => new AsyncCollectionMapper(
					s.GetRequiredService<IAsyncMapper>(),
					s.GetService<IOptionsMonitor<AsyncCollectionMappersOptions>>()?.CurrentValue,
					s.GetService<IMatcher>(),
					s.GetService<IOptionsMonitor<MergeCollectionsOptions>>()?.CurrentValue),
				asyncMappersLifetime));

			// Composite mapper
			services.Add(new ServiceDescriptor(
				typeof(AsyncCompositeMapper),
				s => new AsyncCompositeMapper(s.GetService<IOptionsMonitor<AsyncCompositeMapperOptions>>()?.CurrentValue.Mappers ?? []),
				asyncMappersLifetime));

			// IAsyncMapper, IAsyncMapperFactory
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
					// Creating nullable and collection mappers with EmptyProjector to avoid recursion,
					// the element projector will be overridden by composite projector
					o.Projectors.Add(new NullableProjector(EmptyProjector.Instance));
					o.Projectors.Add(new CollectionProjector(EmptyProjector.Instance));
				});


			// Register projector services

			// Normal projectors
			services.Add(new ServiceDescriptor(
				typeof(CustomProjector),
				s => new CustomProjector(
					s.GetService<IOptionsMonitor<CustomMapsOptions>>()?.CurrentValue,
					s.GetService<IOptionsMonitor<CustomProjectionAdditionalMapsOptions>>()?.CurrentValue,
					s),
				projectorsLifetime));

			// Nullable projector
			services.Add(new ServiceDescriptor(
				typeof(NullableProjector),
				s => new NullableProjector(s.GetRequiredService<IProjector>()),
				projectorsLifetime));

			// Collection projector
			services.Add(new ServiceDescriptor(
				typeof(CollectionProjector),
				s => new CollectionProjector(s.GetRequiredService<IProjector>()),
				projectorsLifetime));

			// Composite projector
			services.Add(new ServiceDescriptor(
				typeof(CompositeProjector),
				s => new CompositeProjector(s.GetService<IOptionsMonitor<CompositeProjectorOptions>>()?.CurrentValue.Projectors ?? []),
				projectorsLifetime));

			// IProjector
			services.Add(new ServiceDescriptor(typeof(IProjector), s => s.GetRequiredService<CompositeProjector>(), projectorsLifetime));
			#endregion

			return services;
		}
	}
}
