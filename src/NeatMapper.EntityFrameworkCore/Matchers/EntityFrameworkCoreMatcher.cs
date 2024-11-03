using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// <see cref="IMatcher"/> which matches entities with their keys (even composite keys as <see cref="Tuple"/>
	/// or <see cref="ValueTuple"/>, and shadow keys). Allows also matching entities with other entities.
	/// Will return <see langword="false"/> if at least one of the entities/key is null, so it won't match
	/// null entities and keys, but will match default non-nullable keys (like <see langword="0"/>,
	/// <see cref="Guid.Empty"/>, ...).
	/// </summary>
	/// <remarks>
	/// When working with shadow keys, a <see cref="DbContext"/> will be required.
	/// Since a single <see cref="DbContext"/> instance cannot be used concurrently and is not thread-safe
	/// on its own, every access to the provided <see cref="DbContext"/> instance and all its members
	/// (local and remote) for each match is protected by a semaphore.<br/>
	/// This makes this class thread-safe and concurrently usable, though not necessarily efficient to do so.<br/>
	/// Any external concurrent use of the <see cref="DbContext"/> instance is not monitored and could throw exceptions,
	/// so you should not be accessing the context externally while matching.
	/// </remarks>
	public sealed class EntityFrameworkCoreMatcher : IMatcher, IMatcherFactory {
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
		/// Values indicating if the given type has at least one shadow key or not, keys are entity types.
		/// </summary>
		// DEV: maybe not worth it? Delete?
		private readonly ConcurrentDictionary<Type, bool> _entityShadowKeyCache = new ConcurrentDictionary<Type, bool>();

		/// <summary>
		/// Delegates which compare an entity with its key, keys are entity types, the order of the parameters is: entity, key.
		/// </summary>
		private readonly ConcurrentDictionary<Type, Func<object, object, SemaphoreSlim, DbContext, bool>> _entityKeyComparerCache =
			new ConcurrentDictionary<Type, Func<object, object, SemaphoreSlim, DbContext, bool>>();

		/// <summary>
		/// Delegates which compare an entity with another entity of the same type, keys are entity types.
		/// </summary>
		private readonly ConcurrentDictionary<Type, Func<object, object, SemaphoreSlim, DbContext, bool>> _entityEntityComparerCache =
			new ConcurrentDictionary<Type, Func<object, object, SemaphoreSlim, DbContext, bool>>();


		/// <summary>
		/// Creates a new instance of <see cref="EntityFrameworkCoreMatcher"/>.
		/// </summary>
		/// <param name="model">Model to use to retrieve keys of entities.</param>
		/// <param name="dbContextType">
		/// Type of the database context to use, must derive from <see cref="DbContext"/>.
		/// </param>
		/// <param name="serviceProvider">
		/// Optional service provider used to retrieve instances of <paramref name="dbContextType"/> context.<br/>
		/// Can be overridden during mapping with <see cref="MatcherOverrideMappingOptions.ServiceProvider"/>.
		/// </param>
		public EntityFrameworkCoreMatcher(
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
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
		}


		public bool CanMatch(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return CanMatchInternal(sourceType, destinationType, mappingOptions, out _, out _);
		}

		public bool Match(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			destination,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			using(var factory = MatchFactory(sourceType, destinationType, mappingOptions)) {
				return factory.Invoke(source, destination);
			}
		}
		public IMatchMapFactory MatchFactory(
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

			if(!CanMatchInternal(sourceType, destinationType, mappingOptions, out var keyType, out var entityType))
				throw new MapNotFoundException((sourceType, destinationType));

			// DEV: convert to use EntityFrameworkProjector?

			// Check if we are matching an entity with its key,
			// or two entities
			if (keyType != null) {
				// Create and cache the delegate for the map if needed
				var entityKeyComparer = _entityKeyComparerCache.GetOrAdd(entityType, _ => {
					var entityParam = Expression.Parameter(typeof(object), "entity");
					var keyParam = Expression.Parameter(typeof(object), "key");
					var dbContextSemaphoreParam = Expression.Parameter(typeof(SemaphoreSlim), "dbContextSemaphore");
					var dbContextParam = Expression.Parameter(typeof(DbContext), "dbContext");

					var entityEntryVar = Expression.Variable(typeof(EntityEntry), "entityEntry");

					var keyParamType = keyType.IsCompositeKeyType() ?
						TupleUtils.GetValueTupleConstructor(keyType.UnwrapNullable().GetGenericArguments()).DeclaringType :
						keyType.UnwrapNullable();
					var modelEntity = _model.GetEntityTypes().First(e => e.ClrType == entityType);
					var key = modelEntity.FindPrimaryKey();
					var keyProperties = key.Properties.Where(p => !p.GetContainingForeignKeys().Any(k => k.IsOwnership));

					var properties = keyProperties.Select((p, i) => {
							if (p.IsShadowProperty()) {
								// (KeyItemType)entityEntry.Property("Key1").CurrentValue
								return (Expression)Expression.Convert(
									Expression.Property(
										Expression.Call(
											entityEntryVar,
											EfCoreUtils.EntityEntry_Property,
											Expression.Constant(p.Name)),
										EfCoreUtils.MemberEntry_CurrentValue),
									p.ClrType);
							}
							else {
								// ((EntityType)entity).Key1
								return Expression.PropertyOrField(Expression.Convert(entityParam, entityType), p.Name);
							}
						});

					Expression body;
					if (properties.Count() == 1) {
						// KEYPROP == (KeyType)key
						body = Expression.Equal(properties.Single(), Expression.Convert(keyParam, keyParamType));
					}
					else {
						// KEYPROP1 == ((KeyType)key).Item1 && ...
						body = properties
							.Select((p, i) => Expression.Equal(p, Expression.PropertyOrField(Expression.Convert(keyParam, keyParamType), "Item" + (i + 1))))
							.Aggregate(Expression.AndAlso);
					}

					// If we have a shadow key we must retrieve values from DbContext, so we must use a semaphore (if not already inside one),
					// also we wrap the access to dbContext in a try/catch block to throw map not found if the context is disposed
					if (_entityShadowKeyCache.GetOrAdd(entityType, __ => keyProperties.Any(p => p.IsShadowProperty()))) {
						var catchExceptionParam = Expression.Parameter(typeof(Exception), "e");

						body = Expression.Block(typeof(bool),
							// if(dbContextSemaphore != null)
							//     dbContextSemaphore.Wait()
							Expression.IfThen(
								Expression.NotEqual(dbContextSemaphoreParam, Expression.Constant(null, dbContextSemaphoreParam.Type)),
								Expression.Call(dbContextSemaphoreParam, EfCoreUtils.SemaphoreSlim_Wait)),
							Expression.TryFinally(
								Expression.Block(typeof(bool), new[] { entityEntryVar },
									// var entityEntry = dbContext.Entry(entity)
									Expression.Assign(
										entityEntryVar,
										Expression.Call(dbContextParam, EfCoreUtils.DbContext_Entry, entityParam)),
									// entityEntry.State == EntityState.Detached ? throw ... : ...
									Expression.Condition(
										Expression.Equal(Expression.Property(entityEntryVar, EfCoreUtils.EntityEntry_State), Expression.Constant(EntityState.Detached)),
										Expression.Throw(
											Expression.New(
												typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) }),
												Expression.Constant($"The entity of type {entityType.FullName ?? entityType.Name} is not being tracked " +
													$"by the provided {nameof(DbContext)}, so its shadow key(s) cannot be retrieved locally. " +
													$"Either provide a valid {nameof(DbContext)} or pass a tracked entity.")),
											body.Type),
										body)
								),
								// if(dbContextSemaphore != null)
								//     dbContextSemaphore.Release()
								Expression.IfThen(
									Expression.NotEqual(dbContextSemaphoreParam, Expression.Constant(null, dbContextSemaphoreParam.Type)),
									Expression.Call(dbContextSemaphoreParam, EfCoreUtils.SemaphoreSlim_Release)))
							);
					}

					return Expression.Lambda<Func<object, object, SemaphoreSlim, DbContext, bool>>(body, entityParam, keyParam, dbContextSemaphoreParam, dbContextParam).Compile();
				});
				
				// If the key is a tuple we convert it to a value tuple, because maps are with value tuples only
				var tupleToValueTupleDelegate = EfCoreUtils.GetOrCreateTupleToValueTupleDelegate(keyType);

				// Retrieve DbContext and semaphore if dealing with shadow keys (semaphore will be retrieved
				// and used only if we are not already inside a semaphore lock)
				DbContext dbContext;
				SemaphoreSlim dbContextSemaphore;
				if (_entityShadowKeyCache.TryGetValue(entityType, out var shadowKey) && shadowKey) {
					dbContext = RetrieveDbContext(mappingOptions) ?? throw new MapNotFoundException((sourceType, destinationType));
					dbContextSemaphore = mappingOptions?.GetOptions<NestedSemaphoreContext>() == null ? EfCoreUtils.GetOrCreateSemaphoreForDbContext(dbContext) : null;
				}
				else {
					dbContext = null;
					dbContextSemaphore = null;
				}

				return new DefaultMatchMapFactory(
					sourceType, destinationType,
					(source, destination) => {
						NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));
						NeatMapper.TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

						if (source == null || destination == null)
							return false;

						object keyObject;
						object entityObject;
						if(sourceType == keyType) {
							keyObject = source;
							entityObject = destination;
						}
						else {
							keyObject = destination;
							entityObject = source;
						}

						// Convert the tuple if needed
						if (tupleToValueTupleDelegate != null) 
							keyObject = tupleToValueTupleDelegate.Invoke(keyObject);

						try { 
							return entityKeyComparer.Invoke(entityObject, keyObject, dbContextSemaphore, dbContext);
						}
						catch (OperationCanceledException) {
							throw;
						}
						catch (Exception e) {
							throw new MatcherException(e, (sourceType, destinationType));
						}
					});
			}
			else {
				// Create and cache the delegate if needed
				var entityEntityComparer = _entityEntityComparerCache.GetOrAdd(sourceType, type => {
					var entity1Param = Expression.Parameter(typeof(object), "entity1");
					var entity2Param = Expression.Parameter(typeof(object), "entity2");
					var dbContextSemaphoreParam = Expression.Parameter(typeof(SemaphoreSlim), "dbContextSemaphore");
					var dbContextParam = Expression.Parameter(typeof(DbContext), "dbContext");

					var entityEntry1Var = Expression.Variable(typeof(EntityEntry), "entityEntry1");
					var entityEntry2Var = Expression.Variable(typeof(EntityEntry), "entityEntry2");

					var modelEntity = _model.GetEntityTypes().First(e => e.ClrType == entityType);
					var key = modelEntity.FindPrimaryKey();

					var keyProperties = key.Properties.Where(p => !p.GetContainingForeignKeys().Any(k => k.IsOwnership));

					Expression body = keyProperties
						.Select(p => {
							if (p.IsShadowProperty()) {
								// (KeyItemType)entityEntry1.Property("Key1").CurrentValue == (KeyItemType)entityEntry2.Property("Key1").CurrentValue
								return Expression.Equal(
									Expression.Convert(
										Expression.Property(
											Expression.Call(
												entityEntry1Var,
												EfCoreUtils.EntityEntry_Property,
												Expression.Constant(p.Name)),
											EfCoreUtils.MemberEntry_CurrentValue),
										p.ClrType),
									Expression.Convert(
										Expression.Property(
											Expression.Call(
												entityEntry2Var,
												EfCoreUtils.EntityEntry_Property,
												Expression.Constant(p.Name)),
											EfCoreUtils.MemberEntry_CurrentValue),
										p.ClrType));
							}
							else if(p.PropertyInfo != null){
								// ((EntityType)entity1).Key1 == ((EntityType)entity2).Key1
								return Expression.Equal(
									Expression.Property(Expression.Convert(entity1Param, type), p.PropertyInfo),
									Expression.Property(Expression.Convert(entity2Param, type), p.PropertyInfo));
							}
							else{
								// ((EntityType)entity1)._key1 == ((EntityType)entity2)._key1
								return Expression.Equal(
									Expression.Field(Expression.Convert(entity1Param, type), p.FieldInfo),
									Expression.Field(Expression.Convert(entity2Param, type), p.FieldInfo));
							}
						})
						.Aggregate(Expression.AndAlso);

					// If we have a shadow key we must retrieve values from DbContext, so we must use a semaphore
					if (_entityShadowKeyCache.GetOrAdd(sourceType, __ => keyProperties.Any(p => p.IsShadowProperty()))) {
						body = Expression.Block(typeof(bool),
							// if(dbContextSemaphore != null)
							//     dbContextSemaphore.Wait()
							Expression.IfThen(
								Expression.NotEqual(dbContextSemaphoreParam, Expression.Constant(null, dbContextSemaphoreParam.Type)),
								Expression.Call(dbContextSemaphoreParam, EfCoreUtils.SemaphoreSlim_Wait)),
							Expression.TryFinally(
								Expression.Block(typeof(bool), new[] { entityEntry1Var, entityEntry2Var },
									// var entityEntry1 = dbContext.Entry(entity1)
									Expression.Assign(
										entityEntry1Var,
										Expression.Call(dbContextParam, EfCoreUtils.DbContext_Entry, entity1Param)),
									// var entityEntry2 = dbContext.Entry(entity2)
									Expression.Assign(
										entityEntry2Var,
										Expression.Call(dbContextParam, EfCoreUtils.DbContext_Entry, entity2Param)),

									// (entityEntry1.State == EntityState.Detached || entityEntry2.State == EntityState.Detached) ? throw ... : ...
									Expression.Condition(
										Expression.OrElse(
											Expression.Equal(Expression.Property(entityEntry1Var, EfCoreUtils.EntityEntry_State), Expression.Constant(EntityState.Detached)),
											Expression.Equal(Expression.Property(entityEntry2Var, EfCoreUtils.EntityEntry_State), Expression.Constant(EntityState.Detached))),
										Expression.Throw(
											Expression.New(
												typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) }),
												Expression.Constant($"The entity(ies) of type {sourceType.FullName ?? sourceType.Name} is/are not being tracked " +
													$"by the provided {nameof(DbContext)}, so its/their shadow key(s) cannot be retrieved locally. " +
													$"Either provide a valid {nameof(DbContext)} or pass a tracked entity(ies).")),
											body.Type),
										body)
								),
								// if(dbContextSemaphore != null)
								//     dbContextSemaphore.Release()
								Expression.IfThen(
									Expression.NotEqual(dbContextSemaphoreParam, Expression.Constant(null, dbContextSemaphoreParam.Type)),
									Expression.Call(dbContextSemaphoreParam, EfCoreUtils.SemaphoreSlim_Release)))
						);
					}

					return Expression.Lambda<Func<object, object, SemaphoreSlim, DbContext, bool>>(body, entity1Param, entity2Param, dbContextSemaphoreParam, dbContextParam).Compile();
				});

				// Retrieve DbContext and semaphore if dealing with shadow keys
				DbContext dbContext;
				SemaphoreSlim dbContextSemaphore;
				if (_entityShadowKeyCache.TryGetValue(sourceType, out var shadowKey) && shadowKey) {
					dbContext = RetrieveDbContext(mappingOptions) ?? throw new MapNotFoundException((sourceType, destinationType));
					dbContextSemaphore = mappingOptions?.GetOptions<NestedSemaphoreContext>() == null ? EfCoreUtils.GetOrCreateSemaphoreForDbContext(dbContext) : null;
				}
				else {
					dbContext = null;
					dbContextSemaphore = null;
				}

				return new DefaultMatchMapFactory(
					sourceType, destinationType,
					(source, destination) => {
						NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));
						NeatMapper.TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

						if (source == null || destination == null)
							return false;

						try { 
							return entityEntityComparer.Invoke(source, destination, dbContextSemaphore, dbContext);
						}
						catch (MapNotFoundException e) {
							if (e.From == sourceType && e.To == destinationType)
								throw;
							else
								throw new MappingException(e, (sourceType, destinationType));
						}
						catch (OperationCanceledException) {
							throw;
						}
						catch (Exception e) {
							throw new MatcherException(e, (sourceType, destinationType));
						}
					});
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}



#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
		bool CanMatchInternal(
			Type sourceType,
			Type destinationType,
			MappingOptions mappingOptions,
			out Type keyType,
			out Type entityType) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Check which type is the key and which is the entity, since it can be used both ways,
			// we could also match two entities
			if (sourceType == destinationType) {
				keyType = null;
				entityType = sourceType;
			}
			else if (sourceType.IsKeyType() || sourceType.IsCompositeKeyType()) {
				keyType = sourceType;
				entityType = destinationType;
			}
			else if (destinationType.IsKeyType() || destinationType.IsCompositeKeyType()) {
				keyType = destinationType;
				entityType = sourceType;
			}
			else { 
				keyType = null;
				entityType = null;
				return false;
			}

			if (!entityType.IsClass)
				return false;

			// Check if the entity is in the model
			// If a type is mapped to multiple entities (currently owned ones),
			// all the types should have the same key configuration (excluding the parent foreign keys)
			var type = entityType;
			var modelEntities = _model.GetEntityTypes()
				.Where(e => e.ClrType == type);
			if(!modelEntities.Any() ||
				modelEntities
					.Select(e => {
						var eKey = e.FindPrimaryKey();
						if(eKey == null)
							return null;
						else { 
							return string.Join("~", eKey.Properties
								.Where(p => !p.GetContainingForeignKeys().Any(k => k.IsOwnership))
								.Select(p => $"{(p.IsShadowProperty() ? "s" : "n")}{p.Name}-{p.ClrType.FullName}"));
						}
					})
					.Distinct()
					.Where(k => !string.IsNullOrEmpty(k))
					.Count() != 1) {

				return false;
			}

			// Check that the entity has a key and that it matches the key type
			// For owned entities foreign keys are excluded
			var key = modelEntities.First().FindPrimaryKey();
			if (key == null)
				return false;

			var keyProperties = key.Properties
				.Where(p => !p.GetContainingForeignKeys().Any(k => k.IsOwnership));
			if (!keyProperties.Any())
				return false;

			var keyPropertiesCount = keyProperties.Count();
			if (keyType != null) {
				if (keyType.IsCompositeKeyType()) {
					var keyTypes = keyType.UnwrapNullable().GetGenericArguments();
					if (keyPropertiesCount != keyTypes.Length || !keyTypes.Zip(keyProperties, (k1, k2) => (k1, k2.ClrType)).All(keys => keys.Item1 == keys.Item2))
						return false;
				}
				else if (keyPropertiesCount != 1 || keyProperties.First().ClrType != keyType.UnwrapNullable())
					return false;
			}

			// If the key has shadow properties we need a DbContext to get the values
			if (keyProperties.Any(p => p.IsShadowProperty()) && RetrieveDbContext(mappingOptions) == null)
				return false;

			return true;
		}

		// Retrieves the DbContext if available, may return null
		private DbContext RetrieveDbContext(MappingOptions mappingOptions) {
			var dbContext = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>()?.DbContextInstance;
			if (dbContext != null && dbContext.GetType() != _dbContextType)
				dbContext = null;

			if (dbContext == null) {
				dbContext = (mappingOptions?.GetOptions<MatcherOverrideMappingOptions>()?.ServiceProvider ?? _serviceProvider)
					.GetService(_dbContextType) as DbContext;
			}

			return dbContext;
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
