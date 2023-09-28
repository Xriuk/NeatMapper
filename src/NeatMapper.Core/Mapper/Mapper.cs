using Microsoft.Extensions.DependencyInjection;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Internal;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NeatMapper.Core.Mapper {
	internal sealed class Mapper : IMapper, IAsyncMapper {
		internal sealed class MapData {
			public IReadOnlyDictionary<(Type From, Type To), MethodInfo> Maps { get; init; } = null!;

			public IEnumerable<GenericMap> GenericMaps { get; init; } = null!;

			public Dictionary<(Type From, Type To), Func<object?[], object?>> GenericCache { get; init; } = new Dictionary<(Type From, Type To), Func<object?[], object?>>();
		}

		// T[] Enumerable.ToArray(this IEnumerable<T> source);
		private static readonly MethodInfo Enumerable_ToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))
			?? throw new InvalidOperationException($"Cannot find method {nameof(Enumerable)}.{nameof(Enumerable.ToArray)}");

		private readonly IMapperConfiguration _configuration;
		private readonly IServiceProvider _serviceProvider;
		private readonly MappingContext? _mappingContext;
		private readonly AsyncMappingContext? _asyncMappingContext;

		private MapData newMaps;
		private MapData mergeMaps;
		private MapData asyncNewMaps;
		private MapData asyncMergeMaps;
		private MapData collectionElementComparers;

		public Mapper(IMapperConfiguration configuration, IServiceProvider serviceProvider) {
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
			asyncNewMaps = new MapData {
				Maps = _configuration.AsyncNewMaps,
				GenericMaps = _configuration.AsyncGenericNewMaps
			};
			asyncMergeMaps = new MapData {
				Maps = _configuration.AsyncMergeMaps,
				GenericMaps = _configuration.AsyncGenericMergeMaps
			};
			collectionElementComparers = new MapData {
				Maps = _configuration.CollectionElementComparers,
				GenericMaps = _configuration.GenericCollectionElementComparers
			};
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
			if (source?.GetType().IsAssignableTo(sourceType) == false)
				throw new ArgumentException($"Object of type {source.GetType().FullName} is not assignable to type {sourceType.FullName}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			var types = (From: sourceType, To: destinationType);
			using(var scope = CreateScopeIfNeeded()) { 
				object? result;
				try {
					result = MapInternal(types, newMaps)
						.Invoke(new object?[] { source, CreateOrReturnContext(scope) });
				}
				catch (MapNotFoundException exc) {
					try {
						result = MapCollectionNewRecursiveInternal(types).Invoke(new object[] { source!, CreateOrReturnContext(scope!) });
					}
					catch (MapNotFoundException) {
						object destination;
						try {
							destination = CreateDestinationFactory(destinationType).Invoke();
						}
						catch (DestinationCreationException) {
							throw exc;
						}

						result = Map(source, sourceType, destination, destinationType);
					}
				}

				// Should not happen
				if (result?.GetType().IsAssignableTo(destinationType) == false)
					throw new InvalidOperationException($"Object of type {result.GetType().FullName} is not assignable to type {destinationType.FullName}");

				return result;
			}
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source?.GetType().IsAssignableTo(sourceType) == false)
				throw new ArgumentException($"Object of type {source.GetType().FullName} is not assignable to type {sourceType.FullName}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));
			if (destination?.GetType().IsAssignableTo(destinationType) == false)
				throw new ArgumentException($"Object of type {destination.GetType().FullName} is not assignable to type {destinationType.FullName}", nameof(destination));

			var types = (From: sourceType, To: destinationType);
			using(var scope = CreateScopeIfNeeded()){
				object? result;
				try {
					result = MapInternal(types, mergeMaps)
						.Invoke(new object?[] { source, destination, CreateOrReturnContext(scope!) });
				}
				catch (MapNotFoundException exc) {
					try {
						result = MapCollectionMergeRecursiveInternal(types, destination, mappingOptions).Invoke(new object[] { source!, destination!, CreateOrReturnContext(scope!) });
					}
					catch (MapNotFoundException) { 
						throw exc;
					}
				}

				// Should not happen
				if (result?.GetType().IsAssignableTo(destinationType) == false)
					throw new InvalidOperationException($"Object of type {result.GetType().FullName} is not assignable to type {destinationType.FullName}");

				return result;
			}
		}

		public async Task<object?> MapAsync(object? source, Type sourceType, Type destinationType, CancellationToken cancellationToken = default) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source?.GetType().IsAssignableTo(sourceType) == false)
				throw new ArgumentException($"Object of type {source.GetType().FullName} is not assignable to type {sourceType.FullName}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			var types = (From: sourceType, To: destinationType);
			using(var scope = CreateScopeIfNeeded()) { 
				object? result;
				try {
					result = await TaskUtils.AwaitTask<object?>((Task)MapInternal(types, asyncNewMaps)
						.Invoke(new object?[] { source, CreateOrReturnAsyncContext(scope!, cancellationToken) }));
				}
				catch (MapNotFoundException exc) {
					try {
						return await TaskUtils.AwaitTask<object?>(MapAsyncCollectionRecursiveInternal(types)
							.Invoke(new object?[] { source, CreateOrReturnAsyncContext(scope!, cancellationToken) }));
					}
					catch (MapNotFoundException) {
						try {
							result = await MapAsync(source, sourceType, CreateDestinationFactory(destinationType).Invoke(), destinationType, cancellationToken);
						}
						catch (DestinationCreationException) {
							throw exc;
						}
					}
				}

				if (result?.GetType().IsAssignableTo(destinationType) == false)
					throw new InvalidOperationException($"Object of type {result.GetType().FullName} is not assignable to type {destinationType.FullName}");

				return result;
			}
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

			var types = (From: sourceType, To: destinationType);
			using (var scope = CreateScopeIfNeeded()) {
				object? result;
				try { 
					result = await TaskUtils.AwaitTask<object?>((Task)MapInternal((sourceType, destinationType), asyncMergeMaps)
						.Invoke(new object?[] { source, destination, CreateOrReturnAsyncContext(scope!, cancellationToken) }));
				}
				catch (MapNotFoundException exc) {
					var destinationInstanceType = destination?.GetType();

					// If both types are collections try mapping the element types
					if (HasInterface(types.From, typeof(IEnumerable<>)) && HasInterface(types.To, typeof(ICollection<>)) &&
						destinationInstanceType?.IsArray != true && source is IEnumerable sourceEnumerable &&
						destination is IEnumerable destinationEnumerable) {

						var interfaceMap = destinationInstanceType!.GetInterfaceMap(destinationInstanceType!.GetInterfaces()
							.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;

						// If the collection is readonly we cannot map to it
						if (!(bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly)))!.Invoke(destination, null)!) {
							var elementTypes = (
								From: GetInterfaceElementType(types.From, typeof(IEnumerable<>)),
								To: GetInterfaceElementType(types.To, typeof(ICollection<>))
							);

							var elementComparer = ElementComparerInternal(elementTypes);

							// At least one of New or Merge mapper is required to map elements
							// If both are found they will be used in the following order:
							// - elements to update will use MergeMap first, then NewMap
							// - elements to add will use NewMap first, then MergeMap

							// (source, context) => Task<destination>
							Func<object?[], object?> newElementMapper = null!;
							// (source, destination, context) => Task<destination>
							Func<object?[], object?> mergeElementMapper = null!;
							// () => destination
							Func<object> destinationElementFactory = null!;
							try {
								newElementMapper = MapInternal(elementTypes, newMaps);
							}
							catch (MapNotFoundException) { }
							try {
								mergeElementMapper = MapInternal(elementTypes, mergeMaps);

								if (newElementMapper == null)
									destinationElementFactory = CreateDestinationFactory(elementTypes.To);
							}
							catch (Exception e) when (e is MapNotFoundException || e is DestinationCreationException) {
								if (newElementMapper == null)
									throw exc;
							}

							var addMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Add)));
							var removeMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Remove)));

							var elementsToRemove = new List<object>();
							var elementsToAdd = new List<object>();

							var comparerContext = CreateOrReturnContext(scope!);
							var mapContext = CreateOrReturnAsyncContext(scope!, cancellationToken);
							// Deleted elements
							foreach (var destinationElement in destinationEnumerable) {
								bool found = false;
								foreach (var sourceElement in sourceEnumerable) {
									if (elementComparer.Invoke(new object[] { sourceElement, destinationElement, comparerContext })) {
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
								foreach (var destinationElement in destinationEnumerable) {
									if ((bool)elementComparer.Invoke(new object[] { sourceElement, destinationElement, comparerContext })) {
										matchingDestinationElement = destinationElement;
										break;
									}
								}

								if (matchingDestinationElement != null) {
									if (mergeElementMapper != null) {
										var mergeResult = await TaskUtils.AwaitTask<object>((Task)mergeElementMapper.Invoke(new object[] { sourceElement, matchingDestinationElement, mapContext }));
										if (mergeResult != matchingDestinationElement) {
											elementsToRemove.Add(matchingDestinationElement);
											elementsToAdd.Add(mergeResult);
										}
									}
									else {
										elementsToRemove.Add(matchingDestinationElement);
										var newElement = await TaskUtils.AwaitTask<object>((Task)newElementMapper!.Invoke(new object[] { sourceElement, mapContext }));
										elementsToAdd.Add(newElement);
									}
								}
								else {
									if (newElementMapper != null) {
										var newElement = await TaskUtils.AwaitTask<object>((Task)newElementMapper.Invoke(new object[] { sourceElement, mapContext }));
										elementsToAdd.Add(newElement);
									}
									else { 
										var mergeElement = await TaskUtils.AwaitTask<object>((Task)mergeElementMapper!.Invoke(new object[] { sourceElement, destinationElementFactory.Invoke(), mapContext }));
										elementsToAdd.Add(mergeElement);
									}
								}
							}

							foreach (var element in elementsToRemove) {
								if (!(bool)removeMethod.Invoke(destination, new object[] { element })!)
									throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destination}");
							}
							foreach (var element in elementsToAdd) {
								addMethod.Invoke(destination, new object[] { element });
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
			using (var scope = CreateScopeIfNeeded()) {
				return ElementComparerInternal(types, false).Invoke(new object?[] { source, destination, CreateOrReturnContext(scope) })!;
			}
		}

		#region Scope & Context methods
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
		#endregion

		#region Mapping methods
		// (source, context) => destination
		// (source, destination, context) => destination
		// (source, context) => Task<destination>
		// (source, destination, context) => Task<destination>
		private static Func<object?[], object?> MapInternal((Type From, Type To) types, MapData mapData) {
			// Try retrieving a regular map
			// or try matching to a generic one
			if (mapData.Maps.ContainsKey(types))
				return (parameters) => {
					try {
						return mapData.Maps[types].Invoke(null, parameters)!;
					}
					catch(Exception e) {
						throw new MappingException(e, types);
					}
				};
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
					RuntimeTypeHandle concreteType;
					try {
						concreteType = MakeGenericTypeWithInferredArguments(map.Class, classArguments).TypeHandle;
					}
					catch {
						continue;
					}

					var mapMethod = MethodBase.GetMethodFromHandle(map.Method, concreteType);
					if(mapMethod == null)
						continue;

					Func<object?[], object?> func = (parameters) => {
						try {
							return mapMethod.Invoke(null, parameters);
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
		Func<object?[], object?> MapCollectionNewRecursiveInternal((Type From, Type To) types) {
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
		Func<object?[], object?> MapCollectionMergeRecursiveInternal(
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
						if(mappingOptions?.CollectionElementComparer != null) {
							elementComparer = (parameters) => {
								try { 
									return mappingOptions.CollectionElementComparer.Invoke(parameters[0]!, parameters[1]!, (MappingContext)parameters[2]!);
								}
								catch (Exception e) {
									throw new CollectionElementComparerException(e, types);
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

		// (source, context) => Task<destination>
		Func<object?[], Task<object?>> MapAsyncCollectionRecursiveInternal((Type From, Type To) types) {
			// If both types are collections try mapping the element types
			if (HasInterface(types.From, typeof(IEnumerable<>)) && types.From != typeof(string) && HasInterface(types.To, typeof(IEnumerable<>)) && types.To != typeof(string)) {
				var elementTypes = (
					From: GetInterfaceElementType(types.From, typeof(IEnumerable<>)),
					To: GetInterfaceElementType(types.To, typeof(IEnumerable<>))
				);

				// (source, context) => Task<destination>
				Func<object?[], object?> elementMapper;
				try {
					// (source, context) => Task<destination>
					elementMapper = MapInternal(elementTypes, asyncNewMaps);
				}
				catch (MapNotFoundException) {
					try {
						// (source, destination, context) => Task<destination>
						var destinationElementMapper = MapInternal(elementTypes, asyncMergeMaps);

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
							// (source, context) => Task<destination>
							elementMapper = MapAsyncCollectionRecursiveInternal(elementTypes);
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

				return async (sourceAndContext) => {
					var destination = CreateCollection(types.To);
					var addMethod = GetCollectionAddMethod(destination);

					if (sourceAndContext[0] is IEnumerable sourceEnumerable) {
						foreach (var element in sourceEnumerable) {
							var destinationElement = await TaskUtils.AwaitTask<object>((Task)elementMapper.Invoke(new object[] { element, sourceAndContext[1]! })!);
							addMethod.Invoke(destination, new object[] { destinationElement });
						}

						return ConvertCollectionToType(destination, types.To);
					}
					else if (sourceAndContext[0] == null)
						return null!;
					else
						throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
				};
			}

			End:

			throw new MapNotFoundException(types);
		}
		#endregion

		// (source, destination, context) => bool
		private Func<object?[], bool> ElementComparerInternal((Type From, Type To) types, bool returnDefault = true) {
			try {
				var comparer = MapInternal(types, collectionElementComparers)!;
				return (parameters) => {
					try {
						return (bool)comparer.Invoke(parameters)!;
					}
					catch (MappingException e) {
						throw new CollectionElementComparerException(e.InnerException!, types);
					}
				};
			}
			catch (MapNotFoundException) {
				if(returnDefault)
					return (_) => false;
				else
					throw new CollectionElementComparerNotFound(types);
			}
		}

		private static string CreateStringFactory() {
			return string.Empty;
		}

		private static Func<object> CreateDestinationFactory(Type destination) {
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

		#region Collection methods
		// Create a non-readonly collection which could be later converted to the given type
		private static object CreateCollection(Type destination) {
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
		private static MethodInfo GetCollectionAddMethod(object collection) {
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

		private static object ConvertCollectionToType(object collection, Type destination) {
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
		private static readonly MethodInfo RuntimeHelpers_IsReferenceOrContainsReference =
			typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences))!
				?? throw new InvalidOperationException("Could not find RuntimeHelpers.IsReferenceOrContainsReferences");
		private static bool IsUnmanaged(Type type) {
			return !(bool)RuntimeHelpers_IsReferenceOrContainsReference.MakeGenericMethod(type).Invoke(null, null)!;
		}

		// Checks if two types are compatible, does not test any constraints
		private static bool MatchOpenGenericArgumentsRecursive(Type openType, Type closedType) {
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

		private static IEnumerable<(Type OpenGenericArgument, Type ClosedType)> InferOpenGenericArgumentsRecursive(Type openType, Type closedType) {
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
		private static Type MakeGenericTypeWithInferredArguments(Type openType, IEnumerable<(Type OpenGenericArgument, Type ClosedType)> arguments) {
			return openType.MakeGenericType(openType.GetGenericArguments().Select(oa => arguments.First(a => a.OpenGenericArgument == oa).ClosedType).ToArray());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool HasInterface(Type type, Type interfaceType) {
			return (type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType) ||
				type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Type GetInterfaceElementType(Type collection, Type interfaceType) {
			return (collection.IsGenericType && collection.GetGenericTypeDefinition() == interfaceType) ?
				collection.GetGenericArguments()[0] :
				collection.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType).GetGenericArguments()[0];
		}
		#endregion
	}
}
