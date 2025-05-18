using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NeatMapper.EntityFrameworkCore {
	internal static class TypeUtils {
		public static bool IsDefaultValueObject<T>(object value) {
			return object.Equals(value, default(T));
		}
		private static readonly MethodInfo this_IsDefaultValueObject = NeatMapper.TypeUtils.GetMethod(() => TypeUtils.IsDefaultValueObject<object>(default!));

		private static readonly ConcurrentDictionary<Type, Func<object, bool>> _isDefaultValueCache =
			new ConcurrentDictionary<Type, Func<object, bool>>();


		public static bool IsNullable(this Type type) {
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		public static bool IsNullable(this Type type, Type unwrappedType) {
			return type.IsNullable() && type.GetGenericArguments().Single() == unwrappedType;
		}

		public static Type UnwrapNullable(this Type type) {
			return type.IsNullable() ? type.GetGenericArguments().Single() : type;
		}

		public static bool IsDefaultValue(Type type, object value) {
			if (!type.IsValueType)
				return value == null;
			
			return _isDefaultValueCache.GetOrAdd(type, t => 
				(Func<object, bool>)Delegate.CreateDelegate(typeof(Func<object, bool>), this_IsDefaultValueObject.MakeGenericMethod(type)))
					.Invoke(value);
		}

		/// <summary>
		/// <see cref="Queryable.Where{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}})"/>
		/// </summary>
		private static readonly MethodInfo Queryable_Where = NeatMapper.TypeUtils.GetMethod(() => default(IQueryable<object>)!.Where(q => default(bool)));
		public static readonly MethodCacheFunc<Type, IQueryable, LambdaExpression, IQueryable> QueryableWhere =
			new MethodCacheFunc<Type, IQueryable, LambdaExpression, IQueryable>(
				(q, _) => q.ElementType,
				t => Queryable_Where.MakeGenericMethod(t),
				"queryable", "predicate");
	}
}
