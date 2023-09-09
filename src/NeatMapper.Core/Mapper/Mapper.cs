using Microsoft.Extensions.DependencyInjection;
using NeatMapper.Core.Configuration;
using System.Reflection;

namespace NeatMapper.Core.Mapper {
	internal sealed class Mapper : IMapper {
		private readonly IMapperConfiguration _configuration;
		private readonly IServiceProvider _serviceProvider;
		private readonly MappingContext? _mappingContext;

		public Mapper(IMapperConfiguration configuration, IServiceProvider serviceProvider) {
			_configuration = configuration;
			_serviceProvider = serviceProvider;
		}
		internal Mapper(IMapperConfiguration configuration, MappingContext mappingContext) : this(configuration, mappingContext.ServiceProvider) {
			_mappingContext = mappingContext;
		}


		public TDestination Map<TSource, TDestination>(TSource source) {
			var types = (From: typeof(TSource), To: typeof(TDestination));
			if (_configuration.NewMaps.ContainsKey(types)) {
				var (scope, mappingContext) = CreateScopeAndContext();
				using (scope) {
					return (TDestination)_configuration.NewMaps[types].Invoke(null, new object?[] { source, mappingContext })!;
				}
			}
			else if (types.From.IsGenericType || types.To.IsGenericType) {
				var map = _configuration.GenericNewMaps.FirstOrDefault(m =>
					MapperConfiguration.MatchOpenGenericArgumentsRecursive(m.From, types.From) &&
					MapperConfiguration.MatchOpenGenericArgumentsRecursive(m.To, types.To));
				if (map != null) {
					var classArguments = InferOpenGenericArgumentsRecursive(map.From, types.From)
						.Concat(InferOpenGenericArgumentsRecursive(map.To, types.To))
						.ToArray();

					// We may have different closed types matching for the same open type argument, in this case the map should not match
					if (classArguments.DistinctBy(a => a.OpenGenericArgument).Count() == classArguments.Length) {
						var (scope, mappingContext) = CreateScopeAndContext();
						using (scope) {
							return (TDestination)MethodInfo.GetMethodFromHandle(map.Method, MakeGenericTypeWithInferredArguments(map.Class, classArguments).TypeHandle)!
								.Invoke(null, new object?[] { source, mappingContext })!;
						}
					}
				}
			}

			var destination = (TDestination)(types.To == typeof(string) ?
				string.Empty :
				Activator.CreateInstance(types.To)!);
			return Map(source, destination);
		}

		public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) {
			var types = (From: typeof(TSource), To: typeof(TDestination));
			if (_configuration.MergeMaps.ContainsKey(types)) {
				var (scope, mappingContext) = CreateScopeAndContext();
				using (scope) {
					return (TDestination)_configuration.MergeMaps[types].Invoke(null, new object?[] { source, destination, mappingContext })!;
				}
			}
			else if(types.From.IsGenericType || types.To.IsGenericType) {
				var map = _configuration.GenericMergeMaps.FirstOrDefault(m =>
					MapperConfiguration.MatchOpenGenericArgumentsRecursive(m.From, types.From) &&
					MapperConfiguration.MatchOpenGenericArgumentsRecursive(m.To, types.To));
				if(map != null) {
					var classArguments = InferOpenGenericArgumentsRecursive(map.From, types.From)
						.Concat(InferOpenGenericArgumentsRecursive(map.To, types.To))
						.ToArray();

					// We may have different closed types matching for the same open type argument, in this case the map should not match
					if(classArguments.DistinctBy(a => a.OpenGenericArgument).Count() == classArguments.Length) {
						var (scope, mappingContext) = CreateScopeAndContext();
						using(scope) {
							return (TDestination)MethodInfo.GetMethodFromHandle(map.Method, MakeGenericTypeWithInferredArguments(map.Class, classArguments).TypeHandle)!
								.Invoke(null, new object?[] { source, destination, mappingContext })!;
						}
					}
				}
			}

			throw new ArgumentException($"No map could be found for the given types: {typeof(TSource).Name} -> {typeof(TDestination).Name}\n{typeof(TSource).FullName} -> {typeof(TDestination).FullName}");
		}

		private (IServiceScope?, MappingContext) CreateScopeAndContext() {
			if (_mappingContext == null) {
				var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
				var mappingContext = new MappingContext {
					ServiceProvider = scope.ServiceProvider
				};

				// New mapper to avoid creating new scopes
				var mapper = new Mapper(_configuration, mappingContext);
				mappingContext.Mapper = mapper;

				return (scope, mappingContext);
			}
			else
				return (null, _mappingContext);
		}

		private static IEnumerable<(Type OpenGenericArgument, Type ClosedType)> InferOpenGenericArgumentsRecursive(Type openType, Type closedType) {
			if(!openType.IsGenericType) {
				if(openType.IsGenericTypeParameter)
					return new[] { (openType, closedType) };
				else
					return Enumerable.Empty<(Type, Type)>();
			}
			else
				return openType.GetGenericArguments().Zip(closedType.GetGenericArguments()).SelectMany((a) => InferOpenGenericArgumentsRecursive(a.First, a.Second));
		}

		private static Type MakeGenericTypeWithInferredArguments(Type openType, IEnumerable<(Type OpenGenericArgument, Type ClosedType)> arguments) {
			return openType.MakeGenericType(openType.GetGenericArguments().Select(oa => arguments.First(a => a.OpenGenericArgument == oa).ClosedType).ToArray());
		}
	}
}
