using System.Reflection;

namespace NeatMapper.Configuration {
	internal sealed class MapperConfiguration : IMapperConfiguration {
		public MapperConfiguration(Func<Type, bool> newMapTypeFilter, Func<Type, bool> mergeMapTypeFilter, MapperConfigurationOptions options) { 
			/*if (!newMapOpenType.IsGenericType || newMapOpenType.GetGenericTypeDefinition() != newMapOpenType)
				throw new ArgumentException("Invalid new map open type", nameof(newMapOpenType));
			if (!mergeMapOpenType.IsGenericType || mergeMapOpenType.GetGenericTypeDefinition() != mergeMapOpenType) 
				throw new ArgumentException("Invalid merge map open type", nameof(mergeMapOpenType));*/

			var newMaps = new Dictionary<(Type From, Type To), Map>();
			var mergeMaps = new Dictionary<(Type From, Type To), Map>();
			var genericNewMaps = new List<GenericMap>();
			var genericMergeMaps = new List<GenericMap>();

			PopulateTypes(i => newMapTypeFilter.Invoke(i) || mergeMapTypeFilter.Invoke(i),
				i => newMapTypeFilter.Invoke(i) ? genericNewMaps : genericMergeMaps,
				i => newMapTypeFilter.Invoke(i) ? newMaps : mergeMaps);

			NewMaps = newMaps;
			MergeMaps = mergeMaps;
			GenericNewMaps = genericNewMaps;
			GenericMergeMaps = genericMergeMaps;


			var collectionElementComparers = new Dictionary<(Type From, Type To), Map>();
			var genericCollectionElementComparers = new List<GenericMap>();

			PopulateTypes(i => i == typeof(IMatchMap<,>)
#if NET7_0_OR_GREATER
			|| i == typeof(IMatchMapStatic<,>)
#endif
			,
				_ => genericCollectionElementComparers,
				_ => collectionElementComparers);

			Matchers = collectionElementComparers;
			GenericMatchers = genericCollectionElementComparers;


			MergeMapsCollectionsOptions = new MergeMapsCollectionsOptions(options.MergeMapsCollectionsOptions);


			void PopulateTypes(Func<Type, bool> interfaceFilter, // GetGenericTypeDefinition
				Func<Type, List<GenericMap>> genericMapsSelector, // GetGenericTypeDefinition
				Func<Type, Dictionary<(Type From, Type To), Map>> mapsSelector // GetGenericTypeDefinition
				) { 

				foreach (var type in options.ScanTypes
					.Distinct()
					.Where(t => t.IsClass && t.GetInterfaces().Any(i =>
						i.IsGenericType && interfaceFilter.Invoke(i.GetGenericTypeDefinition())))) {

					var interfaces = type.GetInterfaces()
						.Where(i => i.IsGenericType && interfaceFilter.Invoke(i.GetGenericTypeDefinition()));

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
									throw new InvalidOperationException($"Duplicate interface {interf.FullName ?? interf.Name} in generic class {type.Name}, an interface with matching parameters is already defined in class {duplicate.Class.Name}. If the class has generic constraints check that they do not overlap eachother");
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
							
							if(!mapsSelector.Invoke(interf.GetGenericTypeDefinition())
								.TryAdd((arguments[0], arguments[1]), new Map {
									Class = type,
									Method = method
								})) {

								throw new InvalidOperationException($"Duplicate interface {interf.FullName ?? interf.Name} in class {type.Name}, an interface with matching parameters is already defined in class {mapsSelector.Invoke(interf.GetGenericTypeDefinition())[(arguments[0], arguments[1])].Class.Name}");
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

		public IEnumerable<GenericMap> GenericMatchers { get; }

		public MergeMapsCollectionsOptions MergeMapsCollectionsOptions { get; }


		private static IEnumerable<Type> GetOpenGenericArgumentsRecursive(Type t) {
			if(t.IsGenericTypeParameter)
				return new[] { t };
			if(!t.IsGenericType)
				return Enumerable.Empty<Type>();

			var arguments = t.GetGenericArguments();
			return arguments
				.Where(a => a.IsGenericTypeParameter)
				.Concat(arguments.SelectMany(GetOpenGenericArgumentsRecursive));
		}

		private static bool MatchOpenGenericArgumentsRecursive(Type t1, Type t2) {
			if(!t1.IsGenericType || !t2.IsGenericType){
				if(t1.IsGenericTypeParameter && t2.IsGenericTypeParameter) {
					// Check if generic constraints overlap, if one class has no attributes it can match the other
					if (t1.GenericParameterAttributes != t2.GenericParameterAttributes &&
						// If one of them has no constrains
						t1.GenericParameterAttributes != GenericParameterAttributes.None &&
						t2.GenericParameterAttributes != GenericParameterAttributes.None &&
						// If one of them is new()
						!t1.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) &&
						!t2.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) &&
						// If both of them are structs/unmanaged
						(!t1.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) ||
						!t2.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
						) { 

						return false;
					}

					// Unmanaged does not count for two types to overlap
					/*if ((t1.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) ||
						t1.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint)) &&
						t1.GetCustomAttributes().Any(a => a.GetType().Name == "IsUnmanagedAttribute") != t2.GetCustomAttributes().Any(a => a.GetType().Name == "IsUnmanagedAttribute")) {

						return false;
					}*/

					var t1Constraints = t1.GetGenericParameterConstraints().Where(c => c != typeof(ValueType));
					var t2Constraints = t2.GetGenericParameterConstraints().Where(c => c != typeof(ValueType));
					if(t1Constraints.Count() != t2Constraints.Count())
						return false;

					return !t1Constraints.Except(t2Constraints).Any() && !t2Constraints.Except(t1Constraints).Any();
				}
				else if(t1.IsArray && t2.IsArray)
					return MatchOpenGenericArgumentsRecursive(t1.GetElementType()!, t2.GetElementType()!);
				else
					return t1 == t2;
			}
			else if (t1.GetGenericTypeDefinition() != t2.GetGenericTypeDefinition())
				return false;

			var arguments1 = t1.GetGenericArguments();
			var arguments2 = t2.GetGenericArguments();
			if(arguments1.Length != arguments2.Length)
				return false;

			return arguments1.Zip(arguments2).All((a) => MatchOpenGenericArgumentsRecursive(a.First, a.Second));
		}
	}
}
