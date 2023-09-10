using Microsoft.Extensions.DependencyInjection;
using NeatMapper.Core.Configuration;
using System.Collections;
using System.Collections.Generic;
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
			catch(ArgumentException e1) {
				if(!IsMapMissing(e1))
					throw;

				if (HasInterface(types.From, typeof(IEnumerable<>)) && HasInterface(types.To, typeof(IEnumerable<>)) && source is IEnumerable sourceEnumerable) {
					var elementTypes = (From: GetInterfaceElementType(types.From, typeof(IEnumerable<>)), To: GetInterfaceElementType(types.To, typeof(IEnumerable<>)));

					Func<object?[], object> elementMapper;
					try {
						// (source, context) => destination
						elementMapper = MapInternal(elementTypes, _configuration.NewMaps, _configuration.GenericNewMaps, null);
					}
					catch (ArgumentException e2) {
						if (!IsMapMissing(e2))
							throw;

						try { 
							// (source, destination, context) => destination
							var destinationElementMapper = MapInternal(elementTypes, _configuration.MergeMaps, _configuration.GenericMergeMaps, null);

							var destinationElementFactory = CreateDestinationFactory(elementTypes.To);

							elementMapper = (sourceContext) => destinationElementMapper.Invoke(new object[] { sourceContext[0]!, destinationElementFactory.Invoke(), sourceContext[1]! });
						}
						catch(ArgumentException e3) {
							if (!IsMapMissing(e3))
								throw;

							goto MergeMap;
						}
					}

					// The created destination will always be a non-readonly collection so the addMethod is always present
					var destination = (TDestination)CreateDestinationFactory(types.To).Invoke();
					var addMethod = types.To.GetInterfaceMap(types.To.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)))
						.TargetMethods.First(m => m.Name == nameof(ICollection<object>.Add));

					var context = CreateOrReturnContext(scope!);
					using (scope) { 
						foreach (var element in sourceEnumerable) {
							addMethod.Invoke(destination, new object[]{ elementMapper.Invoke(new object[] { element, context }) });
						}
					}

					return destination;
				}

				MergeMap:

				return Map(source, (TDestination)CreateDestinationFactory(typeof(TDestination)).Invoke());
			}
		}

		public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) {
			var types = (From: typeof(TSource), To: typeof(TDestination));
			var scope = CreateScope(_mappingContext);
			try { 
				return (TDestination)MapInternal(types, _configuration.MergeMaps, _configuration.GenericMergeMaps, scope)
					.Invoke(new object?[] { source, destination, CreateOrReturnContext(scope!) });
			}
			catch (ArgumentException e1) {
				if (!IsMapMissing(e1))
					throw;

				if (HasInterface(types.From, typeof(IEnumerable<>)) && HasInterface(types.To, typeof(ICollection<>)) && source is IEnumerable sourceEnumerable &&
					destination is IEnumerable destinationEnumerable) {

					var destinationType = destination.GetType();
					var interfaceMap = destinationType.GetInterfaceMap(destinationType.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;
					
					if(!(bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly)))!.Invoke(destination, null)!) { 
						var elementTypes = (From: GetInterfaceElementType(types.From, typeof(IEnumerable<>)), To: GetInterfaceElementType(types.To, typeof(ICollection<>)));

						Func<object?[], object> newElementMapper = null!;
						Func<object?[], object> mergeElementMapper = null!;
						Func<object> destinationElementFactory = null!;
						try {
							newElementMapper = MapInternal(elementTypes, _configuration.NewMaps, _configuration.GenericNewMaps, null);
						}
						catch (ArgumentException e2) {
							if (!IsMapMissing(e2))
								throw;
						}
						try {
							mergeElementMapper = MapInternal(elementTypes, _configuration.MergeMaps, _configuration.GenericMergeMaps, null);

							if(newElementMapper == null)
								destinationElementFactory = CreateDestinationFactory(elementTypes.To);
						}
						catch (ArgumentException e2) {
							if (!IsMapMissing(e2) || newElementMapper == null)
								throw;
						}

						var addMethod = interfaceMap.First(m => m.Name == nameof(ICollection<object>.Add));
						var removeMethod = interfaceMap.First(m => m.Name == nameof(ICollection<object>.Remove));
						var equalityComparer = (IEqualityComparer)typeof(EqualityComparer<>).MakeGenericType(elementTypes.To)
							.GetProperty(nameof(EqualityComparer<object>.Default))!.GetValue(null)!;

						var elementsToRemove = new List<object>();
						var elementsToAdd = new List<object>();

						// Added/updated elements
						var context = CreateOrReturnContext(scope!);
						using (scope) {
							foreach (var sourceElement in sourceEnumerable) {
								object? matchingDestinationElement = null;
								foreach(var destinationElement in destinationEnumerable) {
									if(equalityComparer.Equals(sourceElement, destinationElement)){
										matchingDestinationElement = destinationElement;
										break;
									}
								}

								if(matchingDestinationElement != null) {
									if(mergeElementMapper != null) { 
										var mergeResult = mergeElementMapper.Invoke(new object[] { sourceElement, matchingDestinationElement, context });
										if(mergeResult != matchingDestinationElement) {
											elementsToRemove.Add(matchingDestinationElement);
											elementsToAdd.Add(mergeResult);
										}
									}
									else {
										elementsToRemove.Add(matchingDestinationElement);
										elementsToAdd.Add(newElementMapper!.Invoke(new object[] { sourceElement, context }));
									}
								}
								else {
									if(newElementMapper != null)
										elementsToAdd.Add(newElementMapper.Invoke(new object[] { sourceElement, context }));
									else
										elementsToAdd.Add(mergeElementMapper!.Invoke(new object[] { sourceElement, destinationElementFactory.Invoke(), context }));
								}
							}

							foreach(var element in elementsToRemove) {
								removeMethod.Invoke(destination, new object[] { element });
							}

							foreach (var element in elementsToAdd) {
								addMethod.Invoke(destination, new object[] { element });
							}
						}

						return destination;
					}
				}

				throw;
			}
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

		private static bool HasInterface(Type type, Type interfaceType) {
			return (type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType) ||
				type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
		}

		private static Type GetInterfaceElementType(Type collection, Type interfaceType) {
			return (collection.IsGenericType && collection.GetGenericTypeDefinition() == interfaceType) ?
				collection.GetGenericArguments()[0] :
				collection.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType).GetGenericArguments()[0];
		}

		private static bool IsMapMissing(ArgumentException e) {
			return e.Message.StartsWith("No map could be found for the given types");
		}
	}
}
