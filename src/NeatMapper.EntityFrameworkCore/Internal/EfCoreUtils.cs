using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
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
				var tupleArgs = tupleType.GetGenericArguments();
				// (object)new ValueTuple<...>(((Tuple<...>)key).Item1, ...)
				Expression body = Expression.Convert(Expression.New(
					TupleUtils.GetValueTupleConstructor(tupleArgs),
					Enumerable.Range(1, tupleArgs.Length)
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

		public static IEntityType? RetrieveEntityType(IModel model, Type entityType) {
			// Retrieve all the matching types
			var entityTypes = model.GetEntityTypes().Where(e => e.ClrType == entityType);
			if(!entityTypes.Any())
				return null;

			// If the type is not owned it must be a single one
			if(!entityTypes.First().IsOwned())
				return entityTypes.Single();

			// If a type is mapped to multiple entities (currently owned ones),
			// all the types should have the same key configuration (excluding the parent foreign keys)
			if (entityTypes.Select(e => {
					var keyProperties = e.FindPrimaryKey()
						?.Properties.Where(p => !p.GetContainingForeignKeys().Any(k => k.IsOwnership));
					if (keyProperties?.Any() != true)
						return null;
					else {
						return string.Join("~", keyProperties
							.Select(p => $"{(p.IsShadowProperty() ? "s" : "n")}{p.Name}-{p.ClrType.FullName}"));
					}
				})
				.Distinct()
				.Count() != 1) {
				
				return null;
			}

			return entityTypes.First();
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
#if !NET48_OR_GREATER
#pragma warning disable IDE0028
#endif
			new ConditionalWeakTable<DbContext, Finalizer<SemaphoreSlim>>();
#if !NET48_OR_GREATER
#pragma warning restore IDE0028
#endif


		public static SemaphoreSlim GetOrCreateSemaphoreForDbContext(DbContext dbContext) {
			return _dbContextSemaphores.GetValue(dbContext, key => new Finalizer<SemaphoreSlim>(new SemaphoreSlim(1), s => s.Dispose())).Value;
		}



		/// <summary>
		/// <see cref="SemaphoreSlim.Wait()"/>
		/// </summary>
		public static readonly MethodInfo SemaphoreSlim_Wait = NeatMapper.TypeUtils.GetMethod(() => default(SemaphoreSlim)!.Wait());

		/// <summary>
		/// <see cref="SemaphoreSlim.Release()"/>
		/// </summary>
		public static readonly MethodInfo SemaphoreSlim_Release = NeatMapper.TypeUtils.GetMethod(() => default(SemaphoreSlim)!.Release());

		/// <summary>
		/// <see cref="DbContext.Entry(object)"/>
		/// </summary>
		public static readonly MethodInfo DbContext_Entry = NeatMapper.TypeUtils.GetMethod(() => default(DbContext)!.Entry(default(object)!));

		/// <summary>
		/// <see cref="EntityEntry.State"/>
		/// </summary>
		public static readonly PropertyInfo EntityEntry_State = NeatMapper.TypeUtils.GetProperty(() => default(EntityEntry)!.State);

		/// <summary>
		/// <see cref="EntityEntry.Property(string)"/>
		/// </summary>
		public static readonly MethodInfo EntityEntry_Property = NeatMapper.TypeUtils.GetMethod(() => default(EntityEntry)!.Property(default(string)!));

		/// <summary>
		/// <see cref="MemberEntry.CurrentValue"/>
		/// </summary>
		public static readonly PropertyInfo MemberEntry_CurrentValue = NeatMapper.TypeUtils.GetProperty(() => default(MemberEntry)!.CurrentValue);

		/// <summary>
		/// <see cref="EF.Property{TProperty}(object, string)"/>
		/// </summary>
		public static readonly MethodInfo EF_Property = NeatMapper.TypeUtils.GetMethod(() => EF.Property<object>(default(object)!, default(string)!));
	}
}
