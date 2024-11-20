using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NeatMapper.EntityFrameworkCore {
	internal static class TypeUtils {
		/// <summary>
		/// <see cref="IsDefaultValue{T}(T)"/>
		/// </summary>
		private static readonly MethodInfo TypeUtils_IsDefaultValue = typeof(TypeUtils).GetMethods()
			.First(m => m.Name == nameof(TypeUtils.IsDefaultValue) && m.IsGenericMethod);
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

		public static bool IsDefaultValue<T>(T value) {
			return Object.Equals(value, default(T));
		}

		public static bool IsDefaultValue(Type type, object value) {
			if (!type.IsValueType)
				return value == null;
			
			return _isDefaultValueCache.GetOrAdd(type, t => {
				var param = Expression.Parameter(typeof(object), "value");
				var body = Expression.Call(TypeUtils_IsDefaultValue.MakeGenericMethod(type), Expression.Convert(param, t));
				return Expression.Lambda<Func<object, bool>>(body, param).Compile();
			}).Invoke(value);
		}

		/// <summary>
		/// <see cref="Queryable.Where{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}})"/>
		/// </summary>
		private static readonly MethodInfo Queryable_Where = typeof(Queryable).GetMethods().First(m => {
			if (m.Name != nameof(Queryable.Where))
				return false;
			var parameters = m.GetParameters();
			if (parameters.Length == 2 && parameters[1].ParameterType.IsGenericType) {
				var delegateType = parameters[1].ParameterType.GetGenericArguments()[0];
				if (delegateType.IsGenericType && delegateType.GetGenericTypeDefinition() == typeof(Func<,>))
					return true;
			}

			return false;
		});
		public static readonly MethodCacheFunc<Type, IQueryable, LambdaExpression, IQueryable> QueryableWhere =
			new MethodCacheFunc<Type, IQueryable, LambdaExpression, IQueryable>(
				(q, _) => q.ElementType,
				t => Queryable_Where.MakeGenericMethod(t),
				"queryable", "predicate");
	}
}
