#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NeatMapper {
	internal sealed class ObjectFactory {
		/// <summary>
		/// <see cref="StringBuilder.Append(char)"/>
		/// </summary>
		private static readonly MethodInfo StringBuilder_Append_Char = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new[] { typeof(char) })
			?? throw new InvalidOperationException($"Could not find method {nameof(StringBuilder)}.{nameof(StringBuilder.Append)}({nameof(Char)})");
		/// <summary>
		/// <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/>
		/// </summary>
		private static readonly MethodInfo Enumerable_ToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))
			?? throw new InvalidOperationException($"Cannot find method {nameof(Enumerable)}.{nameof(Enumerable.ToArray)}");

		private static readonly IDictionary<Type, string> typeCreationErrorsCache = new Dictionary<Type, string>();
		private static readonly IDictionary<Type, object> typeInstancesCache = new Dictionary<Type, object>();


		public static Func<object> CreateFactory(Type objectType) {
			return CreateFactory(objectType, out _);
		}
		public static Func<object> CreateFactory(Type objectType, out Type actualType) {
			if (objectType == typeof(string)) {
				actualType = typeof(string);
				return CreateStringFactory;
			}
			else if (objectType.IsInterface && objectType.IsGenericType) {
				var interfaceDefinition = objectType.GetGenericTypeDefinition();
				if (interfaceDefinition == typeof(IEnumerable<>) || interfaceDefinition == typeof(IList<>) || interfaceDefinition == typeof(ICollection<>) ||
					interfaceDefinition == typeof(IReadOnlyList<>) || interfaceDefinition == typeof(IReadOnlyCollection<>)) {

					var type = typeof(List<>).MakeGenericType(objectType.GetGenericArguments().Single());
					actualType = type;
					return () => Activator.CreateInstance(type);
				}
				else if (interfaceDefinition == typeof(IDictionary<,>) || interfaceDefinition == typeof(IReadOnlyDictionary<,>)) {
					var type = typeof(Dictionary<,>).MakeGenericType(objectType.GetGenericArguments());
					actualType = type;
					return () => Activator.CreateInstance(type);
				}
				else if (interfaceDefinition == typeof(ISet<>)
#if NET5_0_OR_GREATER
					|| interfaceDefinition == typeof(IReadOnlySet<>)
#endif
					) {

					var type = typeof(HashSet<>).MakeGenericType(objectType.GetGenericArguments().Single());
					actualType = type;
					return () => Activator.CreateInstance(type);
				}
			}

			lock (typeCreationErrorsCache) { 
				if (typeCreationErrorsCache.TryGetValue(objectType, out var error)) {
					if (error == null) { 
						actualType = objectType;
						return () => Activator.CreateInstance(objectType);
					}
					else
						throw new ObjectCreationException(objectType, new Exception(error));
				}
				else {
					// Try creating an instance
					try {
						Activator.CreateInstance(objectType);
						typeCreationErrorsCache.Add(objectType, null);
						actualType = objectType;
						return () => Activator.CreateInstance(objectType);
					}
					catch (Exception e) {
						typeCreationErrorsCache.Add(objectType, e.Message);
						throw new ObjectCreationException(objectType, e);
					}
				}
			}
		}

		private static string CreateStringFactory() {
			return string.Empty;
		}

		public static object Create(Type objectType) {
			return CreateFactory(objectType).Invoke();
		}

		public static bool CanCreate(Type objectType) {
			if (objectType == typeof(string))
				return true;
			else if (objectType.IsInterface && objectType.IsGenericType) {
				var interfaceDefinition = objectType.GetGenericTypeDefinition();
				if (interfaceDefinition == typeof(IEnumerable<>) || interfaceDefinition == typeof(IList<>) || interfaceDefinition == typeof(ICollection<>) ||
					interfaceDefinition == typeof(IReadOnlyList<>) || interfaceDefinition == typeof(IReadOnlyCollection<>) ||
					interfaceDefinition == typeof(IDictionary<,>) || interfaceDefinition == typeof(IReadOnlyDictionary<,>) || interfaceDefinition == typeof(ISet<>)
#if NET5_0_OR_GREATER
					|| interfaceDefinition == typeof(IReadOnlySet<>)
#endif
					) {

					return true;
				}
			}

			lock (typeCreationErrorsCache) {
				if (typeCreationErrorsCache.TryGetValue(objectType, out var error)) {
					if (error == null)
						return true;
					else
						return false;
				}
				else {
					// Try creating an instance
					try {
						Activator.CreateInstance(objectType);
						typeCreationErrorsCache.Add(objectType, null);
						return true;
					}
					catch (Exception e) {
						typeCreationErrorsCache.Add(objectType, e.Message);
						return false;
					}
				}
			}
		}


		public static object GetOrCreateCached(Type objectType) {
			lock (typeInstancesCache) { 
				if(typeInstancesCache.TryGetValue(objectType, out var obj))
					return obj;
				else {
					obj = Create(objectType);
					typeInstancesCache.Add(objectType, obj);
					return obj;
				}
			}
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
					collectionDefinition == typeof(ReadOnlyObservableCollection<>)) {

					return true;
				}
			}

			return CanCreate(objectType);
		}

		public static Func<object> CreateCollectionFactory(Type objectType, out Type actualType) {
			if (objectType == typeof(string))
				objectType = typeof(StringBuilder);
			else if (objectType.IsArray)
				objectType = typeof(List<>).MakeGenericType(objectType.GetElementType());
			else if (objectType.IsGenericType) {
				var collectionDefinition = objectType.GetGenericTypeDefinition();
				if (collectionDefinition == typeof(ReadOnlyCollection<>))
					objectType = typeof(List<>).MakeGenericType(objectType.GetGenericArguments());
				else if (collectionDefinition == typeof(ReadOnlyDictionary<,>))
					objectType = typeof(Dictionary<,>).MakeGenericType(objectType.GetGenericArguments());
				else if (collectionDefinition == typeof(ReadOnlyObservableCollection<>))
					objectType = typeof(ObservableCollection<>).MakeGenericType(objectType.GetGenericArguments());
			}

			return CreateFactory(objectType, out actualType);
		}

		// Create a non-readonly collection which could be later converted to the given type
		public static object CreateCollection(Type objectType) {
			return CreateCollectionFactory(objectType, out _).Invoke();
		}


		// Returns an instance method which can be invoked with a single parameter to be added to the collection
		public static MethodInfo GetCollectionAddMethod(object collection) {
			return GetCollectionAddMethod(collection.GetType());
		}
		public static MethodInfo GetCollectionAddMethod(Type collectionType) {
			if (collectionType == typeof(StringBuilder))
				return StringBuilder_Append_Char;

			var collectionInterface = collectionType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
			if (collectionInterface != null)
				return collectionType.GetInterfaceMap(collectionInterface).TargetMethods.First(m => m.Name.EndsWith(nameof(ICollection<object>.Add)));
			else if (collectionType.IsGenericType) {
				var collectionGenericType = collectionType.GetGenericTypeDefinition();
				if (collectionGenericType == typeof(Queue<>)) {
					return collectionType.GetMethod(nameof(Queue<object>.Enqueue))
						?? throw new InvalidOperationException($"Cannot find method {nameof(Queue)}.{nameof(Queue<object>.Enqueue)}");
				}
				else if (collectionGenericType == typeof(Stack<>)) {
					return collectionType.GetMethod(nameof(Stack<object>.Push))
						?? throw new InvalidOperationException($"Cannot find method {nameof(Stack)}.{nameof(Stack<object>.Push)}");
				}
			}

			throw new InvalidOperationException("Invalid collection"); // Should not happen
		}

		public static object ConvertCollectionToType(object collection, Type destination) {
			return CreateCollectionConversionFactory(destination).Invoke(collection);
		}

		public static Func<object, object> CreateCollectionConversionFactory(Type destination) {
			if (destination == typeof(string))
				return collection => ((StringBuilder)collection).ToString();
			if (destination.IsArray) {
				var toArray = Enumerable_ToArray.MakeGenericMethod(destination.GetElementType());
				return collection => toArray.Invoke(null, new object[] { collection });
			}
			else if (destination.IsGenericType) {
				var collectionDefinition = destination.GetGenericTypeDefinition();
				if (collectionDefinition == typeof(ReadOnlyCollection<>)) {
					var constr = typeof(ReadOnlyCollection<>).MakeGenericType(destination.GetGenericArguments()).GetConstructors()
						.First(c => {
							var param = c.GetParameters().Single();
							return param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(IList<>);
						});
					return collection => constr.Invoke(new object[] { collection });
				}
				else if (collectionDefinition == typeof(ReadOnlyDictionary<,>)) {
					var constr = typeof(ReadOnlyDictionary<,>).MakeGenericType(destination.GetGenericArguments()).GetConstructors()
						.First(c => {
							var param = c.GetParameters().Single();
							return param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(IDictionary<,>);
						});
					return collection => constr.Invoke(new object[] { collection });
				}
				else if (collectionDefinition == typeof(ReadOnlyObservableCollection<>)) {
					var constr = typeof(ReadOnlyObservableCollection<>).MakeGenericType(destination.GetGenericArguments()).GetConstructors()
						.First(c => {
							var param = c.GetParameters().Single();
							return param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(ObservableCollection<>);
						});
					return collection => constr.Invoke(new object[] { collection });
				}
			}

			return collection => collection;
		}
	}
}
