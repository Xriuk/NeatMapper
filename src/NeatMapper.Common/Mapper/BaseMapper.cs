using NeatMapper.Configuration;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NeatMapper.Core.Mapper {
	public abstract class BaseMapper : IMatcher {
		internal sealed class MapData {
			public IReadOnlyDictionary<(Type From, Type To), Map> Maps { get; init; } = null!;

			public IEnumerable<GenericMap> GenericMaps { get; init; } = null!;

			public Dictionary<(Type From, Type To), Func<object?[], object?>> GenericCache { get; init; } = new Dictionary<(Type From, Type To), Func<object?[], object?>>();
		}

		// T[] Enumerable.ToArray(this IEnumerable<T> source);
		private static readonly MethodInfo Enumerable_ToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))
			?? throw new InvalidOperationException($"Cannot find method {nameof(Enumerable)}.{nameof(Enumerable.ToArray)}");

		protected static Dictionary<Type, object> nonStaticMapsInstances = new Dictionary<Type, object>();

		internal readonly IMapperConfiguration _configuration;
		protected readonly IServiceProvider _serviceProvider;
		protected abstract MatchingContext MatchingContext { get; }

		internal MapData newMaps;
		internal MapData mergeMaps;
		internal MapData collectionElementComparers;

		internal BaseMapper(IMapperConfiguration configuration, IServiceProvider serviceProvider) {
			_configuration = configuration;
			_serviceProvider = serviceProvider;

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

		// (source, context) => destination
		protected Func<object?[], object?> MapCollectionNewRecursiveInternal((Type From, Type To) types) {
			// If both types are collections try mapping the element types
			if (HasInterface(types.From, typeof(IEnumerable<>)) && types.From != typeof(string) &&
				HasInterface(types.To, typeof(IEnumerable<>)) && types.To != typeof(string)) {

				var elementTypes = (
					From: GetInterfaceElementType(types.From, typeof(IEnumerable<>)),
					To: GetInterfaceElementType(types.To, typeof(IEnumerable<>))
				);

				// (source, context) => destination
				Func<object?[], object?> elementMapper;
				try {
					// (source, context) => destination
					elementMapper = MapInternal(elementTypes, newMaps);
				}
				catch (MapNotFoundException) {
					try {
						// (source, destination, context) => destination
						var destinationElementMapper = MapInternal(elementTypes, mergeMaps);

						// () => destination
						var destinationElementFactory = CreateDestinationFactory(elementTypes.To);

						elementMapper = (sourceContext) => destinationElementMapper.Invoke(new object[] {
							sourceContext[0]!,
							destinationElementFactory.Invoke(),
							sourceContext[1]!
						});
					}
					catch (Exception e) when (e is MapNotFoundException || e is DestinationCreationException) {
						try {
							// (source, context) => destination
							elementMapper = MapCollectionNewRecursiveInternal(elementTypes);
						}
						catch (MapNotFoundException) {
							goto End;
						}
					}
				}

				// Check if collection can be created
				try {
					CreateCollection(types.To);
				}
				catch (DestinationCreationException) {
					goto End;
				}

				return (sourceAndContext) => {
					try {
						var destination = CreateCollection(types.To);
						var addMethod = GetCollectionAddMethod(destination);

						if (sourceAndContext[0] is IEnumerable sourceEnumerable) {
							foreach (var element in sourceEnumerable) {
								addMethod.Invoke(destination, new object?[] { elementMapper.Invoke(new object[] { element, sourceAndContext[1]! }) });
							}

							return ConvertCollectionToType(destination, types.To);
						}
						else if (sourceAndContext[0] == null)
							return null;
						else
							throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
					}
					catch (Exception e) {
						throw new CollectionMappingException(e, types);
					}
				};
			}

			End:

			throw new MapNotFoundException(types);
		}

		// (source, destination, context) => destination
		protected Func<object?[], object?> MapCollectionMergeRecursiveInternal(
			(Type From, Type To) types,
			object? destination,
			MappingOptions? mappingOptions = null) {

			// If both types are collections try mapping the element types
			if (HasInterface(types.From, typeof(IEnumerable<>)) && types.From != typeof(string) &&
				HasInterface(types.To, typeof(ICollection<>))) {

				if (destination == null) {
					// Check if collection can be created
					try {
						destination = CreateCollection(types.To);
					}
					catch (DestinationCreationException) {
						goto End;
					}
				}

				var elementTypes = (
					From: GetInterfaceElementType(types.From, typeof(IEnumerable<>)),
					To: GetInterfaceElementType(types.To, typeof(ICollection<>))
				);

				var destinationInstanceType = destination.GetType();
				if (!destinationInstanceType.IsArray) {
					var interfaceMap = destinationInstanceType.GetInterfaceMap(destinationInstanceType.GetInterfaces()
						.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;

					// If the collection is readonly we cannot map to it
					if (!(bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly)))!.Invoke(destination, null)!) {
						// At least one of New or Merge mapper is required to map elements
						// If both are found they will be used in the following order:
						// - elements to update will use MergeMap first (on the existing element),
						//   then NewMap (by removing the existing element and adding the new one)
						// - elements to add will use NewMap first,
						//   then MergeMap (by creating a new element and merging to it)

						// (source, context) => destination
						Func<object?[], object?> newElementMapper = null!;
						// (source, destination, context) => destination
						Func<object?[], object?> mergeElementMapper = null!;
						// () => destination
						Func<object> destinationElementFactory = null!;
						try {
							newElementMapper = MapInternal(elementTypes, newMaps);
						}
						catch (MapNotFoundException) {
							// Here we could recreate any nested collection in case the corresponding merge map would fail,
							// but we are not doing it for consistency: if a merge map on a readonly collection fails,
							// then a nested one should too
							/*try {
								newElementMapper = MapCollectionRecursiveInternal(elementTypes);
							}
							catch (MapNotFoundException) { }*/
						}
						try {
							mergeElementMapper = MapInternal(elementTypes, mergeMaps);
						}
						catch (MapNotFoundException) {
							// If the types are not collections and we don't have a newElementMapper we already know that we can't map them,
							// Otherwise we will try to retrieve the collection map inside the mapping function for each element
							// (because we need to know if the passed runtime types are arrays or not)
							if ((!HasInterface(elementTypes.From, typeof(IEnumerable<>)) || elementTypes.From == typeof(string) ||
								!HasInterface(elementTypes.To, typeof(ICollection<>))) &&
								newElementMapper == null) {

								goto End;
							}
						}
						if (newElementMapper == null) {
							try {
								destinationElementFactory = CreateDestinationFactory(elementTypes.To);
							}
							catch (DestinationCreationException) {
								goto End;
							}
						}

						Func<object?[], bool> elementComparer;
						if(mappingOptions?.Matcher != null) {
							elementComparer = (parameters) => {
								try { 
									return mappingOptions.Matcher.Invoke(parameters[0]!, parameters[1]!, (MatchingContext)parameters[2]!);
								}
								catch (Exception e) {
									throw new MatcherException(e, types);
								}
							};
						}
						else
							elementComparer = ElementComparerInternal(elementTypes);

						var addMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Add)));
						var removeMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Remove)));

						return (sourceDestinationAndContext) => {
							if (sourceDestinationAndContext[0] is IEnumerable sourceEnumerable) {
								try { 
									if (sourceDestinationAndContext[1] == null)
										sourceDestinationAndContext[1] = CreateCollection(types.To);

									if (sourceDestinationAndContext[1] is IEnumerable destinationEnumerable) {
										var elementsToRemove = new List<object?>();
										var elementsToAdd = new List<object?>();
										Func<object?[], object?> nullMergeCollectionMapping = null!;
										var mergeCollectionMappings = new Dictionary<object, Func<object?[], object?>>();

										// Deleted elements (+ missing merge mappings)
										foreach (var destinationElement in destinationEnumerable) {
											bool found = false;
											foreach (var sourceElement in sourceEnumerable) {
												if (elementComparer.Invoke(new object?[] { sourceElement, destinationElement, sourceDestinationAndContext[2] })) {
													found = true;
													break;
												}
											}

											// If not found we remove it
											// Otherwise if we don't have a merge map we try retrieving it, so that we may fail
											// before mapping elements if needed
											if (!found) {
												if(mappingOptions?.CollectionRemoveNotMatchedDestinationElements
													?? _configuration.MergeMapsCollectionsOptions.RemoveNotMatchedDestinationElements) { 

													elementsToRemove.Add(destinationElement);
												}
											}
											else if (mergeElementMapper == null &&
												(destinationElement == null ?
													nullMergeCollectionMapping == null :
													!mergeCollectionMappings.ContainsKey(destinationElement))) {

												try {
													var mapper = MapCollectionMergeRecursiveInternal(elementTypes, destinationElement);
													if(destinationElement == null)
														nullMergeCollectionMapping = mapper;
													else
														mergeCollectionMappings.Add(destinationElement, mapper);
												}
												catch (MapNotFoundException) {
													if (newElementMapper == null)
														throw;
												}
											}
										}

										// Added/updated elements
										foreach (var sourceElement in sourceEnumerable) {
											bool found = false;
											object? matchingDestinationElement = null;
											foreach (var destinationElement in destinationEnumerable) {
												if (elementComparer.Invoke(new object[] { sourceElement, destinationElement, sourceDestinationAndContext[2]! }) &&
													!elementsToRemove.Contains(destinationElement)) {

													matchingDestinationElement = destinationElement;
													found = true;
													break;
												}
											}

											bool tempMergeMapper = false;
											if (found) {
												// If we don't have an element mapper try retrieving it now
												// We don't pass along any collectionElementComparer as they are only for the current collection (if present at all)
												if (mergeElementMapper == null) {
													if(matchingDestinationElement == null) { 
														mergeElementMapper = nullMergeCollectionMapping;
														tempMergeMapper = true;
													}
													else if (mergeCollectionMappings.ContainsKey(matchingDestinationElement)) { 
														mergeElementMapper = mergeCollectionMappings[matchingDestinationElement];
														tempMergeMapper = true;
													}
												}

												if (mergeElementMapper != null) {
													var mergeResult = mergeElementMapper.Invoke(new object?[] { sourceElement, matchingDestinationElement, sourceDestinationAndContext[2] });
													if (mergeResult != matchingDestinationElement) {
														elementsToRemove.Add(matchingDestinationElement);
														elementsToAdd.Add(mergeResult);
													}
												}
												else {
													elementsToRemove.Add(matchingDestinationElement);
													elementsToAdd.Add(newElementMapper!.Invoke(new object?[] { sourceElement, sourceDestinationAndContext[2] }));
												}
											}
											else {
												if (newElementMapper != null)
													elementsToAdd.Add(newElementMapper.Invoke(new object?[] { sourceElement, sourceDestinationAndContext[2] }));
												else {
													var destinationInstance = destinationElementFactory.Invoke();

													// If we don't have an element mapper try retrieving it now
													// We don't pass along any collectionElementComparer as they are only for the current collection (if present at all)
													if (mergeElementMapper == null) {
														mergeElementMapper = MapCollectionMergeRecursiveInternal(elementTypes, destinationInstance);
														tempMergeMapper = true;
													}

													elementsToAdd.Add(mergeElementMapper!.Invoke(new object?[] { sourceElement, destinationInstance, sourceDestinationAndContext[2] }));
												}
											}

											if (tempMergeMapper)
												mergeElementMapper = null!;
										}

										foreach (var element in elementsToRemove) {
											if (!(bool)removeMethod.Invoke(sourceDestinationAndContext[1], new object?[] { element })!)
												throw new InvalidOperationException($"Could not remove element {element} from the destination collection {sourceDestinationAndContext[1]}");
										}
										foreach (var element in elementsToAdd) {
											addMethod.Invoke(sourceDestinationAndContext[1], new object?[] { element });
										}

										return ConvertCollectionToType(sourceDestinationAndContext[1]!, types.To);
									}
									else
										throw new InvalidOperationException("Destination is not an enumerable"); // Should not happen
								}
								catch (MapNotFoundException) {
									throw;
								}
								catch (Exception e) {
									throw new CollectionMappingException(e, types);
								}
							}
							else if (sourceDestinationAndContext[0] == null)
								return null;
							else
								throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
						};
					}
				}
			}

			End:

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

			// Try creating an instance
			try {
				Activator.CreateInstance(destination);
			}
			catch (Exception e) {
				throw new DestinationCreationException(destination, e);
			}
			return () => Activator.CreateInstance(destination)!;
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
