using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	internal static class TypeUtils {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool HasInterface(Type type, Type openInterfaceType) {
			return (type.IsGenericType && type.GetGenericTypeDefinition() == openInterfaceType) ||
				type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterfaceType);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Type GetInterfaceElementType(Type collection, Type openInterfaceType) {
			return (collection.IsGenericType && collection.GetGenericTypeDefinition() == openInterfaceType) ?
				collection.GetGenericArguments()[0] :
				collection.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterfaceType).GetGenericArguments()[0];
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
