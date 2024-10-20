#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
// For IServiceProviderIsService
#if !NET5_0 && !NETCOREAPP3_1
using Microsoft.Extensions.DependencyInjection;
#endif
using System.Collections.ObjectModel;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// Base class for Entity Framework Core mappers.
	/// Internal class.
	/// </summary>
	public abstract class EntityFrameworkCoreBaseMapper {
		internal sealed class DisposableMergeCollectionFactory : IDisposable {
			private int _disposed = 0;
			private readonly Action<IEnumerable, IEnumerable, IEnumerable> _mapDelegate;
			private readonly IDisposable[] _disposables;

			public DisposableMergeCollectionFactory(Action<IEnumerable, IEnumerable, IEnumerable> mapDelegate, params IDisposable[] disposables) {
				_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
				_disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
			}


			public void Invoke(IEnumerable destination, IEnumerable source, IEnumerable sourceEntities) {
				if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
					throw new ObjectDisposedException(null);
				_mapDelegate.Invoke(destination, source, sourceEntities);
			}

			public void Dispose() {
				lock (_disposables) {
					if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0) { 
						foreach (var disposable in _disposables) {
							disposable?.Dispose();
						}
					}
				}
			}
		}

		internal sealed class EntityMappingInfo {
			/// <summary>
			/// Normalized key of the entity, primitive type or <see cref="ValueTuple"/>.
			/// </summary>
			public object Key { get; set; }

			public object LocalEntity { get; set; }
		}

		protected delegate void AttachEntityDelegate(ref object entity, object[] keyValues, SemaphoreSlim dbContextSemaphore, DbContext dbContext);


		/// <summary>
		/// <see cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)"/>
		/// </summary>
		private static readonly MethodInfo Enumerable_Contains = typeof(Enumerable).GetMethods()
			.First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

		/// <summary>
		/// <see cref="DbContext.Set{TEntity}()"/>
		/// </summary>
		private static readonly MethodInfo DbContext_Set = typeof(DbContext).GetMethods()
			.First(m => m.IsGenericMethod && m.Name == nameof(DbContext.Set) && m.GetParameters().Length == 0);

		/// <summary>
		/// Delegates which retrieve a new <see cref="DbSet{TEntity}"/>.
		/// </summary>
		private static readonly ConcurrentDictionary<Type, Func<DbContext, IQueryable>> _setCache =
			new ConcurrentDictionary<Type, Func<DbContext, IQueryable>>();

		/// <summary>
		/// Delegates which retrieve <see cref="DbSet{TEntity}.Local"/> from a <see cref="DbSet{TEntity}"/>.
		/// </summary>
		private static readonly ConcurrentDictionary<Type, Func<IQueryable, IEnumerable>> _localCache =
			new ConcurrentDictionary<Type, Func<IQueryable, IEnumerable>>();


		protected static IQueryable RetrieveDbSet(DbContext context, Type type) {
			return _setCache.GetOrAdd(type, t => {
				var param = Expression.Parameter(typeof(DbContext), "dbContext");
				// dbContext.Set<Type>()
				var body = Expression.Call(param, DbContext_Set.MakeGenericMethod(t));
				return Expression.Lambda<Func<DbContext, IQueryable>>(body, param).Compile();
			}).Invoke(context);
		}

		protected static IEnumerable GetLocalFromDbSet(IQueryable dbSet) {
			return _localCache.GetOrAdd(dbSet.ElementType, t => {
				var param = Expression.Parameter(typeof(IQueryable), "dbSet");
				var prop = typeof(DbSet<>).MakeGenericType(t).GetProperty(nameof(DbSet<object>.Local))
					?? throw new InvalidOperationException("Cannot retrieve DbSet<T>.Local");
				// dbSet.Local
				var body = Expression.Property(Expression.Convert(param, typeof(DbSet<>).MakeGenericType(t)), prop);
				return Expression.Lambda<Func<IQueryable, IEnumerable>>(body, param).Compile();
			}).Invoke(dbSet);
		}


		/// <summary>
		/// Db model, shared between instances of the same DbContext type.
		/// </summary>
		protected readonly IModel _model;

		/// <summary>
		/// Type of DbContext to retrieve from <see cref="_serviceProvider"/>.
		/// </summary>
		protected readonly Type _dbContextType;

		/// <summary>
		/// Service provider used to retrieve <see cref="DbContext"/> instances.
		/// </summary>
		protected readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Options to apply when retrieving entitites.
		/// </summary>
		protected readonly EntityFrameworkCoreOptions _entityFrameworkCoreOptions;

		/// <summary>
		/// <see cref="IMatcher"/> which is used to match source elements with destination elements
		/// to try merging them together.
		/// </summary>
		private readonly IMatcher _elementsMatcher;

		/// <summary>
		/// Options to apply when merging elements in the collections.
		/// </summary>
		private readonly MergeCollectionsOptions _mergeCollectionOptions;

		/// <summary>
		/// Delegates which extract values from a key (single or composite), keys are entity key types.
		/// Composite keys are <see cref="ValueTuple"/>s, so <see cref="Tuple"/>s should be converted with
		/// the corresponding <see cref="EfCoreUtils.GetOrCreateTupleToValueTupleDelegate(Type)"/> delegate first.
		/// </summary>
		private readonly ConcurrentDictionary<Type, Func<object, object[]>> _keyToValuesCache =
			new ConcurrentDictionary<Type, Func<object, object[]>>();

		/// <summary>
		/// Delegates which create a new entity and assign key values to corresponding properties, keys are entity types.
		/// </summary>
		private readonly ConcurrentDictionary<Type, AttachEntityDelegate> _createEntityCache =
			new ConcurrentDictionary<Type, AttachEntityDelegate>();


		internal protected EntityFrameworkCoreBaseMapper(
			IModel model,
			Type dbContextType,
			IServiceProvider serviceProvider,
			EntityFrameworkCoreOptions entityFrameworkCoreOptions = null,
			IMatcher elementsMatcher = null,
			MergeCollectionsOptions mergeCollectionsOptions = null) {

			_model = model ?? throw new ArgumentNullException(nameof(model));
			_dbContextType = dbContextType ?? throw new ArgumentNullException(nameof(dbContextType));
			if (!typeof(DbContext).IsAssignableFrom(_dbContextType))
				throw new ArgumentException($"Type {_dbContextType.FullName ?? _dbContextType.Name} is not derived from DbContext", nameof(dbContextType));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_entityFrameworkCoreOptions = entityFrameworkCoreOptions ?? new EntityFrameworkCoreOptions();
			_elementsMatcher = elementsMatcher != null ? new SafeMatcher(elementsMatcher) : EmptyMatcher.Instance;
			_mergeCollectionOptions = mergeCollectionsOptions ?? new MergeCollectionsOptions();

#if !NET5_0 && !NETCOREAPP3_1
			if (_serviceProvider.GetService<IServiceProviderIsService>()?.IsService(dbContextType) == false)
				throw new ArgumentException($"The provided IServiceProvider does not support the DbContext of type {_dbContextType.FullName ?? _dbContextType.Name}", nameof(serviceProvider));
#endif
		}


		protected bool CanMapMerge(
			Type sourceType,
			Type destinationType,
			IEnumerable destination = null,
			MappingOptions mappingOptions = null) {

			// Prevent being used by a collection mapper
			if (CheckCollectionMapperNestedContextRecursive(mappingOptions))
				return false;

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) elementTypes;
			if (sourceType.IsEnumerable() && sourceType != typeof(string) && destinationType.IsCollection() && !destinationType.IsArray) {
				if (!ObjectFactory.CanCreateCollection(destinationType))
					return false;

				// If the destination type is not an interface, check if it is not readonly
				// If the destination type is not an interface, check if it is not readonly
				// Otherwise check the destination if provided
				if (!destinationType.IsInterface && destinationType.IsGenericType) {
					var collectionDefinition = destinationType.GetGenericTypeDefinition();
					if (collectionDefinition == typeof(ReadOnlyCollection<>) ||
						collectionDefinition == typeof(ReadOnlyDictionary<,>) ||
						collectionDefinition == typeof(ReadOnlyObservableCollection<>)) {

						return false;
					}
				}
				else if (destination != null && NeatMapper.TypeUtils.IsCollectionReadonly(destination)) 
					return false;

				elementTypes = (sourceType.GetEnumerableElementType(), destinationType.GetCollectionElementType());
			}
			else
				elementTypes = (sourceType, destinationType);

			// We can only map from key to entity so we check source and destination types
			if (!elementTypes.From.IsKeyType() && !elementTypes.From.IsCompositeKeyType())
				return false;

			if (CanMap(elementTypes.To, elementTypes.From)) {
				if (sourceType.IsEnumerable() && sourceType != typeof(string) && destinationType.IsCollection() && !destinationType.IsArray) {
					if (!destinationType.IsInterface || destination != null)
						return true;

					throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
				}
				else
					return true;
			}
			else
				return false;
		}

		protected abstract bool CheckCollectionMapperNestedContextRecursive(MappingOptions mappingOptions);

		protected bool CanMap(Type entityType, Type keyType) {
			if (!entityType.IsClass)
				return false;

			// Check if the entity is in the model
			var modelEntity = _model.FindEntityType(entityType);
			if (modelEntity == null || modelEntity.IsOwned())
				return false;

			// Check that the entity has a key and that it matches the key type
			var key = modelEntity.FindPrimaryKey();
			if (key == null || key.Properties.Count < 1)
				return false;
			if (keyType.IsCompositeKeyType()) {
				var keyTypes = keyType.UnwrapNullable().GetGenericArguments();
				if (key.Properties.Count != keyTypes.Length || !keyTypes.Zip(key.Properties, (k1, k2) => (k1, k2.ClrType)).All(keys => keys.Item1 == keys.Item2))
					return false;
			}
			else if (key.Properties.Count != 1 || (key.Properties[0].ClrType != keyType && !keyType.IsNullable(key.Properties[0].ClrType)))
				return false;

			return true;
		}

		// DEV: consider using ArrayPool<T>
		protected Func<object, object[]> GetOrCreateKeyToValuesDelegate(Type key) {
			return _keyToValuesCache.GetOrAdd(key, keyType => {
				var keyParam = Expression.Parameter(typeof(object), "key");
				Expression body;
				if (!keyType.IsCompositeKeyType()) {
					// new object[]{ key }
					body = Expression.NewArrayInit(typeof(object), keyParam);
				}
				else {
					var tupleType = TupleUtils.GetValueTupleConstructor(keyType.UnwrapNullable().GetGenericArguments()).DeclaringType;

					// new object[]{ (object)((ValueTuple<...>)key).Item1, ... }
					body = Expression.NewArrayInit(typeof(object),
						Enumerable.Range(1, tupleType.GetGenericArguments().Length)
							.Select(n => Expression.Convert(Expression.PropertyOrField(Expression.Convert(keyParam, tupleType), "Item" + n), typeof(object))));
				}

				return Expression.Lambda<Func<object, object[]>>(body, keyParam).Compile();
			});
		}

		protected AttachEntityDelegate GetOrCreateAttachEntityDelegate(Type entityType, IKey key) {
			return _createEntityCache.GetOrAdd(entityType, type => {
				var entityParam = Expression.Parameter(typeof(object).MakeByRefType(), "entity");
				var keyValuesParam = Expression.Parameter(typeof(object[]), "keyValues");
				var dbContextSemaphoreParam = Expression.Parameter(typeof(SemaphoreSlim), "dbContextSemaphore");
				var dbContextParam = Expression.Parameter(typeof(DbContext), "dbContext");

				// new Type{ Prop1 = (Type1)key[0], Prop2 = (Type2)key[1], ... }
				Expression body = Expression.MemberInit(
					Expression.New(type),
					key.Properties.Select((kp, i) => (kp, i))
						.Where(kpi => !kpi.kp.IsShadowProperty())
						.Select(kpi => Expression.Bind(kpi.kp.PropertyInfo, Expression.Convert(Expression.ArrayIndex(keyValuesParam, Expression.Constant(kpi.i)), kpi.kp.ClrType))));

				var entityEntryVar = Expression.Variable(typeof(EntityEntry), "entityEntry");
				var blockExprs = new List<Expression>() {
					// var entityEntry = dbContext.Entry(entity)
					Expression.Assign(
						entityEntryVar,
						Expression.Call(dbContextParam, EfCoreUtils.DbContext_Entry, entityParam)),

					// entityEntry.State = EntityState.Unchanged
					Expression.Assign(
						Expression.Property(entityEntryVar, EfCoreUtils.EntityEntry_State),
						Expression.Constant(EntityState.Unchanged))
				};

				if (key.Properties.Any(p => p.IsShadowProperty())) {
					// entityEntry.Property("Prop1").CurrentValue = key[0]
					// ...
					blockExprs.AddRange(key.Properties.Select((kp, i) => (kp, i))
						.Where(kpi => kpi.kp.IsShadowProperty())
						.Select(kpi => Expression.Assign(
							Expression.Property(
								Expression.Call(
									entityEntryVar,
									EfCoreUtils.EntityEntry_Property,
									Expression.Constant(kpi.kp.Name)),
								EfCoreUtils.MemberEntry_CurrentValue),
							Expression.ArrayIndex(keyValuesParam, Expression.Constant(kpi.i))))
					);

					// Reset the state again after changing the shadow keys
					// entityEntry.State = EntityState.Unchanged
					blockExprs.Add(Expression.Assign(Expression.Property(entityEntryVar, EfCoreUtils.EntityEntry_State), Expression.Constant(EntityState.Unchanged)));
				}

				body = Expression.Block(
					// entity = new ...
					Expression.Assign(entityParam, body),

					// dbContextSemaphore.Wait()
					Expression.Call(dbContextSemaphoreParam, EfCoreUtils.SemaphoreSlim_Wait),
					Expression.TryFinally(
						Expression.Block(new[] { entityEntryVar }, blockExprs),
						// dbContextSemaphore.Release()
						Expression.Call(dbContextSemaphoreParam, EfCoreUtils.SemaphoreSlim_Release))
				);

				return Expression.Lambda<AttachEntityDelegate>(body, entityParam, keyValuesParam, dbContextSemaphoreParam, dbContextParam).Compile();
			});
		}

		/// <remarks>Returned factory contains semaphore if dbContext is not null</remarks>
		internal DisposableMergeCollectionFactory MergeCollection(
			(Type From, Type To) types, DbContext dbContext, IKey key, EntitiesRetrievalMode entitiesRetrievalMode,
			MappingOptions mappingOptions, bool throwOnDuplicateEntity) {

			var addDelegate = ObjectFactory.GetCollectionAddDelegate(types.To);
			var removeDelegate = ObjectFactory.GetCollectionRemoveDelegate(types.To);

			var removeNotMatchedDestinationElements = mappingOptions?.GetOptions<MergeCollectionsMappingOptions>()?.RemoveNotMatchedDestinationElements
				?? _mergeCollectionOptions.RemoveNotMatchedDestinationElements;

			var tupleToValueTupleDelegate = types.From.IsTuple() ? EfCoreUtils.GetOrCreateTupleToValueTupleDelegate(types.From) : null;
			var keyValuesDelegate = GetOrCreateKeyToValuesDelegate(types.From);

			var attachEntityDelegate = GetOrCreateAttachEntityDelegate(types.To, key);

			var dbContextSemaphore = dbContext != null ? EfCoreUtils.GetOrCreateSemaphoreForDbContext(dbContext) : null;

			// Create the matcher (it will never throw because of SafeMatcher/EmptyMatcher), may contain semaphore
			var elementsMatcherFactory = RetrieveMatcher(mappingOptions).MatchFactory(types.From, types.To, mappingOptions);

			return new DisposableMergeCollectionFactory(
				(destinationEnumerable, sourceEnumerable, sourceEntitiesEnumerable) => {
					var elementsToRemove = new List<object>();
					var elementsToAdd = new List<object>();

					try {
						// Deleted elements
						if (removeNotMatchedDestinationElements) {
							foreach (var destinationElement in destinationEnumerable) {
								bool found = false;
								foreach (var sourceElement in sourceEnumerable) {
									if (elementsMatcherFactory.Invoke(sourceElement, destinationElement)) {
										found = true;
										break;
									}
								}

								if (!found)
									elementsToRemove.Add(destinationElement);
							}
						}

						// Added/updated elements
						foreach (var sourceElement in sourceEnumerable) {
							// Retrieve matching key
							bool destinationFound = false;
							object matchingDestinationElement = null;
							foreach (var destinationElement in destinationEnumerable) {
								if (elementsMatcherFactory.Invoke(sourceElement, destinationElement) &&
									!elementsToRemove.Contains(destinationElement)) {

									matchingDestinationElement = destinationElement;
									destinationFound = true;
									break;
								}
							}

							// Retrieve matching mapped element
							bool sourceEntityFound = false;
							object matchingSourceEntityElement = null;
							foreach (var sourceEntityElement in sourceEntitiesEnumerable) {
								if (elementsMatcherFactory.Invoke(sourceElement, sourceEntityElement)) {
									matchingSourceEntityElement = sourceEntityElement;
									sourceEntityFound = true;
									break;
								}
							}

							if (destinationFound) {
								if (!sourceEntityFound || matchingDestinationElement != matchingSourceEntityElement) {
									if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach && (!sourceEntityFound || matchingSourceEntityElement == null)) {
										if(dbContextSemaphore != null) { 
											if (matchingDestinationElement != null) {
												dbContextSemaphore.Wait();
												try {
													dbContext.Attach(matchingDestinationElement);
												}
												finally {
													dbContextSemaphore.Release();
												}
											}
											else {
												var keyValues = keyValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
													tupleToValueTupleDelegate.Invoke(sourceElement) :
													sourceElement);

												attachEntityDelegate.Invoke(ref matchingSourceEntityElement, keyValues, dbContextSemaphore, dbContext);

												elementsToRemove.Add(matchingDestinationElement);
												elementsToAdd.Add(matchingSourceEntityElement);
											}
										}
										else
											throw new InvalidOperationException("DbContext to attach entities not provided");
									}
									else {
										if (throwOnDuplicateEntity) {
											if (sourceEntityFound && matchingSourceEntityElement != null) {
												var keyValues = keyValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
													tupleToValueTupleDelegate.Invoke(sourceElement) :
													sourceElement);
												throw new DuplicateEntityException($"A duplicate entity of type {types.To?.FullName ?? types.To.Name} was found for the key {string.Join(", ", keyValues)}");
											}
											else
												throw new DuplicateEntityException($"A non-null entity of type {types.To?.FullName ?? types.To.Name} was provided for a not found entity. When merging objects make sure that they match");
										}
										else {
											elementsToRemove.Add(matchingDestinationElement);
											elementsToAdd.Add(matchingSourceEntityElement);
										}
									}
								}
							}
							else {
								if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach && (!sourceEntityFound || matchingSourceEntityElement == null)) {
									var keyValues = keyValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
										tupleToValueTupleDelegate.Invoke(sourceElement) :
										sourceElement);

									attachEntityDelegate.Invoke(ref matchingSourceEntityElement, keyValues, dbContextSemaphore, dbContext);
								}
								elementsToAdd.Add(matchingSourceEntityElement);
							}
						}

						// Update destination collection
						foreach (var element in elementsToRemove) {
							if (!removeDelegate.Invoke(destinationEnumerable, element))
								throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destinationEnumerable}");
						}
						foreach (var element in elementsToAdd) {
							addDelegate.Invoke(destinationEnumerable, element);
						}
					}
					catch (MapNotFoundException) {
						throw;
					}
					catch (OperationCanceledException) {
						throw;
					}
					catch (Exception e) {
						throw new MappingException(e, types);
					}
				},
				elementsMatcherFactory);
		}

		protected LambdaExpression GetEntitiesPredicate(Type keyType, Type entityType, IKey key, IEnumerable<object[]> keysValues) {
			var param = Expression.Parameter(entityType, "entity");
			if(key.Properties.Count == 1) {
				bool hasOne;
				try {
					keysValues.Single();
					hasOne = true;
				}
				catch {
					hasOne = false;
				}

				if (!hasOne) {
					var prop = key.Properties[0];
					// new []{ key1, key2, ... }.Contains(EF.Property<KeyType>(entity, "Key"))
					// new []{ key1, key2, ... }.Contains(entity.Key)
					return Expression.Lambda(
						Expression.Call(Enumerable_Contains.MakeGenericMethod(prop.ClrType),
							// DEV: check if Expression.Constant of object array is valid
							Expression.NewArrayInit(prop.ClrType, keysValues.Select(values => Expression.Constant(values[0], prop.ClrType))),
							prop.IsShadowProperty() ?
								(Expression)Expression.Call(EfCoreUtils.EF_Property.MakeGenericMethod(prop.ClrType), param, Expression.Constant(prop.Name)) :
								Expression.Property(param, prop.PropertyInfo)),
						param);
				}
			}

			var tupleToValueTuple = keyType.IsTuple() ? EfCoreUtils.GetOrCreateTupleToValueTupleDelegate(keyType) : null;

			// entity.Key1 == key1 && ...
			// EF.Property<KeyType1>(entity, "Key1") == key1 && ...
			return Expression.Lambda(
				keysValues.Select(values => key.Properties
						.Select((prop, i) => (Expression)Expression.Equal(
							prop.IsShadowProperty() ?
								(Expression)Expression.Call(EfCoreUtils.EF_Property.MakeGenericMethod(prop.ClrType), param, Expression.Constant(prop.Name)) :
								Expression.Property(param, prop.PropertyInfo),
							Expression.Constant(values[i], prop.ClrType)))
						.Aggregate(Expression.AndAlso))
					.Aggregate(Expression.OrElse),
				param);
		}

		// Elements matched factory should be provided with NestedSemaphoreContext options set
		/// <remarks>Returned factory contains semaphore</remarks>
		internal Func<object, EntityMappingInfo> RetrieveLocalAndPredicateFactory(
			Type keyType, Type entityType,
			IKey key, EntitiesRetrievalMode retrievalMode, IEnumerable localView, DbContext dbContext, IMatchMapFactory elementsMatcherFactory) {

			var tupleToValueTupleDelegate = keyType.IsTuple() ? EfCoreUtils.GetOrCreateTupleToValueTupleDelegate(keyType) : null;
			var keyToValuesDelegate = GetOrCreateKeyToValuesDelegate(keyType);

			var attachEntityDelegate = GetOrCreateAttachEntityDelegate(entityType, key);

			var dbContextSemaphore = EfCoreUtils.GetOrCreateSemaphoreForDbContext(dbContext);

			return sourceElement => {
				if (sourceElement == null || TypeUtils.IsDefaultValue(keyType.UnwrapNullable(), sourceElement))
					return new EntityMappingInfo();

				if (tupleToValueTupleDelegate != null)
					sourceElement = tupleToValueTupleDelegate.Invoke(sourceElement);
				var keyValues = keyToValuesDelegate.Invoke(sourceElement);

				object localEntity;
				if (retrievalMode != EntitiesRetrievalMode.Remote) {
					dbContextSemaphore.Wait();
					try {
						localEntity = localView
							.Cast<object>()
							.FirstOrDefault(e => elementsMatcherFactory.Invoke(sourceElement, e));
					}
					finally {
						dbContextSemaphore.Release();
					}

					if (localEntity == null && retrievalMode == EntitiesRetrievalMode.LocalOrAttach) 
						attachEntityDelegate.Invoke(ref localEntity, keyValues, dbContextSemaphore, dbContext);
				}
				else
					localEntity = null;

				return new EntityMappingInfo {
					Key = sourceElement,
					LocalEntity = localEntity
				};
			};
		}

		protected IMatcher RetrieveMatcher(MappingOptions mappingOptions) {
			var mergeMappingOptions = mappingOptions?.GetOptions<MergeCollectionsMappingOptions>();
			if (mergeMappingOptions?.Matcher != null)
				return new SafeMatcher(mergeMappingOptions.Matcher);
			else
				return _elementsMatcher;
		}

		/// <summary>
		/// Normalizes <see cref="Tuple"/> key types to <see cref="ValueTuple"/>
		/// </summary>
		protected IMatchMapFactory GetNormalizedMatchFactory((Type Key, Type Entity) types, MappingOptions mappingOptions) {
			return RetrieveMatcher(mappingOptions).MatchFactory(types.Key.IsTuple() ? TupleUtils.GetValueTupleConstructor(types.Key.GetGenericArguments()).DeclaringType : types.Key, types.Entity, mappingOptions);
		}
	}
}
