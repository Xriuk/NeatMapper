using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// For IServiceProviderIsService
#if !NET5_0 && !NETCOREAPP3_1
using Microsoft.Extensions.DependencyInjection;
#endif
using System;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// <see cref="IProjector"/> which projects entities into their keys (even composite keys
	/// as <see cref="Tuple"/> or <see cref="ValueTuple"/>, and shadow keys).
	/// </summary>
	/// <remarks>
	/// When working with shadow keys, a <see cref="DbContext"/> will be required.
	/// Since a single <see cref="DbContext"/> instance cannot be used concurrently and is not thread-safe
	/// on its own, every access to the provided <see cref="DbContext"/> instance and all its members
	/// (local and remote) for each projection is protected by a semaphore.<br/>
	/// This makes this class thread-safe and concurrently usable, though not necessarily efficient to do so.<br/>
	/// Any external concurrent use of the <see cref="DbContext"/> instance is not monitored and could throw exceptions,
	/// so you should not be accessing the context externally while projecting.
	/// </remarks>
	public sealed class EntityFrameworkCoreProjector : IProjector, IProjectorCanProject {
		/// <summary>
		/// Db model, shared between instances of the same DbContext type.
		/// </summary>
		private readonly IModel _model;

		/// <summary>
		/// Type of DbContext to retrieve from <see cref="_serviceProvider"/>. Used for shadow keys.
		/// </summary>
		private readonly Type _dbContextType;

		/// <summary>
		/// Service provider used to retrieve <see cref="DbContext"/> instances. Used for shadow keys.
		/// </summary>
		private readonly IServiceProvider _serviceProvider;


		/// <summary>
		/// Creates a new instance of <see cref="EntityFrameworkCoreProjector"/>.
		/// </summary>
		/// <param name = "model" > Model to use to retrieve keys of entities.</param>
		/// <param name="dbContextType">
		/// Type of the database context to use, must derive from <see cref="DbContext"/>.
		/// </param>
		/// <param name="serviceProvider">
		/// Optional service provider used to retrieve instances of <paramref name="dbContextType"/> context.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions.ServiceProvider"/>.
		/// </param>
		public EntityFrameworkCoreProjector(
			IModel model,
			Type dbContextType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) {

			_model = model ?? throw new ArgumentNullException(nameof(model));
			_dbContextType = dbContextType ?? throw new ArgumentNullException(nameof(dbContextType));
			if (!typeof(DbContext).IsAssignableFrom(_dbContextType))
				throw new ArgumentException($"Type {_dbContextType.FullName ?? _dbContextType.Name} is not derived from DbContext", nameof(dbContextType));
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;

#if !NET5_0 && !NETCOREAPP3_1
			if (_serviceProvider.GetService<IServiceProviderIsService>()?.IsService(dbContextType) == false)
				throw new ArgumentException($"The provided IServiceProvider does not support the DbContext of type {_dbContextType.FullName ?? _dbContextType.Name}", nameof(serviceProvider));
#endif
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

			var entityParam = Expression.Parameter(sourceType, "entity");

			var entity = _model.FindEntityType(sourceType);
			var key = entity.FindPrimaryKey();
			var isCompiling = mappingOptions?.GetOptions<ProjectionCompilationContext>() != null;
			var dbContext = isCompiling ? RetrieveDbContext(mappingOptions) : null;

			Expression body;
			if (isCompiling && key.Properties.Any(k => k.IsShadowProperty())) {
				if(dbContext == null)
					throw new MapNotFoundException((sourceType, destinationType));

				var dbContextSemaphore = EfCoreUtils.GetOrCreateSemaphoreForDbContext(dbContext);

				var dbContextConstant = Expression.Constant(dbContext, _dbContextType);
				var dbContextSemaphoreConstant = Expression.Constant(dbContextSemaphore);
				var entityEntryVar = Expression.Variable(typeof(EntityEntry), "entityEntry");

				var properties = key.Properties.Select(k => {
					// (Type)entityEntry.Property("Prop1").CurrentValue
					// entity.Prop1
					if (k.IsShadowProperty()) { 
						return Expression.Convert(
							Expression.Property(
								Expression.Call(
									entityEntryVar,
									EfCoreUtils.EntityEntry_Property,
									Expression.Constant(k.Name)
								),
								EfCoreUtils.MemberEntry_CurrentValue
							),
							k.ClrType
						);
					}
					else
						return (Expression)Expression.PropertyOrField(entityParam, k.Name);
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

				var catchExceptionParam = Expression.Parameter(typeof(Exception), "e");

				body = Expression.TryCatch(
					Expression.Block(
						new [] { entityEntryVar },
						// dbContextSemaphore.Wait()
						Expression.Call(dbContextSemaphoreConstant, EfCoreUtils.SemaphoreSlim_Wait),
						Expression.TryFinally(
							Expression.Block(new[] { entityEntryVar },
								// entityEntry = context.Entry(entity)
								Expression.Assign(
									entityEntryVar,
									Expression.Call(dbContextConstant, EfCoreUtils.DbContext_Entry, entityParam)),
								// entity.State == EntityState.Detached ? throw ... : ...
								Expression.Condition(
									Expression.Equal(Expression.Property(entityEntryVar, EfCoreUtils.EntityEntry_State), Expression.Constant(EntityState.Detached)),
									Expression.Throw(
										Expression.New(
											typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) }),
											Expression.Constant($"The entity of type {sourceType.FullName ?? sourceType.Name} is not being tracked " +
												$"by the provided {nameof(DbContext)}, so its shadow key(s) cannot be retrieved locally. " +
												$"Either provide a valid {nameof(DbContext)} or pass a tracked entity.")),
										body.Type),
									body)),
							// dbContextSemaphore.Release()
							Expression.Call(dbContextSemaphoreConstant, EfCoreUtils.SemaphoreSlim_Release))),
						Expression.Catch(
							catchExceptionParam,
							Expression.Throw(
								Expression.New(
									typeof(ProjectionException).GetConstructors().Single(),
									catchExceptionParam,
									Expression.Constant((sourceType, destinationType))),
								body.Type)));
			}
			else {
				var properties = key.Properties.Select(k => {
					if (k.IsShadowProperty()) {
						// EF.Property<Type>(entity, "Id")
						return Expression.Call(EfCoreUtils.EF_Property.MakeGenericMethod(k.ClrType), entityParam, Expression.Constant(k.Name));
					}
					else if (k.PropertyInfo != null) {
						// entity.Id
						return (Expression)Expression.Property(entityParam, k.PropertyInfo);
					}
					else {
						// entity.Id
						return (Expression)Expression.Field(entityParam, k.FieldInfo);
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
				Expression.NotEqual(entityParam, Expression.Constant(null, entityParam.Type)),
				body,
				Expression.Default(body.Type));

			return Expression.Lambda(body, entityParam);

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

			// Check that the entity has a key and that it matches the key type
			var key = modelEntity.FindPrimaryKey();
			if (key == null || key.Properties.Count < 1)
				return false;
			if (destinationType.IsCompositeKeyType()) {
				var keyTypes = destinationType.UnwrapNullable().GetGenericArguments();
				if (key.Properties.Count != keyTypes.Length || !keyTypes.Zip(key.Properties, (k1, k2) => (k1, k2.ClrType)).All(keys => keys.Item1 == keys.Item2))
					return false;
			}
			else if (key.Properties.Count != 1 || key.Properties[0].ClrType != destinationType.UnwrapNullable())
				return false;

			// Shadow keys (or partially shadow composite keys) can be projected only if we are not compiling
			// or we have a db context to retrieve the tracked instances
			if (key.Properties.Any(p => p.IsShadowProperty()) && mappingOptions?.GetOptions<ProjectionCompilationContext>() != null &&
				RetrieveDbContext(mappingOptions) == null) {

				return false;
			}

			return true;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		// Retrieves the DbContext if available, may return null
		private DbContext RetrieveDbContext(MappingOptions mappingOptions) {
			var dbContext = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>()?.DbContextInstance;
			if (dbContext != null && dbContext.GetType() != _dbContextType)
				dbContext = null;

			if (dbContext == null && mappingOptions?.GetOptions<ProjectionCompilationContext>() != null) {
				try {
					dbContext = (mappingOptions?.GetOptions<MapperOverrideMappingOptions>()?.ServiceProvider ?? _serviceProvider)
						.GetService(_dbContextType) as DbContext;
				}
				catch { }
			}

			if (dbContext == null) {
				try {
					dbContext = (mappingOptions?.GetOptions<ProjectorOverrideMappingOptions>()?.ServiceProvider ?? _serviceProvider)
						.GetService(_dbContextType) as DbContext;
				}
				catch { }
			}

			return dbContext;
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
