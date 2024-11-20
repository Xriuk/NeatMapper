using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NeatMapper.EntityFrameworkCore {
	internal static class EfCoreUtils {
		/// <summary>
		/// Delegates which convert <see cref="Tuple"/> keys to their corresponding <see cref="ValueTuple"/>,
		/// keys are <see cref="Tuple"/> types.
		/// </summary>
		private static readonly ConcurrentDictionary<Type, Func<object, object>> _tupleToValueTupleCache =
			new ConcurrentDictionary<Type, Func<object, object>>();


		public static Func<object, object>? GetOrCreateTupleToValueTupleDelegate(Type tuple) {
			if (!tuple.IsTuple())
				return null;

			return _tupleToValueTupleCache.GetOrAdd(tuple, tupleType => {
				var keyParam = Expression.Parameter(typeof(object), "key");
				// (object)new ValueTuple<...>(((Tuple<...>)key).Item1, ...)
				Expression body = Expression.Convert(Expression.New(
					TupleUtils.GetValueTupleConstructor(tupleType.GetGenericArguments()),
					Enumerable.Range(1, tupleType.GetGenericArguments().Length)
						.Select(n => Expression.Property(Expression.Convert(keyParam, tupleType), "Item" + n))), typeof(object));
				return Expression.Lambda<Func<object, object>>(body, keyParam).Compile();
			});
		}

		public static bool IsKeyType(this Type type) {
			return type.IsPrimitive || type == typeof(DateTime)
				// DEV: add byte array? and others
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


		private sealed class Finalizer<TValue> where TValue : class {
			private readonly Action<TValue> action;

			public TValue Value { get; }


			public Finalizer(TValue key, Action<TValue> action) {
				this.Value = key;
				this.action = action;
			}

			~Finalizer() {
				action?.Invoke(Value);
			}
		}

		/// <summary>
		/// Semaphores used to lock on DbContext instances, each semaphore should be disposed automatically when
		/// the corresponding context is (depending on the GC).
		/// </summary>
		private static readonly ConditionalWeakTable<DbContext, Finalizer<SemaphoreSlim>> _dbContextSemaphores =
#if !NET47_OR_GREATER
#pragma warning disable IDE0028
#endif
			new ConditionalWeakTable<DbContext, Finalizer<SemaphoreSlim>>();
#if !NET47_OR_GREATER
#pragma warning restore IDE0028
#endif


		public static SemaphoreSlim GetOrCreateSemaphoreForDbContext(DbContext dbContext) {
			return _dbContextSemaphores.GetValue(dbContext, key => new Finalizer<SemaphoreSlim>(new SemaphoreSlim(1), s => s.Dispose())).Value;
		}



		/// <summary>
		/// <see cref="SemaphoreSlim.Wait()"/>
		/// </summary>
		public static readonly MethodInfo SemaphoreSlim_Wait = typeof(SemaphoreSlim).GetMethods().First(m =>
			m.Name == nameof(SemaphoreSlim.Wait) && m.GetParameters().Length == 0);

		/// <summary>
		/// <see cref="SemaphoreSlim.Release()"/>
		/// </summary>
		public static readonly MethodInfo SemaphoreSlim_Release = typeof(SemaphoreSlim).GetMethods().First(m =>
			m.Name == nameof(SemaphoreSlim.Release) && m.GetParameters().Length == 0);

		/// <summary>
		/// <see cref="DbContext.Entry(object)"/>
		/// </summary>
		public static readonly MethodInfo DbContext_Entry = typeof(DbContext).GetMethods().First(m =>
			m.Name == nameof(DbContext.Entry) && !m.IsGenericMethod);

		/// <summary>
		/// <see cref="EntityEntry.State"/>
		/// </summary>
		public static readonly PropertyInfo EntityEntry_State = typeof(EntityEntry).GetProperty(nameof(EntityEntry.State))
			?? throw new Exception("Could not find EntityEntry.State");

		/// <summary>
		/// <see cref="EntityEntry.Property(string)"/>
		/// </summary>
		public static readonly MethodInfo EntityEntry_Property = typeof(EntityEntry).GetMethod(nameof(EntityEntry.Property), [ typeof(string) ])
			?? throw new Exception("Could not find EntityEntry.Property(string)");

		/// <summary>
		/// <see cref="MemberEntry.CurrentValue"/>
		/// </summary>
		public static readonly PropertyInfo MemberEntry_CurrentValue = typeof(MemberEntry).GetProperty(nameof(MemberEntry.CurrentValue))
			?? throw new Exception("Could not find MemberEntry.CurrentValue");

		/// <summary>
		/// <see cref="EF.Property{TProperty}(object, string)"/>
		/// </summary>
		public static readonly MethodInfo EF_Property = typeof(EF).GetMethod(nameof(EF.Property))
			?? throw new Exception("Could not find EF.Property<T>(object, string)");
	}
}
