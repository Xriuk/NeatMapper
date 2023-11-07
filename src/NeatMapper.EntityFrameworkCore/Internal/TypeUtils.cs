#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Linq;
using System.Reflection;

namespace NeatMapper.EntityFrameworkCore {
	internal static class TypeUtils {
		private static readonly MethodInfo TypeUtils_IsDefaultValue = typeof(TypeUtils).GetMethods().First(m => m.Name == nameof(TypeUtils.IsDefaultValue) && m.IsGenericMethod);


		public static bool IsNullable(this Type type) {
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		public static bool IsNullable(this Type type, Type unwrappedType) {
			return type.IsNullable() && type.GetGenericArguments().Single() == unwrappedType;
		}

		public static Type UnwrapNullable(this Type type) {
			return type.IsNullable() ? type.GetGenericArguments().Single() : type;
		}

		public static bool IsDefaultValue<T>(T value) {
			return value.Equals(default(T));
		}

		public static bool IsDefaultValue(Type type, object value) {
			return (bool)TypeUtils_IsDefaultValue.MakeGenericMethod(type).Invoke(null, new object[] { value });
		}
	}
}
