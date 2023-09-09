﻿using System.Reflection;

namespace NeatMapper.Core.Configuration {
	internal sealed class MapperConfiguration : IMapperConfiguration {
		public MapperConfiguration(MapperConfigurationOptions options) {
			var newMaps = new Dictionary<(Type From, Type To), MethodInfo>();
			var mergeMaps = new Dictionary<(Type From, Type To), MethodInfo>();
			var genericNewMaps = new List<GenericMap>();
			var genericMergeMaps = new List<GenericMap>();

			PopulateTypes(typeof(INewMap<,>), typeof(IMergeMap<,>),
				newMaps, mergeMaps, genericNewMaps, genericMergeMaps);

			var asyncNewMaps = new Dictionary<(Type From, Type To), MethodInfo>();
			var asyncMergeMaps = new Dictionary<(Type From, Type To), MethodInfo>();
			var asyncGenericNewMaps = new List<GenericMap>();
			var asyncGenericMergeMaps = new List<GenericMap>();

			PopulateTypes(typeof(IAsyncNewMap<,>), typeof(IAsyncMergeMap<,>),
				asyncNewMaps, asyncMergeMaps, asyncGenericNewMaps, asyncGenericMergeMaps);

			NewMaps = newMaps;
			MergeMaps = mergeMaps;
			AsyncNewMaps = asyncNewMaps;
			AsyncMergeMaps = asyncMergeMaps;
			GenericNewMaps = genericNewMaps;
			GenericMergeMaps = genericMergeMaps;
			AsyncGenericNewMaps = asyncGenericNewMaps;
			AsyncGenericMergeMaps = asyncGenericMergeMaps;

			void PopulateTypes(Type newMapType, Type mergeMapType,
				Dictionary<(Type From, Type To), MethodInfo> newMaps, Dictionary<(Type From, Type To), MethodInfo> mergeMaps,
				List<GenericMap> genericNewMaps, List<GenericMap> genericMergeMaps) { 

				foreach (var type in options.MapTypes
					.Distinct()
					.Where(t => t.IsClass && t.GetInterfaces().Any(i =>
						i.IsGenericType &&
						(i.GetGenericTypeDefinition() == newMapType || i.GetGenericTypeDefinition() == mergeMapType)))) {

					var interfaces = type.GetInterfaces()
						.Where(i => i.IsGenericType &&
							(i.GetGenericTypeDefinition() == newMapType || i.GetGenericTypeDefinition() == mergeMapType));

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
								var map = (interf.GetGenericTypeDefinition() == newMapType ? genericNewMaps : genericMergeMaps);
								var duplicate = map.FirstOrDefault(m => MatchOpenGenericArgumentsRecursive(m.From, interfaceArguments[0]) && MatchOpenGenericArgumentsRecursive(m.To, interfaceArguments[1]));
								if (duplicate != null)
									throw new InvalidOperationException($"Duplicate interface {interf.FullName} in generic class {type.Name}, an interface with matching parameters is already defined in class {duplicate.Class}");

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
							(interf.GetGenericTypeDefinition() == newMapType ? newMaps : mergeMaps)
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


		private static IEnumerable<Type> GetOpenGenericArgumentsRecursive(Type t) {
			if(!t.IsGenericType)
				return Enumerable.Empty<Type>();

			var arguments = t.GetGenericArguments();
			return arguments
				.Where(a => a.IsGenericTypeParameter)
				.Concat(arguments.SelectMany(GetOpenGenericArgumentsRecursive));
		}

		internal static bool MatchOpenGenericArgumentsRecursive(Type t1, Type t2) {
			if(!t1.IsGenericType || !t2.IsGenericType){
				if(t1.IsGenericTypeParameter && t2.IsGenericTypeParameter) {
					// DEV: check constraints

					return true;
				}
				else
					return t1 == t2;
			}

			var arguments1 = t1.GetGenericArguments();
			var arguments2 = t2.GetGenericArguments();
			if(arguments1.Length != arguments2.Length)
				return false;

			return arguments1.Zip(arguments2).All((a) => MatchOpenGenericArgumentsRecursive(a.First, a.Second));
		}
	}
}
