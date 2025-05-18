using System;
using System.Reflection;

namespace NeatMapper.EntityFrameworkCore {
	internal static class TupleUtils {
		public static bool IsTuple(this Type type) {
			return type.FullName?.StartsWith("System.Tuple`") == true;
		}

		public static bool IsValueTuple(this Type type) {
			return type.FullName?.StartsWith("System.ValueTuple`") == true;
		}

		public static bool IsNullableValueTuple(this Type type) {
			return type.IsNullable() && type.GetGenericArguments()[0].IsValueTuple();
		}

		// https://stackoverflow.com/q/47236653/2672235
		public static ConstructorInfo GetTupleConstructor(Type[] types) {
			var tupleType = Type.GetType("System.Tuple`" + types.Length)
				?? throw new InvalidOperationException("No tuple type for arguments length " + types.Length);
			return tupleType.MakeGenericType(types).GetConstructor(types)
				?? throw new InvalidOperationException("No tuple constructor for arguments length " + types.Length);
		}

		public static ConstructorInfo GetValueTupleConstructor(Type[] types) {
			var tupleType = Type.GetType("System.ValueTuple`" + types.Length)
				?? throw new InvalidOperationException("No value tuple type for arguments length " + types.Length);
			return tupleType.MakeGenericType(types).GetConstructor(types)
				?? throw new InvalidOperationException("No value tuple constructor for arguments length " + types.Length);
		}

		public static Type TupleToValueTuple(this Type type) {
			return type.IsTuple() ? TupleUtils.GetValueTupleConstructor(type.GetGenericArguments()).DeclaringType! : type;
		}
	}
}
