using System.Reflection;

namespace NeatMapper.Core.Configuration {
	internal sealed class MapperConfiguration : IMapperConfiguration {
		public MapperConfiguration(MapperConfigurationOptions options) {
			var newMaps = new Dictionary<(Type From, Type To), MethodInfo>();
			var mergeMaps = new Dictionary<(Type From, Type To), MethodInfo>();
			var genericNewMaps = new List<GenericMap>();
			var genericMergeMaps = new List<GenericMap>();

			PopulateTypes(i => i == typeof(INewMap<,>) || i == typeof(IMergeMap<,>),
				i => i == typeof(INewMap<,>) ? genericNewMaps : genericMergeMaps,
				i => i == typeof(INewMap<,>) ? newMaps : mergeMaps);

			var asyncNewMaps = new Dictionary<(Type From, Type To), MethodInfo>();
			var asyncMergeMaps = new Dictionary<(Type From, Type To), MethodInfo>();
			var asyncGenericNewMaps = new List<GenericMap>();
			var asyncGenericMergeMaps = new List<GenericMap>();

			PopulateTypes(i => i == typeof(IAsyncNewMap<,>) || i == typeof(IAsyncMergeMap<,>),
				i => i == typeof(IAsyncNewMap<,>) ? asyncGenericNewMaps : asyncGenericMergeMaps,
				i => i == typeof(IAsyncNewMap<,>) ? asyncNewMaps : asyncMergeMaps);

			NewMaps = newMaps;
			MergeMaps = mergeMaps;
			AsyncNewMaps = asyncNewMaps;
			AsyncMergeMaps = asyncMergeMaps;
			GenericNewMaps = genericNewMaps;
			GenericMergeMaps = genericMergeMaps;
			AsyncGenericNewMaps = asyncGenericNewMaps;
			AsyncGenericMergeMaps = asyncGenericMergeMaps;


			var collectionElementComparers = new Dictionary<(Type From, Type To), MethodInfo>();
			var genericCollectionElementComparers = new List<GenericMap>();

			PopulateTypes(i => i == typeof(ICollectionElementComparer<,>),
				_ => genericCollectionElementComparers,
				_ => collectionElementComparers);

			CollectionElementComparers = collectionElementComparers;
			GenericCollectionElementComparers = genericCollectionElementComparers;


			MergeMapsCollectionsOptions = new MergeMapsCollectionsOptions(options.MergeMapsCollectionsOptions);


			void PopulateTypes(Func<Type, bool> interfaceFilter, // GetGenericTypeDefinition
				Func<Type, List<GenericMap>> genericMapsSelector, // GetGenericTypeDefinition
				Func<Type, Dictionary<(Type From, Type To), MethodInfo>> mapsSelector // GetGenericTypeDefinition
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
								throw new InvalidOperationException($"Interface {interf.FullName} in generic class {type.Name} cannot be instantiated because the generic arguments of the interface do not fully cover the generic arguments of the class so they cannot be inferred");
							else {
								var map = genericMapsSelector.Invoke(interf.GetGenericTypeDefinition());
								var duplicate = map.FirstOrDefault(m => MatchOpenGenericArgumentsRecursive(m.From, interfaceArguments[0]) && MatchOpenGenericArgumentsRecursive(m.To, interfaceArguments[1]));
								if (duplicate != null)
									throw new InvalidOperationException($"Duplicate interface {interf.FullName} in generic class {type.Name}, an interface with matching parameters is already defined in class {duplicate.Class.Name}");

								map.Add(new GenericMap {
									From = interfaceArguments[0],
									To = interfaceArguments[1],
									Class = type,
									Method = type.GetInterfaceMap(interf).TargetMethods.First().MethodHandle
								});
							}
						}
					}
					else {
						foreach (var interf in interfaces) {
							var arguments = interf.GetGenericArguments();
							mapsSelector.Invoke(interf.GetGenericTypeDefinition())
								.Add((arguments[0], arguments[1]), type.GetInterfaceMap(interf).TargetMethods.First());
						}
					}
				}
			}
		}


		public IReadOnlyDictionary<(Type From, Type To), MethodInfo> NewMaps { get; }

		public IReadOnlyDictionary<(Type From, Type To), MethodInfo> MergeMaps { get; }

		public IReadOnlyDictionary<(Type From, Type To), MethodInfo> AsyncNewMaps { get; }

		public IReadOnlyDictionary<(Type From, Type To), MethodInfo> AsyncMergeMaps { get; }

		public IEnumerable<GenericMap> GenericNewMaps { get; }

		public IEnumerable<GenericMap> GenericMergeMaps { get; }

		public IEnumerable<GenericMap> AsyncGenericNewMaps { get; }

		public IEnumerable<GenericMap> AsyncGenericMergeMaps { get; }

		public IReadOnlyDictionary<(Type From, Type To), MethodInfo> CollectionElementComparers { get; }

		public IEnumerable<GenericMap> GenericCollectionElementComparers { get; }

		public MergeMapsCollectionsOptions MergeMapsCollectionsOptions { get; }


		private static IEnumerable<Type> GetOpenGenericArgumentsRecursive(Type t) {
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
					// Check if generic constraints overlap
					if(t1.GenericParameterAttributes != t2.GenericParameterAttributes)
						return false;

					if ((t1.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) ||
						t1.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint)) &&
						t1.GetCustomAttributes().Any(a => a.GetType().Name == "IsUnmanagedAttribute") != t2.GetCustomAttributes().Any(a => a.GetType().Name == "IsUnmanagedAttribute")) {

						return false;
					}

					var t1Constraints = t1.GetGenericParameterConstraints();
					var t2Constraints = t2.GetGenericParameterConstraints();
					if(t1Constraints.Length != t2Constraints.Length)
						return false;

					return !t1Constraints.Except(t2Constraints).Any() && !t2Constraints.Except(t1Constraints).Any();
				}
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
