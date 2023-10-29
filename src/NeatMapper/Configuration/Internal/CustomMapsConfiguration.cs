#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	/// <summary>
	/// Contains informations about custom defined maps (both in classes and outside, both specific and generic)
	/// </summary>
	internal sealed class CustomMapsConfiguration {
		static readonly IDictionary<Type, object> nonStaticMethodsInstances = new ConcurrentDictionary<Type, object>();
		readonly IDictionary<(Type From, Type To), Func<object[], object>> _genericCache = new ConcurrentDictionary<(Type From, Type To), Func<object[], object>>();

		/// <param name="interfaceFilter">
		/// Filter used to retrieve interface(s) for the maps, will receive the declaring type 
		/// and implemented interface type as parameters
		/// </param>
		/// <param name="options">Options to retrieve the maps</param>
		/// <param name="additionalMaps">Additional defined maps, if set will be added after the maps defined in classes</param>
		internal CustomMapsConfiguration(
			Func<Type, Type, bool> interfaceFilter,
			CustomMapsOptions options,
			IEnumerable<CustomAdditionalMap> additionalMaps = null) {

			if (interfaceFilter == null)
				throw new ArgumentNullException(nameof(interfaceFilter));
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			var maps = new Dictionary<(Type From, Type To), CustomMap>();
			var genericMaps = new List<CustomGenericMap>();

			foreach (var type in options.TypesToScan.Distinct().Where(t => t.IsClass && !t.IsAbstract &&
				(t.DeclaringType == null || !t.DeclaringType.IsGenericType) &&
				t.GetInterfaces().Any(i => interfaceFilter.Invoke(t, i)))) {

				var interfaces = type.GetInterfaces()
					.Where(i => i.IsGenericType && interfaceFilter.Invoke(type, i.GetGenericTypeDefinition()));

				if (type.IsGenericTypeDefinition) {
					var typeArguments = type.GetGenericArguments();

					foreach (var interf in interfaces) {
						var interfaceArguments = interf.GetGenericArguments();
						var interfaceOpenGenericArguments = interfaceArguments
							.SelectMany(GetOpenGenericArgumentsRecursive)
							.Distinct()
							.ToArray();
						if (!typeArguments.All(t => interfaceOpenGenericArguments.Contains(t)))
							throw new InvalidOperationException($"Interface {interf.FullName ?? interf.Name} in generic class {type.Name} cannot be instantiated because the generic arguments of the interface do not fully cover the generic arguments of the class so they cannot be inferred");
						else {
							var duplicate = genericMaps.FirstOrDefault(m => MatchOpenGenericsRecursive(m.From, interfaceArguments[0]) && MatchOpenGenericsRecursive(m.To, interfaceArguments[1]));
							if (duplicate != null)
								throw new InvalidOperationException($"Duplicate interface {interf.FullName ?? interf.Name} in generic class {type.Name}, an interface with matching parameters is already defined in class {duplicate.Class.Name}. If the class has generic constraints check that they do not overlap");
							var method = type.GetInterfaceMap(interf).TargetMethods.First();
							if (!method.IsStatic && type.GetConstructor(Type.EmptyTypes) == null)
								throw new InvalidOperationException($"Interface {interf.FullName ?? interf.Name} in generic class {type.Name} cannot be instantiated because the class which implements the non-static interface has no parameterless constructor. Either add a parameterless constructor to the class or implement the static interface (available in .NET 7)");

							genericMaps.Add(new CustomGenericMap {
								From = interfaceArguments[0],
								To = interfaceArguments[1],
								Class = type,
								Method = method.MethodHandle
							});
						}
					}
				}
				else {
					foreach (var interf in interfaces) {
						var arguments = interf.GetGenericArguments();
						var method = type.GetInterfaceMap(interf).TargetMethods.First();
						if (!method.IsStatic && type.GetConstructor(Type.EmptyTypes) == null)
							throw new InvalidOperationException($"Interface {interf.FullName ?? interf.Name} in class {type.Name} cannot be instantiated because the class which implements the non-static interface has no parameterless constructor. Either add a parameterless constructor to the class or implement the static interface (available in .NET 7)");

						var types = (From: arguments[0], To: arguments[1]);
						if (maps.ContainsKey(types))
							throw new InvalidOperationException($"Duplicate interface {interf.FullName ?? interf.Name} in class {type.Name}, an interface with matching parameters is already defined in class {maps[types].Method.DeclaringType.Name}");
						else
							maps.Add(types, new CustomMap {
								From = types.From,
								To = types.To,
								Method = method
							});
					}
				}
			}

			if (additionalMaps != null) {
				foreach (var map in additionalMaps) {
					if (map.From.IsGenericType && !map.From.IsConstructedGenericType ||
						map.To.IsGenericType && !map.To.IsConstructedGenericType ||
						map.Method.IsGenericMethod ||
						map.Method.DeclaringType != null && map.Method.DeclaringType.IsGenericType && !map.Method.DeclaringType.IsConstructedGenericType) {

						throw new InvalidOperationException("Additional map methods cannot be generic or specified in an open generic class");
					}

					// DEV: maybe validate Method against From and To types?

					if (!map.Method.IsStatic && map.Method.DeclaringType.GetConstructor(Type.EmptyTypes) == null && map.Instance == null)
						throw new InvalidOperationException($"Map {map.Method.Name} in class {map.Method.DeclaringType.FullName ?? map.Method.DeclaringType.Name} cannot be instantiated because the class has no parameterless constructor. Either add a parameterless constructor to the class or provide an instance or move the map method to another class");

					var types = (map.From, map.To);
					if (maps.ContainsKey(types)) {
						if (map.ThrowOnDuplicate)
							throw new InvalidOperationException($"Duplicate map {map.Method.Name} in class {map.Method.DeclaringType.FullName ?? map.Method.DeclaringType.Name}, an map with matching parameters is already defined in class {maps[types].Method.DeclaringType.FullName ?? maps[types].Method.DeclaringType.Name}");
					}
					else
						maps.Add(types, map);
				}
			}

			Maps = maps;
			GenericMaps = genericMaps;
		}


		internal IReadOnlyDictionary<(Type From, Type To), CustomMap> Maps { get; }

		internal IEnumerable<CustomGenericMap> GenericMaps { get; }

		// (...params) => result
		internal Func<object[], object> GetMap((Type From, Type To) types) {
			// Try retrieving a regular map
			// or try matching to a generic one
			lock (Maps) { 
				if (Maps.TryGetValue(types, out var map)) {
					return (parameters) => {
						try {
							return map.Method.Invoke(map.Method.IsStatic ? null : map.Instance ?? CreateOrReturnInstance(map.Method.DeclaringType), parameters);
						}
						catch (Exception e) {
							throw new MappingException(e, types);
						}
					};
				}
			}

			// Try retrieving from cache
			lock (_genericCache) { 
				if (_genericCache.TryGetValue(types, out var method))
					return method;
			}

			lock (GenericMaps) { 
				foreach (var map in GenericMaps) {
					// Check if the two types are compatible (we'll check constraints when instantiating)
					if (!MatchOpenAndClosedGenericsRecursive(map.From, types.From) ||
						!MatchOpenAndClosedGenericsRecursive(map.To, types.To)) {

						continue;
					}

					// Try inferring the types
					var classArguments = InferOpenGenericArgumentsRecursive(map.From, types.From)
						.Concat(InferOpenGenericArgumentsRecursive(map.To, types.To))
						.ToArray();

					// We may have different closed types matching for the same open type argument, in this case the map should not match
					var genericArguments = map.Class.GetGenericArguments().Length;
					if (classArguments
#if NET6_0_OR_GREATER
						.DistinctBy(a => a.OpenGenericArgument)
#else
						.GroupBy(a => a.OpenGenericArgument)
						.Select(a => a.First())
#endif
						.Count() != genericArguments ||
						classArguments.Distinct().Count() != genericArguments) {

						continue;
					}

					// Check unmanaged constraints because the CLR seems to not enforce it
					if (classArguments.Any(a => a.OpenGenericArgument.GetCustomAttributes().Any(ca => ca.GetType().Name == "IsUnmanagedAttribute") &&
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
					if (mapMethod == null)
						continue;

#pragma warning disable IDE0039 // Use local function
					Func<object[], object> func = (parameters) => {
						try {
							return mapMethod.Invoke(mapMethod.IsStatic ? null : CreateOrReturnInstance(concreteType), parameters);
						}
						catch (Exception e) {
							throw new MappingException(e, types);
						}
					};
#pragma warning restore IDE0039 // Use local function

					// Cache the method
					lock (_genericCache) { 
						_genericCache.Add(types, func);
					}

					return func;
				}
			}

			throw new MapNotFoundException(types);
		}


		static IEnumerable<Type> GetOpenGenericArgumentsRecursive(Type t) {
			if (IsGenericTypeParameter(t))
				return new[] { t };
			if (!t.IsGenericType)
				return Enumerable.Empty<Type>();

			var arguments = t.GetGenericArguments();
			return arguments
				.Where(a => IsGenericTypeParameter(a))
				.Concat(arguments.SelectMany(GetOpenGenericArgumentsRecursive));
		}

		// Matches two open generic types, checks constraints too
		static bool MatchOpenGenericsRecursive(Type t1, Type t2) {
			if (!t1.IsGenericType || !t2.IsGenericType) {
				if (IsGenericTypeParameter(t1) && IsGenericTypeParameter(t2)) {
					// new() and unmanaged are ignored
					var t1Attributes = t1.GenericParameterAttributes & ~GenericParameterAttributes.DefaultConstructorConstraint;
					var t2Attributes = t2.GenericParameterAttributes & ~GenericParameterAttributes.DefaultConstructorConstraint;

					// Check if generic constraints overlap, if one class has no attributes it can match the other
					if (t1Attributes != t2Attributes &&
						// If one of them has no constrains
						t1Attributes != GenericParameterAttributes.None &&
						t2Attributes != GenericParameterAttributes.None &&
						// If both of them are structs/unmanaged
						(!t1Attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) ||
						!t2Attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
						) {

						return false;
					}

					var t1Constraints = t1.GetGenericParameterConstraints();
					var t2Constraints = t2.GetGenericParameterConstraints();
					if (!t1Constraints.Any() && t1Attributes == GenericParameterAttributes.None ||
						!t2Constraints.Any() && t2Attributes == GenericParameterAttributes.None ||
						t1Constraints.Any(t => IsGenericTypeParameter(t)) || t2Constraints.Any(t => IsGenericTypeParameter(t))) {

						return true;
					}
					if (t1Constraints.Length != t2Constraints.Length)
						return false;

					return !t1Constraints.Where(t1c => !t2Constraints.Any(t2c => t2c.IsAssignableFrom(t1c))).Any() ||
						!t2Constraints.Where(t2c => !t1Constraints.Any(t1c => t1c.IsAssignableFrom(t2c))).Any();
				}
				else if (t1.IsArray && t2.IsArray)
					return MatchOpenGenericsRecursive(t1.GetElementType(), t2.GetElementType());
				else
					return t1 == t2;
			}
			else if (t1.GetGenericTypeDefinition() != t2.GetGenericTypeDefinition())
				return false;

			var arguments1 = t1.GetGenericArguments();
			var arguments2 = t2.GetGenericArguments();
			if (arguments1.Length != arguments2.Length)
				return false;

			return arguments1.Zip(arguments2, (a1, a2) => (First: a1, Second: a2)).All((a) => MatchOpenGenericsRecursive(a.First, a.Second));
		}

		internal static object CreateOrReturnInstance(Type classType) {
			if (!nonStaticMethodsInstances.TryGetValue(classType, out var instance)) {
				try {
					instance = ObjectFactory.Create(classType);
					nonStaticMethodsInstances.Add(classType, instance);
				}
				catch (Exception e) {
					throw new InvalidOperationException($"Could not create instance of type {classType.FullName ?? classType.Name} for non static interface", e);
				}
			}

			return instance;
		}

		#region Types methods
#if NET47 || NET48
		static readonly IDictionary<Type, bool> isUnmanagedCache = new ConcurrentDictionary<Type, bool>();
#else
        static readonly MethodInfo RuntimeHelpers_IsReferenceOrContainsReference =
            typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences))!
                ?? throw new InvalidOperationException("Could not find RuntimeHelpers.IsReferenceOrContainsReferences");
#endif
		static bool IsUnmanaged(Type type) {
#if NET47 || NET48
			// https://stackoverflow.com/a/53969223/2672235

			// check if we already know the answer
			if (!isUnmanagedCache.TryGetValue(type, out var answer)) {

				if (!type.IsValueType) {
					// not a struct -> false
					answer = false;
				}
				else if (type.IsPrimitive || type.IsPointer || type.IsEnum) {
					// primitive, pointer or enum -> true
					answer = true;
				}
				else {
					// otherwise check recursively
					answer = type
						.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
						.All(f => IsUnmanaged(f.FieldType));
				}

				isUnmanagedCache[type] = answer;
			}

			return answer;
#else
            return !(bool)RuntimeHelpers_IsReferenceOrContainsReference.MakeGenericMethod(type).Invoke(null, null);
#endif
		}

		// Checks an open type is compatible with a closed type, does not test any constraints
		static bool MatchOpenAndClosedGenericsRecursive(Type openType, Type closedType) {
			if (!openType.IsGenericType) {
				if (openType.IsArray)
					return closedType.IsArray && MatchOpenAndClosedGenericsRecursive(openType.GetElementType(), closedType.GetElementType());
				else
					return IsGenericTypeParameter(openType) || openType == closedType;
			}
			else if (!closedType.IsGenericType)
				return false;
			else if (openType.GetGenericTypeDefinition() != closedType.GetGenericTypeDefinition())
				return false;

			var openTypeArguments = openType.GetGenericArguments();
			var closedTypeArguments = closedType.GetGenericArguments();
			if (openTypeArguments.Length != closedTypeArguments.Length)
				return false;

			var arguments = openTypeArguments.Zip(closedTypeArguments, (o, c) => (OpenTypeArgument: o, ClosedTypeArgument: c));
			return arguments.All((a) => MatchOpenAndClosedGenericsRecursive(a.OpenTypeArgument, a.ClosedTypeArgument));
		}

		static IEnumerable<(Type OpenGenericArgument, Type ClosedType)> InferOpenGenericArgumentsRecursive(Type openType, Type closedType) {
			if (!openType.IsGenericType) {
				if (IsGenericTypeParameter(openType))
					return new[] { (openType, closedType) };
				else if (openType.IsArray)
					return InferOpenGenericArgumentsRecursive(openType.GetElementType(), closedType.GetElementType());
				else
					return Enumerable.Empty<(Type, Type)>();
			}
			else
				return openType.GetGenericArguments()
					.Zip(closedType.GetGenericArguments(), (o, c) => (First: o, Second: c))
					.SelectMany((a) => InferOpenGenericArgumentsRecursive(a.First, a.Second));
		}

		static bool IsGenericTypeParameter(Type t) {
			return t.IsGenericParameter && t.DeclaringMethod == null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static Type MakeGenericTypeWithInferredArguments(Type openType, IEnumerable<(Type OpenGenericArgument, Type ClosedType)> arguments) {
			return openType.MakeGenericType(openType.GetGenericArguments().Select(oa => arguments.First(a => a.OpenGenericArgument == oa).ClosedType).ToArray());
		}
		#endregion
	}
}
