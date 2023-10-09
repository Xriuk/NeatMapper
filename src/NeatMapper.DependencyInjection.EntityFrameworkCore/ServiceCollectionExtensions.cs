#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using NeatMapper.Configuration;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NeatMapper {
	public static class EntityFrameworkCoreServiceCollectionExtensions {
		private static readonly MethodInfo MapperOptions_TryAddNewMap = typeof(MapperOptions).GetMethod(nameof(MapperOptions.TryAddNewMap))
			?? throw new InvalidOperationException("Cannot find method MapperOptions.TryAddNewMap");
		private static readonly MethodInfo MapperOptions_TryAddMergeMap = typeof(MapperOptions).GetMethod(nameof(MapperOptions.TryAddMergeMap))
			?? throw new InvalidOperationException("Cannot find method MapperOptions.TryAddMergeMap");
		private static readonly MethodInfo MapperOptions_TryAddMatchMap = typeof(MapperOptions).GetMethod(nameof(MapperOptions.TryAddMatchMap))
			?? throw new InvalidOperationException("Cannot find method MapperOptions.TryAddMatchMap");
		private static readonly MethodInfo ServiceProviderServiceExtensions_GetRequiredService = typeof(ServiceProviderServiceExtensions)
			.GetMethods().FirstOrDefault(m => m.Name == nameof(ServiceProviderServiceExtensions.GetRequiredService) && m.IsGenericMethod)
				?? throw new InvalidOperationException("Cannot find method MapperOptions.TryAddMatchMap");


		public static IServiceCollection AddEntitiesMaps<TContext>(this IServiceCollection services) where TContext : DbContext {
			if (services == null)
				throw new ArgumentNullException(nameof(services));

			if (typeof(TContext).GetConstructor(Type.EmptyTypes) != null) {
				using(var instance = (TContext)Activator.CreateInstance(typeof(TContext))) {
					return services.AddEntitiesMaps<TContext>(instance.Model);
				}
			}
			else { 
				using (var serviceProvider = services.BuildServiceProvider()) {
					using(var scope = serviceProvider.CreateScope()) {
						return services.AddEntitiesMaps<TContext>(scope.ServiceProvider.GetRequiredService<TContext>().Model);
					}
				}
			}
		}

		public static IServiceCollection AddEntitiesMaps<TContext>(this IServiceCollection services, IModel model) where TContext : DbContext {
			if (services == null)
				throw new ArgumentNullException(nameof(services));
			if (model == null)
				throw new ArgumentNullException(nameof(model));

			services.Configure<MapperOptions>(options => {
				foreach(var entity in model.GetEntityTypes().Where(e => !e.IsOwned() && e.ClrType != null)) {
					var key = entity.FindPrimaryKey();
					if (key == null || key.Properties.Count < 1 || !key.Properties.All(k => k.PropertyInfo != null))
						continue;

					var keyTypes = key.Properties.Select(p => p.ClrType).ToArray();

					#region Entity -> Key (NewMap)
					// Add new map from entity to its key (composite keys are retrieved as tuples)
					var source = Expression.Parameter(entity.ClrType, "source");
					var context = Expression.Parameter(typeof(MappingContext), "context");
					Expression body;
					if (key.Properties.Count == 1) { 
						// source.Id
						body = Expression.Property(source, key.Properties[0].PropertyInfo);
					}
					else {
						// new Tuple<...>(source.Key1, ...)
						body = Expression.New(
							GetTupleConstructor(keyTypes),
							key.Properties.Select(p => Expression.Property(source, p.PropertyInfo)));
					}
					// source != null ? KEY : default(KEY)
					body = Expression.Condition(
						Expression.NotEqual(source, Expression.Constant(null, source.Type)),
						body,
						Expression.Default(body.Type));
					var expression = Expression.Lambda(typeof(NewMapDelegate<,>).MakeGenericType(source.Type, body.Type), body, source, context);
					MapperOptions_TryAddNewMap.MakeGenericMethod(source.Type, body.Type).Invoke(options, new object[] { expression.Compile() });

					// If the type has more than 1 key property we add the map to value tuple (and nullable) too
					// Otherwise if the key is a value type we map the nullable version too
					if (key.Properties.Count > 1 || key.Properties[0].ClrType.IsValueType) { 
						if (key.Properties.Count > 1) {
							// (source.Key1, ...)
							body = Expression.New(
								GetValueTupleConstructor(keyTypes),
								key.Properties.Select(p => Expression.Property(source, p.PropertyInfo)));

							// source != null ? KEY : default(KEY)
							body = Expression.Condition(
								Expression.NotEqual(source, Expression.Constant(null, source.Type)),
								body,
								Expression.Default(body.Type));
							expression = Expression.Lambda(typeof(NewMapDelegate<,>).MakeGenericType(source.Type, body.Type), body, source, context);
							MapperOptions_TryAddNewMap.MakeGenericMethod(source.Type, body.Type).Invoke(options, new object[] { expression.Compile() });
						}

						// (KEY?)source.Id or (KEY?)(source.Key1, ...)
						body = Expression.Convert(body, typeof(Nullable<>).MakeGenericType(body.Type));
						// source != null ? KEY : default(KEY)
						body = Expression.Condition(
							Expression.NotEqual(source, Expression.Constant(null, source.Type)),
							body,
							Expression.Default(body.Type));
						expression = Expression.Lambda(typeof(NewMapDelegate<,>).MakeGenericType(source.Type, body.Type), body, source, context);
						MapperOptions_TryAddNewMap.MakeGenericMethod(source.Type, body.Type).Invoke(options, new object[] { expression.Compile() });
					}
					#endregion
					
					Type tupleType;
					Type valueTupleType;
					if (key.Properties.Count > 1) {
						tupleType = Type.GetType("System.Tuple`" + key.Properties.Count)?.MakeGenericType(keyTypes)
							?? throw new InvalidOperationException("No tuple type for arguments length " + key.Properties.Count);
						valueTupleType = Type.GetType("System.ValueTuple`" + key.Properties.Count)?.MakeGenericType(keyTypes)
							?? throw new InvalidOperationException("No value tuple type for arguments length " + key.Properties.Count);
					}
					else {
						tupleType = null;
						valueTupleType = null;
					}

					#region Key == Entity (MatchMap) and Entity == Key (MatchMap)
					// Add match map from key to entity
					var destination = Expression.Parameter(entity.ClrType, "destination");
					context = Expression.Parameter(typeof(MatchingContext), "context");
					if (key.Properties.Count == 1) {
						source = Expression.Parameter(key.Properties[0].ClrType, "source");
						// source == destination.Id
						body = Expression.Equal(source, Expression.Property(destination, key.Properties[0].PropertyInfo));
					}
					else {
						source = Expression.Parameter(tupleType, "source");
						// source.Item1 == destination.Key1 && ...
						body = key.Properties.Select((p, i) => Expression.Equal(Expression.PropertyOrField(source, "Item" + (i + 1)), Expression.Property(destination, p.PropertyInfo)))
							.Aggregate((e1, e2) => Expression.AndAlso(e1, e2));
					}
					// destination != null && MATCH
					body = Expression.AndAlso(Expression.NotEqual(destination, Expression.Constant(null, destination.Type)), body);
					if (!source.Type.IsValueType) {
						// source != null && MATCH
						body = Expression.AndAlso(Expression.NotEqual(source, Expression.Constant(null, source.Type)), body);
					}
					expression = Expression.Lambda(typeof(MatchMapDelegate<,>).MakeGenericType(source.Type, destination.Type), body, source, destination, context);
					MapperOptions_TryAddMatchMap.MakeGenericMethod(source.Type, destination.Type).Invoke(options, new object[] { expression.Compile() });
					expression = Expression.Lambda(typeof(MatchMapDelegate<,>).MakeGenericType(destination.Type, source.Type), body, destination, source, context);
					MapperOptions_TryAddMatchMap.MakeGenericMethod(destination.Type, source.Type).Invoke(options, new object[] { expression.Compile() });

					// If the type has more than 1 key property we add the map from value tuple (and nullable) too
					// Otherwise if the key is a value type we map the nullable version too
					if (key.Properties.Count > 1 || key.Properties[0].ClrType.IsValueType) {
						if (key.Properties.Count > 1) {
							source = Expression.Parameter(valueTupleType, "source");
							// source.Item1 == destination.Key1 && ...
							body = key.Properties.Select((p, i) => Expression.Equal(Expression.PropertyOrField(source, "Item" + (i + 1)), Expression.Property(destination, p.PropertyInfo)))
								.Aggregate((e1, e2) => Expression.AndAlso(e1, e2));

							// destination != null && MATCH
							body = Expression.AndAlso(Expression.NotEqual(destination, Expression.Constant(null, destination.Type)), body);
							expression = Expression.Lambda(typeof(MatchMapDelegate<,>).MakeGenericType(source.Type, destination.Type), body, source, destination, context);
							MapperOptions_TryAddMatchMap.MakeGenericMethod(source.Type, destination.Type).Invoke(options, new object[] { expression.Compile() });
							expression = Expression.Lambda(typeof(MatchMapDelegate<,>).MakeGenericType(destination.Type, source.Type), body, destination, source, context);
							MapperOptions_TryAddMatchMap.MakeGenericMethod(destination.Type, source.Type).Invoke(options, new object[] { expression.Compile() });

							var nonNullableType = source.Type;
							source = Expression.Parameter(typeof(Nullable<>).MakeGenericType(nonNullableType), "source");
							// (SOURCE)source.Item1 == destination.Key1 && ...
							body = key.Properties.Select((p, i) => Expression.Equal(Expression.PropertyOrField(Expression.Convert(source, nonNullableType), "Item" + (i + 1)), Expression.Property(destination, p.PropertyInfo)))
								.Aggregate((e1, e2) => Expression.AndAlso(e1, e2));
						}
						else {
							var nonNullableType = source.Type;
							source = Expression.Parameter(typeof(Nullable<>).MakeGenericType(nonNullableType), "source");
							// (SOURCE)source == destination.Id
							body = Expression.Equal(Expression.Convert(source, nonNullableType), Expression.Property(destination, key.Properties[0].PropertyInfo));
						}

						// source.HasValue && destination != null && MATCH
						body = Expression.AndAlso(
							Expression.Property(source, nameof(Nullable<int>.HasValue)),
							Expression.AndAlso(
								Expression.NotEqual(destination, Expression.Constant(null, destination.Type)),
								body));
						expression = Expression.Lambda(typeof(MatchMapDelegate<,>).MakeGenericType(source.Type, destination.Type), body, source, destination, context);
						MapperOptions_TryAddMatchMap.MakeGenericMethod(source.Type, destination.Type).Invoke(options, new object[] { expression.Compile() });
						expression = Expression.Lambda(typeof(MatchMapDelegate<,>).MakeGenericType(destination.Type, source.Type), body, destination, source, context);
						MapperOptions_TryAddMatchMap.MakeGenericMethod(destination.Type, source.Type).Invoke(options, new object[] { expression.Compile() });
					}
					#endregion
					
					#region Key -> Entity (NewMap and MergeMap)
					// Add merge map from key to entity
					context = Expression.Parameter(typeof(MappingContext), "context");
					// context.ServiceProvider.GetRequiredService<TContext>()
					var contextInstance = Expression.Call(
						ServiceProviderServiceExtensions_GetRequiredService.MakeGenericMethod(typeof(TContext)),
						Expression.Property(context, nameof(MappingContext.ServiceProvider)));
					var findMethod = typeof(TContext).GetMethods().First(m => m.Name == nameof(DbContext.Find) && m.IsGenericMethod)
						.MakeGenericMethod(entity.ClrType);
					if (key.Properties.Count == 1) {
						source = Expression.Parameter(key.Properties[0].ClrType, "source");
						// DBCONTEXT.Find(new object[]{ (object)source })
						body = Expression.Call(contextInstance, findMethod, Expression.NewArrayInit(typeof(object), Expression.Convert(source, typeof(object))));
					}
					else {
						source = Expression.Parameter(tupleType, "source");
						// DBCONTEXT.Find(new object[]{ (object)source.Item1, ... })
						body = Expression.Call(contextInstance, findMethod, Expression.NewArrayInit(typeof(object),
							Enumerable.Range(1, key.Properties.Count)
								.Select(n => Expression.Convert(Expression.PropertyOrField(source, "Item" + n), typeof(object)))));
					}
					if (!source.Type.IsValueType) {
						// source != null ? MAP : null
						body = Expression.Condition(
							Expression.NotEqual(source, Expression.Constant(null, source.Type)),
							body,
							Expression.Constant(null, destination.Type));
					}
					expression = Expression.Lambda(typeof(NewMapDelegate<,>).MakeGenericType(source.Type, body.Type), body, source, context);
					MapperOptions_TryAddNewMap.MakeGenericMethod(source.Type, body.Type).Invoke(options, new object[] { expression.Compile() });

					// destination != null ? destination : MAP
					body = Expression.Condition(
						Expression.NotEqual(destination, Expression.Constant(null, destination.Type)),
						destination,
						body);
					expression = Expression.Lambda(typeof(MergeMapDelegate<,>).MakeGenericType(source.Type, destination.Type), body, source, destination, context);
					MapperOptions_TryAddMergeMap.MakeGenericMethod(source.Type, destination.Type).Invoke(options, new object[] { expression.Compile() });

					// If the type has more than 1 key property we add the map from value tuple (and nullable) too
					// Otherwise if the key is a value type we map the nullable version too
					if (key.Properties.Count > 1 || key.Properties[0].ClrType.IsValueType) {
						if (key.Properties.Count > 1) {
							source = Expression.Parameter(valueTupleType, "source");
							// DBCONTEXT.Find(new object[]{ (object)source.Item1, ... })
							body = Expression.Call(contextInstance, findMethod, Expression.NewArrayInit(typeof(object),
								Enumerable.Range(1, key.Properties.Count)
									.Select(n => Expression.Convert(Expression.PropertyOrField(source, "Item" + n), typeof(object)))));

							expression = Expression.Lambda(typeof(NewMapDelegate<,>).MakeGenericType(source.Type, body.Type), body, source, context);
							MapperOptions_TryAddNewMap.MakeGenericMethod(source.Type, body.Type).Invoke(options, new object[] { expression.Compile() });

							// destination != null ? destination : MAP
							Expression.Condition(
								Expression.NotEqual(destination, Expression.Constant(null, destination.Type)),
								destination,
								body);
							expression = Expression.Lambda(typeof(MergeMapDelegate<,>).MakeGenericType(source.Type, destination.Type), body, source, destination, context);
							MapperOptions_TryAddMergeMap.MakeGenericMethod(source.Type, destination.Type).Invoke(options, new object[] { expression.Compile() });

							var nonNullableType = source.Type;
							source = Expression.Parameter(typeof(Nullable<>).MakeGenericType(nonNullableType), "source");
							// DBCONTEXT.Find(new object[]{ (object)(SOURCE)source.Item1, ... })
							body = Expression.Call(contextInstance, findMethod, Expression.NewArrayInit(typeof(object),
								Enumerable.Range(1, key.Properties.Count)
									.Select(n => Expression.Convert(Expression.PropertyOrField(Expression.Convert(source, nonNullableType), "Item" + n), typeof(object)))));
						}
						else {
							var nonNullableType = source.Type;
							source = Expression.Parameter(typeof(Nullable<>).MakeGenericType(nonNullableType), "source");
							// DBCONTEXT.Find(new object[]{ (object)(SOURCE)source })
							body = Expression.Call(contextInstance, findMethod, Expression.NewArrayInit(typeof(object), Expression.Convert(Expression.Convert(source, nonNullableType), typeof(object))));
						}

						// source != null ? MAP : null
						body = Expression.Condition(
							Expression.NotEqual(source, Expression.Constant(null, source.Type)),
							body,
							Expression.Constant(null, body.Type));
						expression = Expression.Lambda(typeof(NewMapDelegate<,>).MakeGenericType(source.Type, body.Type), body, source, context);
						MapperOptions_TryAddNewMap.MakeGenericMethod(source.Type, body.Type).Invoke(options, new object[] { expression.Compile() });

						// destination != null ? destination : MAP
						body = Expression.Condition(
							Expression.NotEqual(destination, Expression.Constant(null, destination.Type)),
							destination,
							body);
						expression = Expression.Lambda(typeof(MergeMapDelegate<,>).MakeGenericType(source.Type, destination.Type), body, source, destination, context);
						MapperOptions_TryAddMergeMap.MakeGenericMethod(source.Type, destination.Type).Invoke(options, new object[] { expression.Compile() });
					}
					#endregion
				}
			});

			return services;
		}


		// https://stackoverflow.com/q/47236653/2672235
		private static ConstructorInfo GetTupleConstructor(Type[] types) {
			var tupleType = Type.GetType("System.Tuple`" + types.Length)
				?? throw new InvalidOperationException("No tuple type for arguments length " + types.Length);
			return tupleType.MakeGenericType(types).GetConstructor(types)
				?? throw new InvalidOperationException("No tuple constructor for arguments length " + types.Length);
		}

		private static ConstructorInfo GetValueTupleConstructor(Type[] types) {
			var tupleType = Type.GetType("System.ValueTuple`" + types.Length)
				?? throw new InvalidOperationException("No value tuple type for arguments length " + types.Length);
			return tupleType.MakeGenericType(types).GetConstructor(types)
				?? throw new InvalidOperationException("No value tuple constructor for arguments length " + types.Length);
		}

		// https://stackoverflow.com/a/40579063/2672235
		private static Delegate CreateDelegate(this MethodInfo methodInfo, object target) {
			Func<Type[], Type> getType;
			var isAction = methodInfo.ReturnType.Equals((typeof(void)));
			var types = methodInfo.GetParameters().Select(p => p.ParameterType);

			if (isAction) 
				getType = Expression.GetActionType;
			else {
				getType = Expression.GetFuncType;
				types = types.Concat(new[] { methodInfo.ReturnType });
			}

			if (methodInfo.IsStatic) 
				return Delegate.CreateDelegate(getType(types.ToArray()), methodInfo);


			return Delegate.CreateDelegate(getType(types.ToArray()), target, methodInfo.Name);
		}
	}
}
