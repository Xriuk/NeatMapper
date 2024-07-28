#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	internal static class TypeUtils {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasInterface(this Type type, Type openInterfaceType) {
			return (type.IsGenericType && type.GetGenericTypeDefinition() == openInterfaceType) ||
				type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterfaceType);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetInterfaceElementType(this Type collection, Type openInterfaceType) {
			return (collection.IsGenericType && collection.GetGenericTypeDefinition() == openInterfaceType) ?
				collection.GetGenericArguments()[0] :
				collection.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterfaceType).GetGenericArguments()[0];
		}

		public static Type GetArrayElementType(this Type arrayType) {
			var rank = arrayType.GetArrayRank();
			if(rank == 1)
				return arrayType.GetElementType();
			else
				return arrayType.GetElementType().MakeArrayType(rank - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEnumerable(this Type type) {
			return type.HasInterface(typeof(IEnumerable<>));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetEnumerableElementType(this Type type) {
			return type.GetInterfaceElementType(typeof(IEnumerable<>));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAsyncEnumerable(this Type type) {
			return type.HasInterface(typeof(IAsyncEnumerable<>));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetAsyncEnumerableElementType(this Type type) {
			return type.GetInterfaceElementType(typeof(IAsyncEnumerable<>));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsCollection(this Type type) {
			return type.HasInterface(typeof(ICollection<>));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetCollectionElementType(this Type type) {
			return type.GetInterfaceElementType(typeof(ICollection<>));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CheckObjectType(object obj, Type type, string argument = null) {
			if(obj != null ? !type.IsAssignableFrom(obj.GetType()) : (type.IsValueType && Nullable.GetUnderlyingType(type) == null)) {
				var message = (obj != null ? $"Object of type {obj.GetType().FullName ?? obj.GetType().Name}" : "null") + " " +
					$"is not assignable to type {type.FullName ?? type.Name}.";
				throw argument != null ? (Exception)new ArgumentException(message) : new InvalidOperationException(message);
			}
		}
	}
}
