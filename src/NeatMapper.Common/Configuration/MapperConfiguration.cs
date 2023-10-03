#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NeatMapper.Configuration {
	internal sealed class MapperConfiguration : IMapperConfiguration {
		public MapperConfiguration(Func<Type, bool> newMapTypeFilter, Func<Type, bool> mergeMapTypeFilter, MapperConfigurationOptions options) { 
			var newMaps = new Dictionary<(Type From, Type To), Map>();
			var mergeMaps = new Dictionary<(Type From, Type To), Map>();
			var genericNewMaps = new List<GenericMap>();
			var genericMergeMaps = new List<GenericMap>();

			PopulateTypes((t, i) => newMapTypeFilter.Invoke(i) || mergeMapTypeFilter.Invoke(i),
				i => newMapTypeFilter.Invoke(i) ? genericNewMaps : genericMergeMaps,
				i => newMapTypeFilter.Invoke(i) ? newMaps : mergeMaps);

			NewMaps = newMaps;
			MergeMaps = mergeMaps;
			GenericNewMaps = genericNewMaps;
			GenericMergeMaps = genericMergeMaps;


			var matchers = new Dictionary<(Type From, Type To), Map>();
			var hierarchyMatchers = new Dictionary<(Type From, Type To), Map>();
			var genericMatchers = new List<GenericMap>();
			var genericHierarchyMatchers = new List<GenericMap>();

			PopulateTypes((t, i) => (i == typeof(IMatchMap<,>)
#if NET7_0_OR_GREATER
				|| i == typeof(IMatchMapStatic<,>)
#endif
				) || ((i == typeof(IHierarchyMatchMap<,>)
#if NET7_0_OR_GREATER
				|| i == typeof(IHierarchyMatchMapStatic<,>)
#endif
			) && !t.ContainsGenericParameters),

			i => (i == typeof(IMatchMap<,>)
#if NET7_0_OR_GREATER
				|| i == typeof(IMatchMapStatic<,>)
#endif			
			) ? genericMatchers : genericHierarchyMatchers,

			i => (i == typeof(IMatchMap<,>)
#if NET7_0_OR_GREATER
				|| i == typeof(IMatchMapStatic<,>)
#endif		
			) ? matchers : hierarchyMatchers);

			Matchers = matchers;
			HierarchyMatchers = hierarchyMatchers;
			GenericMatchers = genericMatchers;


			MergeMapsCollectionsOptions = new MergeMapsCollectionsOptions(options.MergeMapsCollectionsOptions);


			void PopulateTypes(Func<Type, Type, bool> interfaceFilter, // Type, Interface GetGenericTypeDefinition
				Func<Type, List<GenericMap>> genericMapsSelector, // Interface GetGenericTypeDefinition
				Func<Type, Dictionary<(Type From, Type To), Map>> mapsSelector // Interface GetGenericTypeDefinition
				) { 

				foreach (var type in options.ScanTypes
					.Distinct()
					.Where(t => t.IsClass && !t.IsAbstract && (t.DeclaringType == null || !t.DeclaringType.IsGenericType) && t.GetInterfaces().Any(i =>
						i.IsGenericType && interfaceFilter.Invoke(t, i.GetGenericTypeDefinition())))) {

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
								var map = genericMapsSelector.Invoke(interf.GetGenericTypeDefinition());
								var duplicate = map.FirstOrDefault(m => MatchOpenGenericArgumentsRecursive(m.From, interfaceArguments[0]) && MatchOpenGenericArgumentsRecursive(m.To, interfaceArguments[1]));
								if (duplicate != null)
									throw new InvalidOperationException($"Duplicate interface {interf.FullName ?? interf.Name} in generic class {type.Name}, an interface with matching parameters is already defined in class {duplicate.Class.Name}. If the class has generic constraints check that they do not overlap");
								var method = type.GetInterfaceMap(interf).TargetMethods.First();
								if (!method.IsStatic && type.GetConstructor(Type.EmptyTypes) == null)
									throw new InvalidOperationException($"Interface {interf.FullName ?? interf.Name} in generic class {type.Name} cannot be instantiated because the class which implements the non-static interface has no parameterless constructor. Either add a parameterless constructor to the class or implement the static interface (available in .NET 7)");

								map.Add(new GenericMap {
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
							if(!method.IsStatic && type.GetConstructor(Type.EmptyTypes) == null)
								throw new InvalidOperationException($"Interface {interf.FullName ?? interf.Name} in class {type.Name} cannot be instantiated because the class which implements the non-static interface has no parameterless constructor. Either add a parameterless constructor to the class or implement the static interface (available in .NET 7)");
							
							var dictionary = mapsSelector.Invoke(interf.GetGenericTypeDefinition());
							if (dictionary.ContainsKey((arguments[0], arguments[1])))
								throw new InvalidOperationException($"Duplicate interface {interf.FullName ?? interf.Name} in class {type.Name}, an interface with matching parameters is already defined in class {mapsSelector.Invoke(interf.GetGenericTypeDefinition())[(arguments[0], arguments[1])].Class.Name}");
							else { 
								dictionary.Add((arguments[0], arguments[1]), new Map {
									Class = type,
									Method = method
								});
							}
						}
					}
				}
			}
		}


		public IReadOnlyDictionary<(Type From, Type To), Map> NewMaps { get; }

		public IReadOnlyDictionary<(Type From, Type To), Map> MergeMaps { get; }

		public IEnumerable<GenericMap> GenericNewMaps { get; }

		public IEnumerable<GenericMap> GenericMergeMaps { get; }

		public IReadOnlyDictionary<(Type From, Type To), Map> Matchers { get; }

		public IReadOnlyDictionary<(Type From, Type To), Map> HierarchyMatchers { get; }

		public IEnumerable<GenericMap> GenericMatchers { get; }

		public MergeMapsCollectionsOptions MergeMapsCollectionsOptions { get; }


		private static IEnumerable<Type> GetOpenGenericArgumentsRecursive(Type t) {
			if(IsGenericTypeParameter(t))
				return new[] { t };
			if(!t.IsGenericType)
				return Enumerable.Empty<Type>();

			var arguments = t.GetGenericArguments();
			return arguments
				.Where(a => IsGenericTypeParameter(a))
				.Concat(arguments.SelectMany(GetOpenGenericArgumentsRecursive));
		}

		private static bool MatchOpenGenericArgumentsRecursive(Type t1, Type t2) {
			if(!t1.IsGenericType || !t2.IsGenericType){
				if(IsGenericTypeParameter(t1) && IsGenericTypeParameter(t2)) {
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
					if ((!t1Constraints.Any() && t1Attributes == GenericParameterAttributes.None) ||
						(!t2Constraints.Any() && t2Attributes == GenericParameterAttributes.None) ||
						t1Constraints.Any(t => IsGenericTypeParameter(t)) || t2Constraints.Any(t => IsGenericTypeParameter(t))){ 

						return true;
					}
					if(t1Constraints.Length != t2Constraints.Length)
						return false;

					return !t1Constraints.Where(t1c => !t2Constraints.Any(t2c => t2c.IsAssignableFrom(t1c))).Any() ||
						!t2Constraints.Where(t2c => !t1Constraints.Any(t1c => t1c.IsAssignableFrom(t2c))).Any();
				}
				else if(t1.IsArray && t2.IsArray)
					return MatchOpenGenericArgumentsRecursive(t1.GetElementType(), t2.GetElementType());
				else
					return t1 == t2;
			}
			else if (t1.GetGenericTypeDefinition() != t2.GetGenericTypeDefinition())
				return false;

			var arguments1 = t1.GetGenericArguments();
			var arguments2 = t2.GetGenericArguments();
			if(arguments1.Length != arguments2.Length)
				return false;

			return arguments1.Zip(arguments2, (a1, a2) => (First: a1, Second: a2)).All((a) => MatchOpenGenericArgumentsRecursive(a.First, a.Second));
		}

		private static bool IsGenericTypeParameter(Type t) {
			return t.IsGenericParameter && t.DeclaringMethod == null;
		}
	}
}
