#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	internal sealed class ObjectFactory {
		static readonly IDictionary<Type, string> typeCreationErrorsCache = new ConcurrentDictionary<Type, string>();


		public static Func<object> CreateFactory(Type objectType) {
			if (objectType == typeof(string))
				return CreateStringFactory;
			else if (objectType.IsInterface && objectType.IsGenericType) {
				var interfaceDefinition = objectType.GetGenericTypeDefinition();
				if (interfaceDefinition == typeof(IEnumerable<>) || interfaceDefinition == typeof(IList<>) || interfaceDefinition == typeof(ICollection<>) ||
					interfaceDefinition == typeof(IReadOnlyList<>) || interfaceDefinition == typeof(IReadOnlyCollection<>)) {

					return () => Activator.CreateInstance(typeof(List<>).MakeGenericType(objectType.GetGenericArguments().Single()));
				}
				else if (interfaceDefinition == typeof(IDictionary<,>) || interfaceDefinition == typeof(IReadOnlyDictionary<,>))
					return () => Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(objectType.GetGenericArguments()));
				else if (interfaceDefinition == typeof(ISet<>)
#if NET5_0_OR_GREATER
					|| interfaceDefinition == typeof(IReadOnlySet<>)
#endif
					) {

					return () => Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(objectType.GetGenericArguments().Single()));
				}
			}

			if (typeCreationErrorsCache.TryGetValue(objectType, out var error)) {
				if (error == null)
					return () => Activator.CreateInstance(objectType);
				else
					throw new ObjectCreationException(objectType, new Exception(error));
			}
			else {
				// Try creating an instance
				try {
					Activator.CreateInstance(objectType);
					typeCreationErrorsCache.Add(objectType, null);
					return () => Activator.CreateInstance(objectType);
				}
				catch (Exception e) {
					typeCreationErrorsCache.Add(objectType, e.Message);
					throw new ObjectCreationException(objectType, e);
				}
			}
		}

		public static object Create(Type objectType) {
			return CreateFactory(objectType).Invoke();
		}

		public static bool CanCreate(Type objectType) {
			if (typeCreationErrorsCache.TryGetValue(objectType, out var error))
				return error == null;
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

		static string CreateStringFactory() {
			return string.Empty;
		}
	}
}
