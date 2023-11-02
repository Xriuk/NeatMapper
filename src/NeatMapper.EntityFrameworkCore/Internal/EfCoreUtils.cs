#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Linq;

namespace NeatMapper.EntityFrameworkCore {
	internal static class EfCoreUtils {
		public static bool IsKeyType(this Type type) {
			return type.IsPrimitive || type == typeof(DateTime)
#if NET6_0_OR_GREATER
				|| type == typeof(DateOnly)
#endif
				|| type == typeof(Guid)
				|| type == typeof(string)
				|| (type.IsNullable() && type.GetGenericArguments()[0].IsKeyType());
		}

		public static bool IsCompositeKeyType(this Type type) {
			if(!type.IsGenericType || (!type.IsValueTuple() && !type.IsNullableValueTuple() && !type.IsTuple()))
				return false;

			var arguments = type.GetGenericArguments();
			if(type.IsNullableValueTuple())
				arguments = arguments[0].GetGenericArguments();
			return arguments.Length > 0 && arguments.All(a => a.IsKeyType());
		}
	}
}
