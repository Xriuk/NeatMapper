using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// <see cref="IProjector"/> which projects entities into their keys, even composite keys
	/// as <see cref="Tuple"/> or <see cref="ValueTuple"/>.
	/// </summary>
	public sealed class EntityFrameworkCoreProjector : IProjector, IProjectorCanProject {
		/// <summary>
		/// <see cref="EF.Property{TProperty}(object, string)"/>
		/// </summary>
		private static readonly MethodInfo EF_Property = typeof(EF).GetMethod(nameof(EF.Property))
			?? throw new Exception("Could not find EF.Property<T>(object, string)");
		/// <summary>
		/// <see cref="DbContext.Entry(object)"/>
		/// </summary>
		private static readonly MethodInfo DbContext_Entry = typeof(DbContext).GetMethods().Single(m => m.Name == nameof(DbContext.Entry) && !m.IsGenericMethod);
		/// <summary>
		/// <see cref="EntityEntry.State"/>
		/// </summary>
		private static readonly PropertyInfo EntityEntry_State = typeof(EntityEntry).GetProperty(nameof(EntityEntry.State))
			?? throw new Exception("Could not find EntityEntry.State");
		/// <summary>
		/// <see cref="EntityEntry.Property(string)"/>
		/// </summary>
		private static readonly MethodInfo EntityEntry_Property = typeof(EntityEntry).GetMethod(nameof(EntityEntry.Property))
			?? throw new Exception("Could not find EntityEntry.Property(string)");


		private readonly IModel _model;
		private readonly Type _dbContextType;

		/// <summary>
		/// Creates a new instance of <see cref="EntityFrameworkCoreProjector"/>.
		/// </summary>
		/// <param name="model">Model to use to retrieve keys of entities.</param>
		/// <param name="dbContextType">Type of the database context to use, must derive from <see cref="DbContext"/>.</param>
		public EntityFrameworkCoreProjector(IModel model, Type dbContextType) {
			_model = model ?? throw new ArgumentNullException(nameof(model));
			_dbContextType = dbContextType ?? throw new ArgumentNullException(nameof(dbContextType));
			if (!typeof(DbContext).IsAssignableFrom(_dbContextType))
				throw new ArgumentException($"Type {_dbContextType.FullName ?? _dbContextType.Name} is not derived from DbContext", nameof(dbContextType));
		}


		public LambdaExpression Project(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (!CanProject(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			var param = Expression.Parameter(sourceType, "entity");

			var entity = _model.FindEntityType(sourceType);
			var key = entity.FindPrimaryKey();
			var isCompiling = mappingOptions.GetOptions<ProjectionCompilationContext>() != null;
			var dbContext = isCompiling ? mappingOptions.GetOptions<EntityFrameworkCoreMappingOptions>()?.DbContextInstance : null;

			Expression body;
			if (isCompiling && key.Properties.Any(k => k.IsShadowProperty())) {
				if(dbContext == null)
					throw new MapNotFoundException((sourceType, destinationType));

				var entityEntry = Expression.Variable(typeof(EntityEntry), "entity");

				var properties = key.Properties.Select(k => {
					// (Type)context.Entry(entity).Property("Id").CurrentValue
					return Expression.Convert(
						// context.Entry(entity).Property("Id").CurrentValue
						Expression.Property(
							// context.Entry(entity).Property("Id")
							Expression.Call(
								entityEntry,
								EntityEntry_Property,
								Expression.Constant(k.Name)
							),
							nameof(PropertyEntry.CurrentValue)
						),
						k.ClrType
					);
				});
				if (key.Properties.Count == 1) {
					// entity.Id
					body = properties.Single();
				}
				else {
					// new Tuple<...>(entity.Key1, ...)
					// new ValueTuple<...>(entity.Key1, ...)
					body = Expression.New(
						(destinationType.IsTuple() ?
							TupleUtils.GetTupleConstructor(destinationType.UnwrapNullable().GetGenericArguments()) :
							TupleUtils.GetValueTupleConstructor(destinationType.UnwrapNullable().GetGenericArguments())),
						properties);
				}

				body = Expression.TryCatch(
					Expression.Block(
						new [] { entityEntry },
						// entity = context.Entry(entity)
						Expression.Assign(
							entityEntry,
							Expression.Call(
								Expression.Constant(dbContext, _dbContextType),
								DbContext_Entry,
								param)),
						// Throws MapNotFoundException if the DbContext is disposed
						// entity.State == EntityState.Detached ? throw ... : ...
						Expression.Condition(
							Expression.Equal(Expression.Property(entityEntry, EntityEntry_State), Expression.Constant(EntityState.Detached)),
							Expression.Throw(
								Expression.New(
									typeof(ProjectionException).GetConstructors().Single(),
									Expression.New(
										typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) }),
										Expression.Constant($"The entity of type {sourceType.FullName ?? sourceType.Name} is not being tracked by the provided context, so its shadow key(s) cannot be retrieved locally.")),
									Expression.Constant((sourceType, destinationType))
								),
								body.Type
							),
							body
						)
					),
					Expression.Catch(
						typeof(ObjectDisposedException),
						Expression.Throw(
							Expression.New(
								typeof(MapNotFoundException).GetConstructor(new[] { typeof((Type, Type)) }),
								Expression.Constant((sourceType, destinationType))
							),
							body.Type
						)
					)
				);
			}
			else {
				var properties = key.Properties.Select(k => {
					if (k.IsShadowProperty()) {
						// EF.Property<Type>(entity, "Id")
						return Expression.Call(EF_Property.MakeGenericMethod(k.ClrType), param, Expression.Constant(k.Name));
					}
					else if (k.PropertyInfo != null) {
						// entity.Id
						return (Expression)Expression.Property(param, k.PropertyInfo);
					}
					else {
						// entity.Id
						return (Expression)Expression.Field(param, k.FieldInfo);
					}
				});
				if (key.Properties.Count == 1) {
					// entity.Id
					body = properties.Single();
				}
				else {
					// new Tuple<...>(entity.Key1, ...)
					// new ValueTuple<...>(entity.Key1, ...)
					body = Expression.New(
						(destinationType.IsTuple() ?
							TupleUtils.GetTupleConstructor(destinationType.UnwrapNullable().GetGenericArguments()) :
							TupleUtils.GetValueTupleConstructor(destinationType.UnwrapNullable().GetGenericArguments())),
						properties);
				}
			}

			if (destinationType.IsNullable()) { 
				// (KEY?)body
				body = Expression.Convert(body, destinationType);
			}

			// entity != null ? KEY : default(KEY)
			body = Expression.Condition(
				Expression.NotEqual(param, Expression.Constant(null, param.Type)),
				body,
				Expression.Default(body.Type));

			return Expression.Lambda(body, param);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public bool CanProject(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// We only project entities to keys
			if(!sourceType.IsClass || (!destinationType.IsKeyType() && !destinationType.IsCompositeKeyType()))
				return false;

			// Check if the entity is in the model
			var modelEntity = _model.FindEntityType(sourceType);
			if (modelEntity == null || modelEntity.IsOwned())
				return false;

			// Check that the entity has a key
			var key = modelEntity.FindPrimaryKey();
			if (key == null || key.Properties.Count < 1)
				return false;

			// Shadow keys (or partially shadow composite keys) can be projected only if we are not compiling
			// or a db context is passed to retrieve the tracked instances
			if(key.Properties.Any(p => p.IsShadowProperty()) && mappingOptions.GetOptions<ProjectionCompilationContext>() != null) {
				var contextType = mappingOptions.GetOptions<EntityFrameworkCoreMappingOptions>()?.DbContextInstance?.GetType();
				if(contextType == null || !_dbContextType.IsAssignableFrom(contextType))
					return false;
			}

			// Check that the key type matches
			if (destinationType.IsCompositeKeyType()) {
				var keyTypes = destinationType.UnwrapNullable().GetGenericArguments();
				if (key.Properties.Count != keyTypes.Length || !keyTypes.Zip(key.Properties, (k1, k2) => (k1, k2.ClrType)).All(keys => keys.Item1 == keys.Item2))
					return false;
			}
			else if (key.Properties.Count != 1 || (key.Properties[0].ClrType != destinationType && !destinationType.IsNullable(key.Properties[0].ClrType)))
				return false;

			return true;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
