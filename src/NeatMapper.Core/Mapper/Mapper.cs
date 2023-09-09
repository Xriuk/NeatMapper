using Microsoft.Extensions.DependencyInjection;
using NeatMapper.Core.Configuration;
using System.Reflection;

namespace NeatMapper.Core.Mapper {
	internal sealed class Mapper : IMapper, IAsyncMapper {
		private readonly IMapperConfiguration _configuration;
		private readonly IServiceProvider _serviceProvider;
		private readonly MappingContext? _mappingContext;
		private readonly AsyncMappingContext? _asyncMappingContext;

		public Mapper(IMapperConfiguration configuration, IServiceProvider serviceProvider) {
			_configuration = configuration;
			_serviceProvider = serviceProvider;
		}
		internal Mapper(IMapperConfiguration configuration, MappingContext mappingContext) : this(configuration, mappingContext.ServiceProvider) {
			_mappingContext = mappingContext;
		}
		internal Mapper(IMapperConfiguration configuration, AsyncMappingContext asyncMappingContext) : this(configuration, asyncMappingContext.ServiceProvider) {
			_asyncMappingContext = asyncMappingContext;
		}


		public TDestination Map<TSource, TDestination>(TSource source) {
			try { 
				return (TDestination)MapInternal<TSource, TDestination, MappingContext>(new object?[] { source }, _configuration.NewMaps, _configuration.GenericNewMaps, CreateScopeAndContext);
			}
			catch(ArgumentException) {
				var destination = (TDestination)CreateDestination(typeof(TDestination));
				return Map(source, destination);
			}
		}

		public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) {
			return (TDestination)MapInternal<TSource, TDestination, MappingContext>(new object?[] { source, destination }, _configuration.MergeMaps, _configuration.GenericMergeMaps, CreateScopeAndContext);
		}

		public Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default) {
			try {
				return (Task<TDestination>)MapInternal<TSource, TDestination, AsyncMappingContext>(new object?[] { source }, _configuration.AsyncNewMaps, _configuration.AsyncGenericNewMaps, () => CreateScopeAndAsyncContext(cancellationToken));
			}
			catch (ArgumentException) {
				var destination = (TDestination)CreateDestination(typeof(TDestination));
				return MapAsync(source, destination, cancellationToken);
			}
		}

		public Task<TDestination> MapAsync<TSource, TDestination>(TSource source, TDestination destination, CancellationToken cancellationToken = default) {
			return (Task<TDestination>)MapInternal<TSource, TDestination, AsyncMappingContext>(new object?[] { source, destination }, _configuration.AsyncMergeMaps, _configuration.AsyncGenericMergeMaps, () => CreateScopeAndAsyncContext(cancellationToken));
		}

		private object MapInternal<TSource, TDestination, TContext>(IEnumerable<object?> parameters,
			IReadOnlyDictionary<(Type From, Type To), MethodInfo> maps,
			IEnumerable<GenericMap> genericMaps,
			Func<(IServiceScope?, TContext)> context) {

			var types = (From: typeof(TSource), To: typeof(TDestination));
			if (maps.ContainsKey(types)) {
				var (scope, mappingContext) = context.Invoke();
				using (scope) {
					return maps[types].Invoke(null, parameters.Append(mappingContext).ToArray())!;
				}
			}
			else if (types.From.IsGenericType || types.To.IsGenericType) {
				var map = genericMaps.FirstOrDefault(m =>
					MatchOpenGenericArgumentsRecursive(m.From, types.From) &&
					MatchOpenGenericArgumentsRecursive(m.To, types.To));
				if (map != null) {
					var classArguments = InferOpenGenericArgumentsRecursive(map.From, types.From)
						.Concat(InferOpenGenericArgumentsRecursive(map.To, types.To))
						.ToArray();

					// We may have different closed types matching for the same open type argument, in this case the map should not match
					var genericArguments = map.Class.GetGenericArguments().Length;
					if (classArguments.DistinctBy(a => a.OpenGenericArgument).Count() == genericArguments && classArguments.Distinct().Count() == genericArguments) {
						var (scope, mappingContext) = context.Invoke();
						using (scope) {
							return MethodInfo.GetMethodFromHandle(map.Method, MakeGenericTypeWithInferredArguments(map.Class, classArguments).TypeHandle)!
								.Invoke(null, parameters.Append(mappingContext).ToArray())!;
						}
					}
				}
			}

			throw new ArgumentException($"No map could be found for the given types: {typeof(TSource).Name} -> {typeof(TDestination).Name}\n{typeof(TSource).FullName} -> {typeof(TDestination).FullName}");
		}

		private object CreateDestination(Type destination) {
			return (destination == typeof(string) ?
				string.Empty :
				Activator.CreateInstance(destination)!);
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
		
		private (IServiceScope?, AsyncMappingContext) CreateScopeAndAsyncContext(CancellationToken cancellationToken) {
			if (_asyncMappingContext == null) {
				var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
				var mappingContext = new AsyncMappingContext {
					ServiceProvider = scope.ServiceProvider,
					CancellationToken = cancellationToken
				};

				// New mapper to avoid creating new scopes
				var mapper = new Mapper(_configuration, mappingContext);
				mappingContext.Mapper = mapper;

				return (scope, mappingContext);
			}
			else
				return (null, _asyncMappingContext);
		}

		private static bool MatchOpenGenericArgumentsRecursive(Type openType, Type closedType) {
			if (!openType.IsGenericType) {
				if(closedType.IsGenericType)
					return false;
				else
					return openType.IsGenericTypeParameter || openType == closedType;
			}
			else if (!closedType.IsGenericType)
				return false;

			var arguments1 = openType.GetGenericArguments();
			var arguments2 = closedType.GetGenericArguments();
			if (arguments1.Length != arguments2.Length)
				return false;

			return arguments1.Zip(arguments2).All((a) => MatchOpenGenericArgumentsRecursive(a.First, a.Second));
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
