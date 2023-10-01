using NeatMapper.Configuration;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NeatMapper.Common.Mapper {
	public abstract class BaseMapper : IMatcher {
		internal sealed class EmptyServiceProvider : IServiceProvider {
			public object? GetService(Type serviceType) {
				throw new NotImplementedException();
			}
		}
		internal static readonly EmptyServiceProvider EmptyServiceProviderInstance = new EmptyServiceProvider();
		internal sealed class MapData {
			public IReadOnlyDictionary<(Type From, Type To), Map> Maps { get; init; } = null!;

			public IEnumerable<GenericMap> GenericMaps { get; init; } = null!;

			public Dictionary<(Type From, Type To), Func<object?[], object?>> GenericCache { get; init; } = new Dictionary<(Type From, Type To), Func<object?[], object?>>();
		}

		// T[] Enumerable.ToArray(this IEnumerable<T> source);
		private static readonly MethodInfo Enumerable_ToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))
			?? throw new InvalidOperationException($"Cannot find method {nameof(Enumerable)}.{nameof(Enumerable.ToArray)}");

		protected static Dictionary<Type, object> nonStaticMapsInstances = new Dictionary<Type, object>();
		protected static Dictionary<Type, string?> typeCreationErrors = new Dictionary<Type, string?>();

		internal readonly IMapperConfiguration _configuration;
		protected readonly IServiceProvider _serviceProvider;
		protected abstract MatchingContext MatchingContext { get; }

		internal MapData newMaps;
		internal MapData mergeMaps;
		internal MapData collectionElementComparers;

		internal BaseMapper(IMapperConfiguration configuration, IServiceProvider? serviceProvider = null) {
			_configuration = configuration;
			_serviceProvider = serviceProvider ?? EmptyServiceProviderInstance;

			newMaps = new MapData {
				Maps = _configuration.NewMaps,
				GenericMaps = _configuration.GenericNewMaps
			};
			mergeMaps = new MapData {
				Maps = _configuration.MergeMaps,
				GenericMaps = _configuration.GenericMergeMaps
			};
			collectionElementComparers = new MapData {
				Maps = _configuration.Matchers,
				GenericMaps = _configuration.GenericMatchers
			};
		}


		public bool Match(object? source, Type sourceType, object? destination, Type destinationType) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source?.GetType().IsAssignableTo(sourceType) == false)
				throw new ArgumentException($"Object of type {source.GetType().FullName} is not assignable to type {sourceType.FullName}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));
			if (destination?.GetType().IsAssignableTo(destinationType) == false)
				throw new ArgumentException($"Object of type {destination.GetType().FullName} is not assignable to type {destinationType.FullName}", nameof(destination));

			var types = (From: sourceType, To: destinationType);
			return ElementComparerInternal(types, false).Invoke(new object?[] { source, destination, MatchingContext })!;
		}


		#region Mapping methods
		// (source, context) => destination
		// (source, destination, context) => destination
		internal static Func<object?[], object?> MapInternal((Type From, Type To) types, MapData mapData) {
			// Try retrieving a regular map
			// or try matching to a generic one
			if (mapData.Maps.ContainsKey(types)) {
				var map = mapData.Maps[types];
				return (parameters) => {
					try {
						return map.Method.Invoke(map.Method.IsStatic ? null : CreateOrReturnInstance(map.Class), parameters)!;
					}
					catch(Exception e) {
						throw new MappingException(e, types);
					}
				};
			}
			else {
				// Try retrieving from cache
				if(mapData.GenericCache.TryGetValue(types, out var method)) 
					return method;

				foreach(var map in mapData.GenericMaps) {
					// Check if the two types are compatible (we'll check constraints when instantiating)
					if (!MatchOpenGenericArgumentsRecursive(map.From, types.From) ||
						!MatchOpenGenericArgumentsRecursive(map.To, types.To)) {

						continue;
					}

					// Try inferring the types
					var classArguments = InferOpenGenericArgumentsRecursive(map.From, types.From)
						.Concat(InferOpenGenericArgumentsRecursive(map.To, types.To))
						.ToArray();

					// We may have different closed types matching for the same open type argument, in this case the map should not match
					var genericArguments = map.Class.GetGenericArguments().Length;
					if (classArguments.DistinctBy(a => a.OpenGenericArgument).Count() != genericArguments ||
						classArguments.Distinct().Count() != genericArguments) {

						continue;
					}

					// Check unmanaged constraints because the CLR seems to not enforce it
					if(classArguments.Any(a => a.OpenGenericArgument.GetCustomAttributes().Any(a => a.GetType().Name == "IsUnmanagedAttribute") &&
						!IsUnmanaged(a.ClosedType))) {

						continue;
					}

					// Try creating the type, this will verify any other constraints too
					Type concreteType;
					try {
						concreteType = MakeGenericTypeWithInferredArguments(map.Class, classArguments);
					}
					catch {
						continue;
					}

					var mapMethod = MethodBase.GetMethodFromHandle(map.Method, concreteType.TypeHandle);
					if(mapMethod == null)
						continue;


					Func<object?[], object?> func = (parameters) => {
						try {
							return mapMethod.Invoke(mapMethod.IsStatic ? null : CreateOrReturnInstance(concreteType), parameters);
						}
						catch (Exception e) {
							throw new MappingException(e, types);
						}
					};

					// Cache the method
					mapData.GenericCache.Add(types, func);

					return func;
				}
			}

			throw new MapNotFoundException(types);
		}
		#endregion

		// (source, destination, context) => bool
		protected Func<object?[], bool> ElementComparerInternal((Type From, Type To) types, bool returnDefault = true) {
			try {
				var comparer = MapInternal(types, collectionElementComparers)!;
				return (parameters) => {
					try {
						return (bool)comparer.Invoke(parameters)!;
					}
					catch (MappingException e) {
						throw new MatcherException(e.InnerException!, types);
					}
				};
			}
			catch (MapNotFoundException) {
				if(returnDefault)
					return (_) => false;
				else
					throw new MatcherNotFound(types);
			}
		}

		protected static string CreateStringFactory() {
			return string.Empty;
		}

		protected static Func<object> CreateDestinationFactory(Type destination) {
			if (destination == typeof(string))
				return CreateStringFactory;
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

			lock (typeCreationErrors) {
				if (typeCreationErrors.TryGetValue(destination, out var error)) {
					if (error == null)
						return () => Activator.CreateInstance(destination)!;
					else
						throw new DestinationCreationException(destination, new Exception(error));
				}
				else {
					// Try creating an instance
					try {
						Activator.CreateInstance(destination);
						typeCreationErrors.Add(destination, null);
						return () => Activator.CreateInstance(destination)!;
					}
					catch (Exception e) {
						typeCreationErrors.Add(destination, e.Message);
						throw new DestinationCreationException(destination, e);
					}
				}
			}
		}

		internal static object CreateOrReturnInstance(Type classType) {
			lock (nonStaticMapsInstances) { 
				if(!nonStaticMapsInstances.TryGetValue(classType, out var instance)){
					try {
						instance = CreateDestinationFactory(classType).Invoke();
						nonStaticMapsInstances.Add(classType, instance);
					}
					catch (Exception e) {
						throw new InvalidOperationException($"Could not create instance of type {classType.FullName ?? classType.Name} for non static interface", e);
					}
				}

				return instance;
			}
		}

		#region Collection methods
		protected static bool CanCreateCollection(Type destination) {
			if (destination.IsArray)
				return true;
			else if (destination.IsGenericType) {
				var collectionDefinition = destination.GetGenericTypeDefinition();
				if (collectionDefinition == typeof(ReadOnlyCollection<>) ||
					collectionDefinition == typeof(ReadOnlyDictionary<,>) ||
					collectionDefinition == typeof(ReadOnlyObservableCollection<>)) { 

					return true;
				}
			}

			if (destination == typeof(string))
				return true;
			else if (destination.IsInterface && destination.IsGenericType) {
				var interfaceDefinition = destination.GetGenericTypeDefinition();
				if (interfaceDefinition == typeof(IEnumerable<>) || interfaceDefinition == typeof(IList<>) || interfaceDefinition == typeof(ICollection<>) ||
					interfaceDefinition == typeof(IReadOnlyList<>) || interfaceDefinition == typeof(IReadOnlyCollection<>) ||
					interfaceDefinition == typeof(IDictionary<,>) || interfaceDefinition == typeof(IReadOnlyDictionary<,>) ||
					interfaceDefinition == typeof(ISet<>) || interfaceDefinition == typeof(IReadOnlySet<>)) {

					return true;
				}
			}

			lock (typeCreationErrors) {
				if (typeCreationErrors.TryGetValue(destination, out var error))
					return error == null;
				else {
					// Try creating an instance
					try {
						Activator.CreateInstance(destination);
						typeCreationErrors.Add(destination, null);
						return true;
					}
					catch (Exception e) {
						typeCreationErrors.Add(destination, e.Message);
						return false;
					}
				}
			}
		}

		// Create a non-readonly collection which could be later converted to the given type
		protected static object CreateCollection(Type destination) {
			if (destination.IsArray)
				return Activator.CreateInstance(typeof(List<>).MakeGenericType(destination.GetElementType()!))!;
			else if (destination.IsGenericType) {
				var collectionDefinition = destination.GetGenericTypeDefinition();
				if (collectionDefinition == typeof(ReadOnlyCollection<>))
					return Activator.CreateInstance(typeof(List<>).MakeGenericType(destination.GetGenericArguments()))!;
				else if (collectionDefinition == typeof(ReadOnlyDictionary<,>))
					return Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(destination.GetGenericArguments()))!;
				else if (collectionDefinition == typeof(ReadOnlyObservableCollection<>))
					destination = typeof(ObservableCollection<>).MakeGenericType(destination.GetGenericArguments());
			}

			return CreateDestinationFactory(destination).Invoke();
		}

		// Returns an instance method which can be invoked with a single parameter to be added to the collection
		protected static MethodInfo GetCollectionAddMethod(object collection) {
			var collectionInstanceType = collection.GetType();
			var collectionInterface = collectionInstanceType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
			if (collectionInterface != null)
				return collectionInstanceType.GetInterfaceMap(collectionInterface).TargetMethods.First(m => m.Name.EndsWith(nameof(ICollection<object>.Add)));
			else if (collectionInstanceType.IsGenericType) {
				var collectionGenericType = collectionInstanceType.GetGenericTypeDefinition();
				if (collectionGenericType == typeof(Queue<>)) {
					return collectionInstanceType.GetMethod(nameof(Queue<object>.Enqueue))
						?? throw new InvalidOperationException($"Cannot find method {nameof(Queue)}.{nameof(Queue<object>.ToArray)}");
				}
				else if (collectionGenericType == typeof(Stack<>)) {
					return collectionInstanceType.GetMethod(nameof(Stack<object>.Push))
						?? throw new InvalidOperationException($"Cannot find method {nameof(Queue)}.{nameof(Queue<object>.ToArray)}");
				}
			}

			throw new InvalidOperationException("Invalid collection"); // Should not happen
		}

		protected static object ConvertCollectionToType(object collection, Type destination) {
			if (destination.IsArray)
				return Enumerable_ToArray.MakeGenericMethod(destination.GetElementType()!).Invoke(null, new object[] { collection })!;
			else if (destination.IsGenericType) {
				var collectionDefinition = destination.GetGenericTypeDefinition();
				if (collectionDefinition == typeof(ReadOnlyCollection<>)) {
					return typeof(ReadOnlyCollection<>).MakeGenericType(destination.GetGenericArguments()).GetConstructors()
						.First(c => {
							var param = c.GetParameters().Single();
							return param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(IList<>);
						}).Invoke(new object[] { collection });
				}
				else if (collectionDefinition == typeof(ReadOnlyDictionary<,>)) {
					return typeof(ReadOnlyDictionary<,>).MakeGenericType(destination.GetGenericArguments()).GetConstructors()
						.First(c => {
							var param = c.GetParameters().Single();
							return param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(IDictionary<,>);
						}).Invoke(new object[] { collection });
				}
				else if (collectionDefinition == typeof(ReadOnlyObservableCollection<>)) {
					return typeof(ReadOnlyObservableCollection<>).MakeGenericType(destination.GetGenericArguments()).GetConstructors()
						.First(c => {
							var param = c.GetParameters().Single();
							return param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(ObservableCollection<>);
						}).Invoke(new object[] { collection });
				}
			}

			return collection;
		}
		#endregion

		#region Types methods
		protected static readonly MethodInfo RuntimeHelpers_IsReferenceOrContainsReference =
			typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences))!
				?? throw new InvalidOperationException("Could not find RuntimeHelpers.IsReferenceOrContainsReferences");
		protected static bool IsUnmanaged(Type type) {
			return !(bool)RuntimeHelpers_IsReferenceOrContainsReference.MakeGenericMethod(type).Invoke(null, null)!;
		}

		// Checks if two types are compatible, does not test any constraints
		protected static bool MatchOpenGenericArgumentsRecursive(Type openType, Type closedType) {
			if (!openType.IsGenericType) {
				if(openType.IsArray)
					return closedType.IsArray && MatchOpenGenericArgumentsRecursive(openType.GetElementType()!, closedType.GetElementType()!);
				else
					return openType.IsGenericTypeParameter || openType == closedType;
			}
			else if (!closedType.IsGenericType)
				return false;
			else if(openType.GetGenericTypeDefinition() != closedType.GetGenericTypeDefinition())
				return false;
			
			var openTypeArguments = openType.GetGenericArguments();
			var closedTypeArguments = closedType.GetGenericArguments();
			if (openTypeArguments.Length != closedTypeArguments.Length)
				return false;

			IEnumerable<(Type OpenTypeArgument, Type ClosedTypeArgument)> arguments = openTypeArguments.Zip(closedTypeArguments);
			return arguments.All((a) => MatchOpenGenericArgumentsRecursive(a.OpenTypeArgument, a.ClosedTypeArgument));
		}

		protected static IEnumerable<(Type OpenGenericArgument, Type ClosedType)> InferOpenGenericArgumentsRecursive(Type openType, Type closedType) {
			if (!openType.IsGenericType) {
				if (openType.IsGenericTypeParameter)
					return new[] { (openType, closedType) };
				else if(openType.IsArray)
					return InferOpenGenericArgumentsRecursive(openType.GetElementType()!, closedType.GetElementType()!);
				else
					return Enumerable.Empty<(Type, Type)>();
			}
			else
				return openType.GetGenericArguments().Zip(closedType.GetGenericArguments()).SelectMany((a) => InferOpenGenericArgumentsRecursive(a.First, a.Second));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static Type MakeGenericTypeWithInferredArguments(Type openType, IEnumerable<(Type OpenGenericArgument, Type ClosedType)> arguments) {
			return openType.MakeGenericType(openType.GetGenericArguments().Select(oa => arguments.First(a => a.OpenGenericArgument == oa).ClosedType).ToArray());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static bool HasInterface(Type type, Type interfaceType) {
			return (type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType) ||
				type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static Type GetInterfaceElementType(Type collection, Type interfaceType) {
			return (collection.IsGenericType && collection.GetGenericTypeDefinition() == interfaceType) ?
				collection.GetGenericArguments()[0] :
				collection.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType).GetGenericArguments()[0];
		}
		#endregion
	}
}
