using Microsoft.Extensions.DependencyInjection;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Internal;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
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


		public object? Map(object? source, Type sourceType, Type destinationType) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if(source?.GetType().IsAssignableTo(sourceType) == false)
				throw new ArgumentException($"Object of type {source.GetType().FullName} is not assignable to type {sourceType.FullName}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			var types = (From: sourceType, To: destinationType);
			var scope = CreateScopeIfNeeded();
			object? result;
			try {
				result = MapInternal(types, _configuration.NewMaps, _configuration.GenericNewMaps, scope)
					.Invoke(new object?[] { source, CreateOrReturnContext(scope) });
			}
			catch(ArgumentException e1) {
				if(!IsMapMissing(e1))
					throw;

				if(types.To.IsArray)
					ThrowNoMapFound(types);

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
					var destination = CreateDestinationFactory(types.To).Invoke();
					var destinationInstanceType = destination.GetType();
					var addMethod = destinationInstanceType.GetInterfaceMap(destinationInstanceType.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)))
						.TargetMethods.First(m => m.Name == nameof(ICollection<object>.Add));

					var context = CreateOrReturnContext(scope!);
					using (scope) { 
						foreach (var element in sourceEnumerable) {
							addMethod.Invoke(destination, new object[]{ elementMapper.Invoke(new object[] { element, context }) });
						}
					}

					result = destination;

					goto End;
				}

				MergeMap:

				result = Map(source, sourceType, CreateDestinationFactory(destinationType).Invoke(), destinationType);
			}

			End:

			if (result?.GetType().IsAssignableTo(destinationType) == false)
				throw new InvalidOperationException($"Object of type {result.GetType().FullName} is not assignable to type {destinationType.FullName}");

			return result;
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source?.GetType().IsAssignableTo(sourceType) == false)
				throw new ArgumentException($"Object of type {source.GetType().FullName} is not assignable to type {sourceType.FullName}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));
			if (destination?.GetType().IsAssignableTo(destinationType) == false)
				throw new ArgumentException($"Object of type {destination.GetType().FullName} is not assignable to type {destinationType.FullName}", nameof(destination));

			var types = (From: sourceType, To: destinationType);
			var scope = CreateScopeIfNeeded();
			object? result;
			try {
				result = MapInternal(types, _configuration.MergeMaps, _configuration.GenericMergeMaps, scope)
					.Invoke(new object?[] { source, destination, CreateOrReturnContext(scope!) });
			}
			catch (ArgumentException e1) {
				if (!IsMapMissing(e1))
					throw;

				var destinationInstanceType = destination?.GetType();

				if (HasInterface(types.From, typeof(IEnumerable<>)) && HasInterface(types.To, typeof(ICollection<>)) && destinationInstanceType?.IsArray != true && source is IEnumerable sourceEnumerable &&
					destination is IEnumerable destinationEnumerable) {

					var interfaceMap = destinationInstanceType!.GetInterfaceMap(destinationInstanceType!.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;
					
					// If the collection is readonly we cannot map to it
					if(!(bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly)))!.Invoke(destination, null)!) { 
						var elementTypes = (From: GetInterfaceElementType(types.From, typeof(IEnumerable<>)), To: GetInterfaceElementType(types.To, typeof(ICollection<>)));

						var elementComparer = ElementComparerInternal(elementTypes, _configuration.CollectionElementComparers, _configuration.GenericCollectionElementComparers);
						
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

						var elementsToRemove = new List<object>();
						var elementsToAdd = new List<object>();

						var context = CreateOrReturnContext(scope!);
						using (scope) {
							// Deleted elements
							foreach (var destinationElement in destinationEnumerable) {
								bool found = false;
								foreach (var sourceElement in sourceEnumerable) {
									if ((bool)elementComparer.Invoke(new object[] { sourceElement, destinationElement, context })) {
										found = true;
										break;
									}
								}

								if (!found)
									elementsToRemove.Add(destinationElement);
							}

							// Added/updated elements
							foreach (var sourceElement in sourceEnumerable) {
								object? matchingDestinationElement = null;
								foreach(var destinationElement in destinationEnumerable) {
									if((bool)elementComparer.Invoke(new object[] { sourceElement, destinationElement, context })){
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

						result = destination;

						goto End;
					}
				}

				throw;
			}

			End:

			if (result?.GetType().IsAssignableTo(destinationType) == false)
				throw new InvalidOperationException($"Object of type {result.GetType().FullName} is not assignable to type {destinationType.FullName}");

			return result;
		}

		public async Task<object?> MapAsync(object? source, Type sourceType, Type destinationType, CancellationToken cancellationToken = default) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source?.GetType().IsAssignableTo(sourceType) == false)
				throw new ArgumentException($"Object of type {source.GetType().FullName} is not assignable to type {sourceType.FullName}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			var types = (From: sourceType, To: destinationType);
			var scope = CreateScopeIfNeeded();
			object? result;
			try {
				result = await TaskUtils.AwaitTask<object?>((Task)MapInternal(types, _configuration.AsyncNewMaps, _configuration.AsyncGenericNewMaps, scope)
					.Invoke(new object?[] { source, CreateOrReturnAsyncContext(scope!, cancellationToken) }));
			}
			catch (ArgumentException e1) {
				if (!IsMapMissing(e1))
					throw;

				if (types.To.IsArray)
					ThrowNoMapFound(types);

				if (HasInterface(types.From, typeof(IEnumerable<>)) && HasInterface(types.To, typeof(IEnumerable<>)) && source is IEnumerable sourceEnumerable) {
					var elementTypes = (From: GetInterfaceElementType(types.From, typeof(IEnumerable<>)), To: GetInterfaceElementType(types.To, typeof(IEnumerable<>)));

					Func<object?[], object> elementMapper;
					try {
						// (source, context) => Task<destination>
						elementMapper = MapInternal(elementTypes, _configuration.AsyncNewMaps, _configuration.AsyncGenericNewMaps, null);
					}
					catch (ArgumentException e2) {
						if (!IsMapMissing(e2))
							throw;

						try {
							// (source, destination, context) => Task<destination>
							var destinationElementMapper = MapInternal(elementTypes, _configuration.AsyncMergeMaps, _configuration.AsyncGenericMergeMaps, null);

							var destinationElementFactory = CreateDestinationFactory(elementTypes.To);

							elementMapper = (sourceContext) => destinationElementMapper.Invoke(new object[] { sourceContext[0]!, destinationElementFactory.Invoke(), sourceContext[1]! });
						}
						catch (ArgumentException e3) {
							if (!IsMapMissing(e3))
								throw;

							goto MergeMap;
						}
					}

					// The created destination will always be a non-readonly collection so the addMethod is always present
					var destination = CreateDestinationFactory(types.To).Invoke();
					var destinationInstanceType = destination.GetType();
					var addMethod = destinationInstanceType.GetInterfaceMap(destinationInstanceType.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)))
						.TargetMethods.First(m => m.Name == nameof(ICollection<object>.Add));

					var context = CreateOrReturnAsyncContext(scope!, cancellationToken);
					using (scope) {
						foreach (var sourceElement in sourceEnumerable) {
							var destinationElement = await TaskUtils.AwaitTask<object>((Task)elementMapper.Invoke(new object[] { sourceElement, context })!);
							addMethod.Invoke(destination, new object[] { destinationElement });
						}
					}

					result = destination;

					goto End;
				}

				MergeMap:

				result = await MapAsync(source, sourceType, CreateDestinationFactory(destinationType).Invoke(), destinationType, cancellationToken);
			}

			End:

			if (result?.GetType().IsAssignableTo(destinationType) == false)
				throw new InvalidOperationException($"Object of type {result.GetType().FullName} is not assignable to type {destinationType.FullName}");

			return result;
		}

		public async Task<object?> MapAsync(object? source, Type sourceType, object? destination, Type destinationType, CancellationToken cancellationToken = default) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source?.GetType().IsAssignableTo(sourceType) == false)
				throw new ArgumentException($"Object of type {source.GetType().FullName} is not assignable to type {sourceType.FullName}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));
			if (destination?.GetType().IsAssignableTo(destinationType) == false)
				throw new ArgumentException($"Object of type {destination.GetType().FullName} is not assignable to type {destinationType.FullName}", nameof(destination));

			var scope = CreateScopeIfNeeded();
			var result = await TaskUtils.AwaitTask<object?>((Task)MapInternal((sourceType, destinationType), _configuration.AsyncMergeMaps, _configuration.AsyncGenericMergeMaps, scope)
				.Invoke(new object?[] { source, destination, CreateOrReturnAsyncContext(scope!, cancellationToken) }));

			if (result?.GetType().IsAssignableTo(destinationType) == false)
				throw new InvalidOperationException($"Object of type {result.GetType().FullName} is not assignable to type {destinationType.FullName}");

			return result;
		}


		private IServiceScope? CreateScopeIfNeeded() {
			if (_mappingContext == null && _asyncMappingContext == null)
				return _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
			else
				return null;
		}

		private MappingContext CreateOrReturnContext(IServiceScope? scope) {
			if (_mappingContext == null) {
				var mappingContext = new MappingContext {
					ServiceProvider = scope?.ServiceProvider ?? _asyncMappingContext!.ServiceProvider
				};

				// New mapper to avoid creating new scopes
				var mapper = new Mapper(_configuration, mappingContext);
				mappingContext.Mapper = mapper;

				return mappingContext;
			}
			else
				return _mappingContext;
		}
		
		private AsyncMappingContext CreateOrReturnAsyncContext(IServiceScope? scope, CancellationToken cancellationToken) {
			if (_asyncMappingContext == null) {
				var mappingContext = new AsyncMappingContext {
					ServiceProvider = scope?.ServiceProvider ?? _mappingContext!.ServiceProvider,
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

		[DoesNotReturn]
		private static void ThrowNoMapFound((Type From, Type To) types) {
			throw new ArgumentException($"No map could be found for the given types: {types.From.Name} -> {types.To.Name}\n" +
				$"{types.From.FullName} -> {types.To.FullName}");
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

			ThrowNoMapFound(types);
			return null!;
		}

		private static Func<object?[], object> ElementComparerInternal((Type From, Type To) types,
			IReadOnlyDictionary<(Type From, Type To), MethodInfo> comparers,
			IEnumerable<GenericMap> genericComparers) {

			try {
				return MapInternal(types, comparers, genericComparers, null);
			}
			catch (ArgumentException e) {
				if (!IsMapMissing(e))
					throw;
				else
					return (_) => false;
			}
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
