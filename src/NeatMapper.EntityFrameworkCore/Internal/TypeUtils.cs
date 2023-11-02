#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Linq;

namespace NeatMapper.EntityFrameworkCore {
	internal static class TypeUtils {
		public static bool IsNullable(this Type type) {
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		public static bool IsNullable(this Type type, Type unwrappedType) {
			return type.IsNullable() && type.GetGenericArguments().Single() == unwrappedType;
		}
	}
}
