#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper.EntityFrameworkCore {
	internal static class EfCoreUtils {
		/// <summary>
		/// Delegates which convert <see cref="Tuple"/> keys to their corresponding <see cref="ValueTuple"/>,
		/// keys are <see cref="Tuple"/> types.
		/// </summary>
		private static readonly ConcurrentDictionary<Type, Func<object, object>> _tupleToValueTupleCache =
			new ConcurrentDictionary<Type, Func<object, object>>();


		public static Func<object, object> GetOrCreateTupleToValueTupleMap(Type tuple) {
			if (!tuple.IsTuple())
				throw new ArgumentException("Type is not a Tuple", nameof(tuple));

			return _tupleToValueTupleCache.GetOrAdd(tuple, tupleType => {
				var keyParam = Expression.Parameter(typeof(object), "key");
				// (object)new ValueTuple<...>(((Tuple<...>)key).Item1, ...)
				Expression body = Expression.Convert(Expression.New(
					TupleUtils.GetValueTupleConstructor(tupleType.GetGenericArguments()),
					Enumerable.Range(1, tupleType.GetGenericArguments().Count())
						.Select(n => Expression.Property(Expression.Convert(keyParam, tupleType), "Item" + n))), typeof(object));
				return Expression.Lambda<Func<object, object>>(body, keyParam).Compile();
			});
		}

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
			return arguments.Length > 1 && arguments.All(a => a.IsKeyType());
		}
	}
}
