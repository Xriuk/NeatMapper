using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	internal static class TypeUtils {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool HasInterface(Type type, Type interfaceType) {
			return (type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType) ||
				type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Type GetInterfaceElementType(Type collection, Type interfaceType) {
			return (collection.IsGenericType && collection.GetGenericTypeDefinition() == interfaceType) ?
				collection.GetGenericArguments()[0] :
				collection.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType).GetGenericArguments()[0];
		}

		internal static Type GetArrayElementType(Type arrayType) {
			var rank = arrayType.GetArrayRank();
			if(rank == 1)
				return arrayType.GetElementType();
			else
				return arrayType.GetElementType().MakeArrayType(rank - 1);
		}
	}
}
