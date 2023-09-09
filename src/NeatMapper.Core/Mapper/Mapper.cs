using Microsoft.Extensions.DependencyInjection;
using NeatMapper.Core.Configuration;
using System.Collections;
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
			var types = (From: typeof(TSource), To: typeof(TDestination));
			var scope = CreateScope(_mappingContext);
			try { 
				return (TDestination)MapInternal(types, _configuration.NewMaps, _configuration.GenericNewMaps, scope)
					.Invoke(new object?[] { source, CreateOrReturnContext(scope!) });
			}
			catch(ArgumentException) {
				if (IsCollection(types.From) && IsCollection(types.To) && source is IEnumerable sourceEnumerable) {
					var elementTypes = (From: GetCollectionElementType(types.From), To: GetCollectionElementType(types.To));

					Func<object?[], object> elementMapper;
					try {
						// (source, context) => destination
						elementMapper = MapInternal(elementTypes, _configuration.NewMaps, _configuration.GenericNewMaps, null);
					}
					catch (ArgumentException) {
						// (source, destination, context) => destination
						var destinationElementMapper = MapInternal(elementTypes, _configuration.MergeMaps, _configuration.GenericMergeMaps, null);
						
						var destinationElementFactory = CreateDestinationFactory(elementTypes.To);

						elementMapper =  (sourceContext) => destinationElementMapper.Invoke(new object[] { sourceContext[0]!, destinationElementFactory.Invoke(), sourceContext[1]! });
					}

					var destination = (TDestination)CreateDestinationFactory(typeof(TDestination)).Invoke();
					var addMethod = destination.GetType().GetMethod(nameof(ICollection<object>.Add)) ?? throw new InvalidOperationException("Created type is not a valid collection");

					var context = CreateOrReturnContext(scope!);
					using (scope) { 
						foreach (var element in sourceEnumerable) {
							addMethod.Invoke(destination, new object[]{ elementMapper.Invoke(new object[] { element, context }) });
						}
					}

					return destination;
				}

				return Map(source, (TDestination)CreateDestinationFactory(typeof(TDestination)).Invoke());
			}
		}

		public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) {
			var scope = CreateScope(_mappingContext);
			return (TDestination)MapInternal((typeof(TSource), typeof(TDestination)), _configuration.MergeMaps, _configuration.GenericMergeMaps, scope)
				.Invoke(new object?[] { source, destination, CreateOrReturnContext(scope!) });
		}

		public Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default) {
			try {
				var scope = CreateScope(_asyncMappingContext);
				return (Task<TDestination>)MapInternal((typeof(TSource), typeof(TDestination)), _configuration.AsyncNewMaps, _configuration.AsyncGenericNewMaps, scope)
					.Invoke(new object?[] { source, CreateOrReturnAsyncContext(scope!, cancellationToken) });
			}
			catch (ArgumentException) {
				return MapAsync(source, (TDestination)CreateDestinationFactory(typeof(TDestination)).Invoke(), cancellationToken);
			}
		}

		public Task<TDestination> MapAsync<TSource, TDestination>(TSource source, TDestination destination, CancellationToken cancellationToken = default) {
			var scope = CreateScope(_asyncMappingContext);
			return (Task<TDestination>)MapInternal((typeof(TSource), typeof(TDestination)), _configuration.AsyncMergeMaps, _configuration.AsyncGenericMergeMaps, scope)
				.Invoke(new object?[] { source, destination, CreateOrReturnAsyncContext(scope!, cancellationToken) });
		}


		private IServiceScope? CreateScope(object? context) {
			if (context == null)
				return _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
			else
				return null;
		}

		private MappingContext CreateOrReturnContext(IServiceScope scope) {
			if (_mappingContext == null) {
				var mappingContext = new MappingContext {
					ServiceProvider = scope.ServiceProvider
				};

				// New mapper to avoid creating new scopes
				var mapper = new Mapper(_configuration, mappingContext);
				mappingContext.Mapper = mapper;

				return mappingContext;
			}
			else
				return _mappingContext;
		}
		
		private AsyncMappingContext CreateOrReturnAsyncContext(IServiceScope scope, CancellationToken cancellationToken) {
			if (_asyncMappingContext == null) {
				var mappingContext = new AsyncMappingContext {
					ServiceProvider = scope.ServiceProvider,
					CancellationToken = cancellationToken
				};

				// New mapper to avoid creating new scopes
				var mapper = new Mapper(_configuration, mappingContext);
				mappingContext.Mapper = mapper;

				return mappingContext;
			}
			else
				return _asyncMappingContext;
		}

		private static Func<object?[], object> MapInternal(
			(Type From, Type To) types,
			IReadOnlyDictionary<(Type From, Type To), MethodInfo> maps,
			IEnumerable<GenericMap> genericMaps,
			IServiceScope? scope) {

			if (maps.ContainsKey(types)) {
				using (scope) {
					return (parameters) => maps[types].Invoke(null, parameters)!;
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
						using (scope) {
							return (parameters) => MethodInfo.GetMethodFromHandle(map.Method, MakeGenericTypeWithInferredArguments(map.Class, classArguments).TypeHandle)!
								.Invoke(null, parameters)!;
						}
					}
				}
			}

			throw new ArgumentException($"No map could be found for the given types: {types.From.Name} -> {types.To.Name}\n" +
				$"{types.From.FullName} -> {types.To.FullName}");
		}

		private static Func<object> CreateDestinationFactory(Type destination) {
			if (destination == typeof(string))
				return () => string.Empty;
			else if (destination.IsInterface && destination.IsGenericType) {
				var interfaceDefinition = destination.GetGenericTypeDefinition();
				if (interfaceDefinition == typeof(IEnumerable<>) || interfaceDefinition == typeof(IList<>) || interfaceDefinition == typeof(ICollection<>) ||
					interfaceDefinition == typeof(IReadOnlyList<>) || interfaceDefinition == typeof(IReadOnlyCollection<>)) {

					return () => Activator.CreateInstance(typeof(List<>).MakeGenericType(destination.GetGenericArguments().Single()))!;
				}
				else if (interfaceDefinition == typeof(IDictionary<,>) || interfaceDefinition == typeof(IReadOnlyDictionary<,>))
					return () => Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(destination.GetGenericArguments()))!;
				else if (interfaceDefinition == typeof(ISet<>) || interfaceDefinition == typeof(IReadOnlySet<>))
					return () => Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(destination.GetGenericArguments().Single()))!;
			}

			return () => Activator.CreateInstance(destination)!;
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

		private static bool IsCollection(Type type) {
			return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
				type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
		}

		private static Type GetCollectionElementType(Type collection) {
			return (collection.IsGenericType && collection.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ?
				collection.GetGenericArguments()[0] :
				collection.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GetGenericArguments()[0];
		}
	}
}
