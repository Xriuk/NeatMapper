using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	internal static class ObjectFactory {
		/// <summary>
		/// <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/>
		/// </summary>
		private static readonly MethodInfo Enumerable_ToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))
			?? throw new InvalidOperationException($"Cannot find method {nameof(Enumerable)}.{nameof(Enumerable.ToArray)}");

		private static readonly ConcurrentDictionary<Type, object> typeInstancesCache = new ConcurrentDictionary<Type, object>();

		private static readonly ConcurrentDictionary<Type, (Func<object>, Type)> factoriesCache = new ConcurrentDictionary<Type, (Func<object>, Type)>();

		private static readonly ConcurrentDictionary<Type, Action<object, object?>> collectionsCustomAddMethodsCache =
			new ConcurrentDictionary<Type, Action<object, object?>>();
		private static readonly ConcurrentDictionary<Type, Action<object, object?>> collectionsAddMethodsCache =
			new ConcurrentDictionary<Type, Action<object, object?>>();
		private static readonly ConcurrentDictionary<Type, Func<object, object?, bool>> collectionsRemoveMethodsCache =
			new ConcurrentDictionary<Type, Func<object, object?, bool>>();
		private static readonly ConcurrentDictionary<Type, Func<object, object>> collectionsConversionMethodsCache =
			new ConcurrentDictionary<Type, Func<object, object>>();
		private static readonly ConcurrentDictionary<Type, Func<object, CancellationToken, IAsyncDisposable>> asyncGetEnumeratorMethodsCache =
			new ConcurrentDictionary<Type, Func<object, CancellationToken, IAsyncDisposable>>();
		private static readonly ConcurrentDictionary<Type, Func<IAsyncDisposable, ValueTask<bool>>> asyncMoveNextMethodsCache =
			new ConcurrentDictionary<Type, Func<IAsyncDisposable, ValueTask<bool>>>();
		private static readonly ConcurrentDictionary<Type, Func<IAsyncDisposable, object?>> asyncCurrentMethodsCache =
			new ConcurrentDictionary<Type, Func<IAsyncDisposable, object?>>();


		public static Func<object> CreateFactory(Type objectType) {
			return CreateFactory(objectType, out _);
		}
		public static Func<object> CreateFactory(Type objectType, out Type actualType) {
			if (objectType == typeof(string)) {
				actualType = typeof(string);
				return CreateStringFactory;
			}

			Func<object> factory;
			(factory, actualType) = factoriesCache.GetOrAdd(objectType, type => {
				if (type.IsInterface && type.IsGenericType) {
					var interfaceDefinition = type.GetGenericTypeDefinition();
					if (interfaceDefinition == typeof(IEnumerable<>) || interfaceDefinition == typeof(IList<>) || interfaceDefinition == typeof(ICollection<>) ||
						interfaceDefinition == typeof(IReadOnlyList<>) || interfaceDefinition == typeof(IReadOnlyCollection<>)) {

						type = typeof(List<>).MakeGenericType(type.GetGenericArguments().Single());
					}
					else if (interfaceDefinition == typeof(IDictionary<,>) || interfaceDefinition == typeof(IReadOnlyDictionary<,>)) {
						type = typeof(Dictionary<,>).MakeGenericType(type.GetGenericArguments());
					}
					else if (interfaceDefinition == typeof(ISet<>)
#if NET5_0_OR_GREATER
						|| interfaceDefinition == typeof(IReadOnlySet<>)
#endif
						) {

						type = typeof(HashSet<>).MakeGenericType(type.GetGenericArguments().Single());
					}
				}

				// Check if the type has a parameterless constructor
				if(type.IsValueType || type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null) { 
					// new ValueType() or new Constr()
					var body = Expression.New(type);
					return (Expression.Lambda<Func<object>>(Expression.Convert(body, typeof(object))).Compile(), type);
				}

				return (null!, null!);
			});

			if(factory == null || actualType == null)
				throw new ObjectCreationException(objectType, new Exception("No default constructor found for type"));

			return factory;
		}

		private static string CreateStringFactory() {
			return string.Empty;
		}

		public static object Create(Type objectType) {
			return CreateFactory(objectType).Invoke();
		}

		public static bool TryCreate(Type objectType, out object instance) {
			if (CanCreate(objectType)) {
				try { 
					instance = Create(objectType);
					return true;
				}
				catch { }
			}

			instance = null!;
			return false;
		}

		public static bool CanCreate(Type objectType) {
			if (objectType == typeof(string))
				return true;
			else if (objectType.IsInterface && objectType.IsGenericType) {
				var interfaceDefinition = objectType.GetGenericTypeDefinition();
				if (interfaceDefinition == typeof(IEnumerable<>) ||
					interfaceDefinition == typeof(ICollection<>) || interfaceDefinition == typeof(IReadOnlyCollection<>) ||
					interfaceDefinition == typeof(IList<>) || interfaceDefinition == typeof(IReadOnlyList<>) ||
					interfaceDefinition == typeof(IDictionary<,>) || interfaceDefinition == typeof(IReadOnlyDictionary<,>) ||
					interfaceDefinition == typeof(ISet<>)
#if NET5_0_OR_GREATER
					|| interfaceDefinition == typeof(IReadOnlySet<>)
#endif
					) {

					return true;
				}
			}

			return objectType.IsValueType || objectType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null;
		}


		public static object GetOrCreateCached(Type objectType) {
			return typeInstancesCache.GetOrAdd(objectType, Create);
		}


		public static bool CanCreateCollection(Type objectType) {
			if(objectType == typeof(string))
				return true;
			else if (objectType.IsArray)
				return true;
			else if (objectType.IsGenericType) {
				var collectionDefinition = objectType.GetGenericTypeDefinition();
				if (collectionDefinition == typeof(ReadOnlyCollection<>) ||
					collectionDefinition == typeof(ReadOnlyDictionary<,>) ||
					collectionDefinition == typeof(ReadOnlyObservableCollection<>) ||
					collectionDefinition == typeof(IAsyncEnumerable<>)) {

					return true;
				}
			}

			return CanCreate(objectType);
		}

		// Creates a non-readonly collection which could be later converted to the given type
		public static Func<object> CreateCollectionFactory(Type objectType, out Type actualType) {
			if (objectType == typeof(string))
				objectType = typeof(StringBuilder);
			else if (objectType.IsArray)
				objectType = typeof(List<>).MakeGenericType(objectType.GetElementType()!);
			else if (objectType.IsGenericType) {
				if (objectType.IsAsyncEnumerable()) 
					objectType = typeof(List<>).MakeGenericType(objectType.GetAsyncEnumerableElementType());
				else { 
					var collectionDefinition = objectType.GetGenericTypeDefinition();
					if (collectionDefinition == typeof(ReadOnlyCollection<>))
						objectType = typeof(List<>).MakeGenericType(objectType.GetGenericArguments());
					else if (collectionDefinition == typeof(ReadOnlyDictionary<,>))
						objectType = typeof(Dictionary<,>).MakeGenericType(objectType.GetGenericArguments());
					else if (collectionDefinition == typeof(ReadOnlyObservableCollection<>))
						objectType = typeof(ObservableCollection<>).MakeGenericType(objectType.GetGenericArguments());
				}
			}

			return CreateFactory(objectType, out actualType);
		}

		// Returns an instance method which can be invoked with a single parameter to be added to the collection
		public static Action<object, object?> GetCollectionCustomAddDelegate(Type collectionType) {
			if (collectionType == typeof(StringBuilder))
				return (collection, element) => ((StringBuilder)collection).Append((char?)element ?? '\0');

			return collectionsCustomAddMethodsCache.GetOrAdd(collectionType, collection => {
				var collectionInterface = collection.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
				MethodInfo method = null!;
				if (collectionInterface != null)
					method = collection.GetInterfaceMap(collectionInterface).TargetMethods.Single(m => m.Name.EndsWith(nameof(ICollection<object>.Add)));
				else if (collection.IsGenericType) {
					var collectionGenericType = collection.GetGenericTypeDefinition();
					if (collectionGenericType == typeof(Queue<>)) {
						method = collection.GetMethod(nameof(Queue<object>.Enqueue))
							?? throw new InvalidOperationException($"Cannot find method {nameof(Queue)}.{nameof(Queue<object>.Enqueue)}");
					}
					else if (collectionGenericType == typeof(Stack<>)) {
						method = collection.GetMethod(nameof(Stack<object>.Push))
							?? throw new InvalidOperationException($"Cannot find method {nameof(Stack)}.{nameof(Stack<object>.Push)}");
					}
				}

				if(method != null) {
					var collectionParam = Expression.Parameter(typeof(object), "collection");
					var elementParam = Expression.Parameter(typeof(object), "element");
					// ((Type)collection).Add((Type)element)
					var body = Expression.Call(Expression.Convert(collectionParam, method.DeclaringType!), method, Expression.Convert(elementParam, method.GetParameters()[0].ParameterType));
					return Expression.Lambda<Action<object, object?>>(body, collectionParam, elementParam).Compile();
				}

				throw new InvalidOperationException("Invalid collection"); // Should not happen
			});
		}
		public static Action<object, object?> GetCollectionAddDelegate(Type elementType) {
			return collectionsAddMethodsCache.GetOrAdd(elementType, element => {
				var collectionType = typeof(ICollection<>).MakeGenericType(elementType);
				var collectionParam = Expression.Parameter(typeof(object), "collection");
				var elementParam = Expression.Parameter(typeof(object), "element");
				// ((ICollection<Type>)collection).Add((Type)element)
				var body = Expression.Call(Expression.Convert(collectionParam, collectionType), collectionType.GetMethod(nameof(ICollection<object>.Add))!, Expression.Convert(elementParam, elementType));
				return Expression.Lambda<Action<object, object?>>(body, collectionParam, elementParam).Compile();
			});
		}

		public static Func<object, object?, bool> GetCollectionRemoveDelegate(Type elementType) {
			return collectionsRemoveMethodsCache.GetOrAdd(elementType, element => {
				var collectionType = typeof(ICollection<>).MakeGenericType(elementType);
				var collectionParam = Expression.Parameter(typeof(object), "collection");
				var elementParam = Expression.Parameter(typeof(object), "element");
				// ((ICollection<Type>)collection).Remove((Type)element)
				var body = Expression.Call(Expression.Convert(collectionParam, collectionType), collectionType.GetMethod(nameof(ICollection<object>.Remove))!, Expression.Convert(elementParam, elementType));
				return Expression.Lambda<Func<object, object?, bool>>(body, collectionParam, elementParam).Compile();
			});
		}

		public static Func<object, object> CreateCollectionConversionFactory(Type actualType, Type destinationType) {
			if(actualType != destinationType) { 
				if (destinationType == typeof(string))
					return collection => ((StringBuilder)collection).ToString();
				else if(destinationType.IsArray || destinationType.IsGenericType) { 
					return collectionsConversionMethodsCache.GetOrAdd(destinationType, destination => {
						if (destination.IsArray) {
							var collectionParam = Expression.Parameter(typeof(object), "collection");
							// ((Type)collection).ToArray()
							var body = Expression.Call(Enumerable_ToArray.MakeGenericMethod(destination.GetElementType()!), Expression.Convert(collectionParam, actualType));
							return Expression.Lambda<Func<object, object>>(Expression.Convert(body, typeof(object)), collectionParam).Compile();
						}
						else if (destination.IsGenericType){
							ConstructorInfo constr = null!;
							if (destination.IsAsyncEnumerable()) {
								constr = typeof(DefaultAsyncEnumerable<>).MakeGenericType(destination.GetAsyncEnumerableElementType()).GetConstructors()
									.Single();
							}
							else { 
								var collectionDefinition = destination.GetGenericTypeDefinition();
								if (collectionDefinition == typeof(ReadOnlyCollection<>)) {
									constr = typeof(ReadOnlyCollection<>).MakeGenericType(destination.GetGenericArguments()).GetConstructors()
										.First(c => {
											var param = c.GetParameters().Single();
											return param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(IList<>);
										});
								}
								else if (collectionDefinition == typeof(ReadOnlyDictionary<,>)) {
									constr = typeof(ReadOnlyDictionary<,>).MakeGenericType(destination.GetGenericArguments()).GetConstructors()
										.First(c => {
											var param = c.GetParameters().Single();
											return param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(IDictionary<,>);
										});
								}
								else if (collectionDefinition == typeof(ReadOnlyObservableCollection<>)) {
									constr = typeof(ReadOnlyObservableCollection<>).MakeGenericType(destination.GetGenericArguments()).GetConstructors()
										.First(c => {
											var param = c.GetParameters().Single();
											return param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(ObservableCollection<>);
										});
								}
							}

							if(constr != null) {
								var collectionParam = Expression.Parameter(typeof(object), "collection");
								// new Constr((Type)collection)
								var body = Expression.New(constr, Expression.Convert(collectionParam, actualType));
								return Expression.Lambda<Func<object, object>>(Expression.Convert(body, typeof(object)), collectionParam).Compile();
							}
						}

						return collection => collection;
					});
				}
			}
			
			return collection => collection;
		}

		public static Func<object, CancellationToken, IAsyncDisposable> GetAsyncEnumerableGetAsyncEnumerator(Type elementType) {
			return asyncGetEnumeratorMethodsCache.GetOrAdd(elementType, type => {
				var enumerableType = typeof(IAsyncEnumerable<>).MakeGenericType(type);
				var enumerableParam = Expression.Parameter(typeof(object), "enumerable");
				var cancellationParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
				// ((IAsyncEnumerator<Type>)enumerable).GetAsyncEnumerator(cancellationToken)
				var body = Expression.Call(
					Expression.Convert(enumerableParam, enumerableType),
					enumerableType.GetMethod(nameof(IAsyncEnumerable<object>.GetAsyncEnumerator))!,
					cancellationParam);
				return Expression.Lambda<Func<object, CancellationToken, IAsyncDisposable>>(body, enumerableParam, cancellationParam).Compile();
			});
		}
		public static Func<IAsyncDisposable, ValueTask<bool>> GetAsyncEnumeratorMoveNextAsync(Type elementType) {
			return asyncMoveNextMethodsCache.GetOrAdd(elementType, type => {
				var enumeratorType = typeof(IAsyncEnumerator<>).MakeGenericType(type);
				var enumeratorParam = Expression.Parameter(typeof(IAsyncDisposable), "enumerator");
				// ((IAsyncEnumerator<Type>)enumerator).MoveNextAsync()
				var body = Expression.Call(
					Expression.Convert(enumeratorParam, enumeratorType),
					enumeratorType.GetMethod(nameof(IAsyncEnumerator<object>.MoveNextAsync))!);
				return Expression.Lambda<Func<IAsyncDisposable, ValueTask<bool>>>(body, enumeratorParam).Compile();
			});
		}
		public static Func<IAsyncDisposable, object?> GetAsyncEnumeratorCurrent(Type elementType) {
			return asyncCurrentMethodsCache.GetOrAdd(elementType, type => {
				var enumeratorType = typeof(IAsyncEnumerator<>).MakeGenericType(type);
				var enumeratorParam = Expression.Parameter(typeof(IAsyncDisposable), "enumerator");
				// (object)((IAsyncEnumerator<Type>)enumerator).Current
				var body = Expression.Convert(
					Expression.Property(
						Expression.Convert(enumeratorParam, enumeratorType),
						enumeratorType.GetProperty(nameof(IAsyncEnumerator<object>.Current))!),
					typeof(object));
				return Expression.Lambda<Func<IAsyncDisposable, object?>>(body, enumeratorParam).Compile();
			});
		}
	}
}
