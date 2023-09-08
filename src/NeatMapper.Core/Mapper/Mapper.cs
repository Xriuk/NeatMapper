using Microsoft.Extensions.DependencyInjection;
using NeatMapper.Core.Configuration;

namespace NeatMapper.Core.Mapper {
	internal sealed class Mapper : IMapper {
		private readonly IMapperConfiguration _configuration;
		private readonly IServiceProvider _serviceProvider;
		private MappingContext? mappingContext;

		public Mapper(IMapperConfiguration configuration, IServiceProvider serviceProvider) {
			_configuration = configuration;
			_serviceProvider = serviceProvider;
		}
		internal Mapper(IMapperConfiguration configuration, MappingContext mappingContext) : this(configuration, mappingContext.ServiceProvider) {}


		public TDestination Map<TSource, TDestination>(TSource source) {
			var (scope, mappingContext) = CreateScopeAndContext();

			var types = (typeof(TSource), typeof(TDestination));
			if (_configuration.Maps.ContainsKey(types)) {
				using (scope) {
					return (TDestination)_configuration.Maps[types].Invoke(null, new object?[] { source, mappingContext })!;
				}
			}
			else if (_configuration.MergeMaps.ContainsKey(types)) {
				using (scope) {
					var destination = default(TDestination);
					_configuration.Maps[types].Invoke(null, new object?[] { source, destination, mappingContext });
					return destination!;
				}
			}
			else
				throw new ArgumentException($"No map or merge map could be found for the given types: {typeof(TSource).Name} -> {typeof(TDestination).Name}");
		}

		public void Map<TSource, TDestination>(TSource source, TDestination destination) {
			var types = (typeof(TSource), typeof(TDestination));
			if (_configuration.MergeMaps.ContainsKey(types)) {
				var (scope, mappingContext) = CreateScopeAndContext();
				using (scope) {
					_configuration.Maps[types].Invoke(null, new object?[] { source, destination, mappingContext });
				}
			}
			else
				throw new ArgumentException($"No merge map could be found for the given types: {typeof(TSource).Name} -> {typeof(TDestination).Name}");
		}

		private (IServiceScope?, MappingContext) CreateScopeAndContext() {
			if (mappingContext == null) {
				var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
				var context = new MappingContext {
					ServiceProvider = scope.ServiceProvider
				};
				var mapper = new Mapper(_configuration, context);
				context.Mapper = mapper;
				return (scope, context);
			}
			else
				return (null, mappingContext);
		}
	}
}
