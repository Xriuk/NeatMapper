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
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

		protected static LambdaExpression GetEntitiesPredicate(Type keyType, Type entityType, IKey key, IEnumerable<object[]> keysValues) {
			var param = Expression.Parameter(entityType, "entity");
			if (key.Properties.Count == 1) {
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

			var tupleToValueTuple = EfCoreUtils.GetOrCreateTupleToValueTupleDelegate(keyType);

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
		private readonly ConcurrentDictionary<Type, Func<object[], DbContext, object>> _createEntityCache =
			new ConcurrentDictionary<Type, Func<object[], DbContext, object>>();


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


		protected abstract bool CheckCollectionMapperNestedContextRecursive(MappingOptions mappingOptions);

		protected bool CanMapTypesInternal((Type Key, Type Entity) elementTypes) {
			// We can only map from key to entity so we check source and destination types
			if (!elementTypes.Key.IsKeyType() && !elementTypes.Key.IsCompositeKeyType())
				return false;

			if (!elementTypes.Entity.IsClass)
				return false;

			// Check if the entity is in the model
			var modelEntity = _model.FindEntityType(elementTypes.Entity);
			if (modelEntity == null || modelEntity.IsOwned())
				return false;

			// Check that the entity has a key and that it matches the key type
			var key = modelEntity.FindPrimaryKey();
			if (key == null || key.Properties.Count < 1)
				return false;
			if (elementTypes.Key.IsCompositeKeyType()) {
				var keyTypes = elementTypes.Key.UnwrapNullable().GetGenericArguments();
				if (key.Properties.Count != keyTypes.Length || !keyTypes.Zip(key.Properties, (k1, k2) => (k1, k2.ClrType)).All(keys => keys.Item1 == keys.Item2))
					return false;
			}
			else if (key.Properties.Count != 1 || (key.Properties[0].ClrType != elementTypes.Key && !elementTypes.Key.IsNullable(key.Properties[0].ClrType)))
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

		protected Func<object[], DbContext, object> GetOrCreateAttachEntityDelegate(Type entityType, IKey key) {
			return _createEntityCache.GetOrAdd(entityType, type => {
				var keyValuesParam = Expression.Parameter(typeof(object[]), "keyValues");
				var dbContextParam = Expression.Parameter(typeof(DbContext), "dbContext");

				// new Type{ Prop1 = (Type1)key[0], Prop2 = (Type2)key[1], ... }
				Expression body = Expression.MemberInit(
					Expression.New(type),
					key.Properties.Select((kp, i) => (kp, i))
						.Where(kpi => !kpi.kp.IsShadowProperty())
						.Select(kpi => Expression.Bind(kpi.kp.PropertyInfo, Expression.Convert(Expression.ArrayIndex(keyValuesParam, Expression.Constant(kpi.i)), kpi.kp.ClrType))));

				var entityVar = Expression.Variable(typeof(object), "entity");
				var entityEntryVar = Expression.Variable(typeof(EntityEntry), "entityEntry");
				var blockExprs = new List<Expression>() {
					// var entityEntry = dbContext.Entry(entity)
					Expression.Assign(
						entityEntryVar,
						Expression.Call(dbContextParam, EfCoreUtils.DbContext_Entry, entityVar)),

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
					new[] { entityVar },
					// entity = new ...
					Expression.Assign(entityVar, body),
					Expression.Block(new[] { entityEntryVar }, blockExprs),
					entityVar
				);

				return Expression.Lambda<Func<object[], DbContext, object>> (body, keyValuesParam, dbContextParam).Compile();
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

			var tupleToValueTupleDelegate = EfCoreUtils.GetOrCreateTupleToValueTupleDelegate(types.From);
			var keyValuesDelegate = GetOrCreateKeyToValuesDelegate(types.From);

			var attachEntityDelegate = GetOrCreateAttachEntityDelegate(types.To, key);

			var dbContextSemaphore = dbContext != null ? EfCoreUtils.GetOrCreateSemaphoreForDbContext(dbContext) : null;

			// Create the matcher (it will never throw because of SafeMatcher/EmptyMatcher), may contain semaphore
			var elementsMatcherFactory = GetMatcher(mappingOptions).MatchFactory(types.From, types.To, mappingOptions);

			return new DisposableMergeCollectionFactory(
				(destinationEnumerable, sourceEnumerable, sourceEntitiesEnumerable) => {
					// DEV: use pool?
					var elementsToAdd = new List<object>();
					var elementsToRemove = new List<object>();

					try {
						// Deleted elements
						var matchedDestinations = removeNotMatchedDestinationElements ?
							new List<object>() :
							null;

						if(entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach && dbContext != null)
							dbContextSemaphore.Wait();
						try {
							// Added/updated elements, also attach entities if needed
							foreach (var sourceElement in sourceEnumerable) {
								// Retrieve matching destination element
								bool destinationFound = false;
								object matchingDestinationElement = null;
								foreach (var destinationElement in destinationEnumerable.Cast<object>().Concat(elementsToAdd)) {
									if (elementsMatcherFactory.Invoke(sourceElement, destinationElement) &&
										!elementsToRemove.Contains(destinationElement)) {

										matchingDestinationElement = destinationElement;
										matchedDestinations?.Add(matchingDestinationElement);
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
											if(dbContext != null) { 
												if (matchingDestinationElement != null) 
													dbContext.Attach(matchingDestinationElement);
												else {
													var keyValues = keyValuesDelegate.Invoke(tupleToValueTupleDelegate != null ?
														tupleToValueTupleDelegate.Invoke(sourceElement) :
														sourceElement);

													matchingSourceEntityElement = attachEntityDelegate.Invoke(keyValues, dbContext);

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

										matchingSourceEntityElement = attachEntityDelegate.Invoke(keyValues, dbContext);
									}
									elementsToAdd.Add(matchingSourceEntityElement);
								}
							}
						}
						finally {
							if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach && dbContext != null)
								dbContextSemaphore.Release();
						}

						// Deleted elements
						if (removeNotMatchedDestinationElements)
							elementsToRemove.AddRange(destinationEnumerable.Cast<object>().Except(matchedDestinations));

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IMatcher GetMatcher(MappingOptions mappingOptions) {
			var mergeMappingOptions = mappingOptions?.GetOptions<MergeCollectionsMappingOptions>();
			if (mergeMappingOptions?.Matcher != null && mergeMappingOptions.Matcher != _elementsMatcher) {
				// Creating a CompositeMatcher because the provided matcher just overrides any maps in _elementsMatcher
				// so all the others should be available
				var options = new CompositeMatcherOptions();
				options.Matchers.Add(mergeMappingOptions.Matcher);
				options.Matchers.Add(_elementsMatcher);
				return new SafeMatcher(new CompositeMatcher(options));
			}
			else
				return _elementsMatcher;
		}

		/// <summary>
		/// Normalizes <see cref="Tuple"/> key types to <see cref="ValueTuple"/>
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected IMatchMapFactory GetNormalizedMatchFactory((Type Key, Type Entity) types, MappingOptions mappingOptions) {
			return GetMatcher(mappingOptions).MatchFactory(types.Key.TupleToValueTuple(), types.Entity, mappingOptions);
		}
	}
}
