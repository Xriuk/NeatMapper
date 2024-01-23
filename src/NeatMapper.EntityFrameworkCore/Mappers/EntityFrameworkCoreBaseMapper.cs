#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Collections;
using System.Linq.Expressions;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// Base class for Entity Framework Core mappers.
	/// Internal class.
	/// </summary>
	public abstract class EntityFrameworkCoreBaseMapper {
		/// <summary>
		/// <see cref="Queryable.Where{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}})"/>
		/// </summary>
		protected static readonly MethodInfo Queryable_Where = typeof(Queryable).GetMethods().First(m => {
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


		protected readonly IModel _model;
		protected readonly Type _dbContextType;
		protected readonly IServiceProvider _serviceProvider;
		protected readonly EntityFrameworkCoreOptions _entityFrameworkCoreOptions;
		protected readonly IMatcher _elementsMatcher;
		protected readonly MergeCollectionsOptions _mergeCollectionOptions;
		// entity: (key) => [...values] (composite keys are ValueTuples)
		protected readonly IDictionary<Type, Delegate> _keyToValuesCache = new Dictionary<Type, Delegate>();
		protected readonly IDictionary<Type, Delegate> _tupleToValueTupleCache = new Dictionary<Type, Delegate>();

		internal EntityFrameworkCoreBaseMapper(
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
				throw new ArgumentException($"The provided IServiceProvider does not support the DbContext of type {_dbContextType.FullName ?? _dbContextType.Name}");
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
				else if (destination != null) {
					var destinationInstanceType = destination.GetType();
					if (destinationInstanceType.IsArray)
						return false;

					var interfaceMap = destinationInstanceType.GetInterfaceMap(destinationInstanceType.GetInterfaces()
						.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;

					// If the collection is readonly we cannot map to it
					if ((bool)interfaceMap.First(m => m.Name.EndsWith("get_" + nameof(ICollection<object>.IsReadOnly))).Invoke(destination, null))
						return false;
				}

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

		protected void AttachEntity(Type entityType, DbContext db, ref object entity, object[] keyValues, IKey key) {
			entity = ObjectFactory.Create(entityType);
			for (int i = 0; i < keyValues.Length; i++) {
				key.Properties[i].PropertyInfo?.SetValue(entity, keyValues[i]);
			}
			db.Entry(entity).State = EntityState.Unchanged;
		}

		protected Delegate GetOrCreateKeyToValuesMap(Type entityType, Type keyType) {
			lock (_keyToValuesCache) {
				if (!_keyToValuesCache.TryGetValue(entityType, out var keyToValuesMap)) {
					var keyParam = Expression.Parameter(keyType.IsCompositeKeyType() ? TupleUtils.GetValueTupleConstructor(keyType.UnwrapNullable().GetGenericArguments()).DeclaringType : keyType.UnwrapNullable(), "key");
					var key = _model.FindEntityType(entityType).FindPrimaryKey();
					Expression body;
					if (key.Properties.Count == 1) {
						// new object[]{ (object)key }
						body = Expression.NewArrayInit(typeof(object), Expression.Convert(keyParam, typeof(object)));
					}
					else {
						// new object[]{ (object)key.Item1, ... }
						body = Expression.NewArrayInit(typeof(object),
							Enumerable.Range(1, key.Properties.Count)
								.Select(n => Expression.Convert(Expression.PropertyOrField(keyParam, "Item" + n), typeof(object))));
					}

					keyToValuesMap = Expression.Lambda(typeof(Func<,>).MakeGenericType(keyParam.Type, body.Type), body, keyParam).Compile();
					_keyToValuesCache.Add(entityType, keyToValuesMap);
				}

				return keyToValuesMap;
			}
		}

		protected Delegate GetOrCreateTupleToValueTupleMap(Type entityType, Type tupleType) {
			if (!tupleType.IsTuple())
				throw new ArgumentException("Type is not a Tuple", nameof(tupleType));

			lock (_tupleToValueTupleCache) {
				if (!_tupleToValueTupleCache.TryGetValue(entityType, out var tupleToValueTuple)) {
					var keyParam = Expression.Parameter(tupleType, "key");
					// new ValueTuple<...>(key.Item1, ...)
					Expression body = Expression.New(
						TupleUtils.GetValueTupleConstructor(keyParam.Type.GetGenericArguments()),
						Enumerable.Range(1, keyParam.Type.GetGenericArguments().Count()).Select(n => Expression.Property(keyParam, "Item" + n)));
					tupleToValueTuple = Expression.Lambda(typeof(Func<,>).MakeGenericType(keyParam.Type, body.Type), body, keyParam).Compile();
					_tupleToValueTupleCache.Add(entityType, tupleToValueTuple);
				}

				return tupleToValueTuple;
			}
		}

		protected DbContext RetrieveDbContext(MappingOptions mappingOptions) {
			var db = mappingOptions.GetOptions<EntityFrameworkCoreMappingOptions>()?.DbContextInstance;
			if (db != null && db.GetType() != _dbContextType)
				db = null;
			if (db == null) {
				try {
					db = (mappingOptions?.GetOptions<MapperOverrideMappingOptions>()?.ServiceProvider ?? _serviceProvider)
						.GetRequiredService(_dbContextType) as DbContext;
				}
				catch {
					db = null;
				}
			}

			if (db == null)
				throw new InvalidOperationException($"Could not retrieve a DbContext of type {_dbContextType.FullName ?? _dbContextType.Name}");

			return db;
		}

		object[] GetKeyValues(Type entityType, Type keyType, object keyEntity) {
			var keyToValuesMap = GetOrCreateKeyToValuesMap(entityType, keyType);
			if (keyType.IsTuple())
				keyEntity = GetOrCreateTupleToValueTupleMap(entityType, keyType).DynamicInvoke(keyEntity);
			return keyToValuesMap.DynamicInvoke(keyEntity) as object[]
				?? throw new InvalidOperationException($"Invalid key(s) returned for entity of type {entityType.FullName ?? entityType.Name}");
		}

		// DEV: convert to factory
		protected void MergeCollection(
			IEnumerable destinationEnumerable, IEnumerable sourceEnumerable, IEnumerable sourceEntitiesEnumerable,
			(Type From, Type To) types, DbContext db, IKey key, EntitiesRetrievalMode entitiesRetrievalMode,
			MappingOptions mappingOptions, bool throwOnDuplicateEntity) {

			var destinationInstanceType = destinationEnumerable.GetType();
			var interfaceMap = destinationInstanceType.GetInterfaceMap(destinationInstanceType.GetInterfaces()
				.First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))).TargetMethods;

			var addMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Add)));
			var removeMethod = interfaceMap.First(m => m.Name.EndsWith(nameof(ICollection<object>.Remove)));

			var elementsToRemove = new List<object>();
			var elementsToAdd = new List<object>();

			var mergeMappingOptions = mappingOptions?.GetOptions<MergeCollectionsMappingOptions>();

			// Create the matcher
			IMatcher elementMatcher;
			if (mergeMappingOptions?.Matcher != null)
				elementMatcher = new SafeMatcher(new DelegateMatcher(mergeMappingOptions.Matcher, _elementsMatcher, _serviceProvider));
			else
				elementMatcher = _elementsMatcher;

			// Deleted elements
			if (mergeMappingOptions?.RemoveNotMatchedDestinationElements
				?? _mergeCollectionOptions.RemoveNotMatchedDestinationElements) {
				foreach (var destinationElement in destinationEnumerable) {
					bool found = false;
					foreach (var sourceElement in sourceEnumerable) {
						if (elementMatcher.Match(sourceElement, types.From, destinationElement, types.To, mappingOptions)) {
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
				bool destinationFound = false;
				object matchingDestinationElement = null;
				foreach (var destinationElement in destinationEnumerable) {
					if (elementMatcher.Match(sourceElement, types.From, destinationElement, types.To, mappingOptions) &&
						!elementsToRemove.Contains(destinationElement)) {

						matchingDestinationElement = destinationElement;
						destinationFound = true;
						break;
					}
				}

				bool sourceEntityFound = false;
				object matchingSourceEntityElement = null;
				foreach (var sourceEntityElement in sourceEntitiesEnumerable) {
					if (elementMatcher.Match(sourceElement, types.From, sourceEntityElement, types.To, mappingOptions)) {
						matchingSourceEntityElement = sourceEntityElement;
						sourceEntityFound = true;
						break;
					}
				}

				if (destinationFound) {
					if (!sourceEntityFound || matchingDestinationElement != matchingSourceEntityElement) {
						if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach && (!sourceEntityFound || matchingSourceEntityElement == null)) {
							if (matchingDestinationElement != null)
								db.Attach(matchingDestinationElement);
							else {
								AttachEntity(types.To, db, ref matchingSourceEntityElement, GetKeyValues(types.To, types.From, sourceElement), key);
								elementsToRemove.Add(matchingDestinationElement);
								elementsToAdd.Add(matchingSourceEntityElement);
							}
						}
						else {
							if (throwOnDuplicateEntity) {
								if (sourceEntityFound && matchingSourceEntityElement != null)
									throw new DuplicateEntityException($"A duplicate entity of type {types.To?.FullName ?? types.To.Name} was found for the key {string.Join(", ", GetKeyValues(types.To, types.From, sourceElement))}");
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
					if (entitiesRetrievalMode == EntitiesRetrievalMode.LocalOrAttach && (!sourceEntityFound || matchingSourceEntityElement == null))
						AttachEntity(types.To, db, ref matchingSourceEntityElement, GetKeyValues(types.To, types.From, sourceElement), key);
					elementsToAdd.Add(matchingSourceEntityElement);
				}
			}

			foreach (var element in elementsToRemove) {
				if (!(bool)removeMethod.Invoke(destinationEnumerable, new object[] { element }))
					throw new InvalidOperationException($"Could not remove element {element} from the destination collection {destinationEnumerable}");
			}
			foreach (var element in elementsToAdd) {
				addMethod.Invoke(destinationEnumerable, new object[] { element });
			}
		}

		protected LambdaExpression GetEntityPredicate(Type entityType, object[] keyValues, IKey key) {
			var entityParam = Expression.Parameter(entityType, "entity");
			Expression body = key.Properties
				.Select((p, i) => Expression.Equal(Expression.Property(entityParam, p.PropertyInfo), Expression.Constant(keyValues[i])))
				.Aggregate(Expression.AndAlso);
			return Expression.Lambda(body, entityParam);
		}

		internal EntityMappingInfo[] RetrieveLocalsAndPredicates(
			IEnumerable sourceEnumerable, Type keyType, Type entityType,
			IKey key, EntitiesRetrievalMode retrievalMode, IEnumerable local, DbContext db) {

			Delegate tupleToValueTuple = keyType.IsTuple() ? GetOrCreateTupleToValueTupleMap(entityType, keyType) : null;
			Delegate keyToValuesMap = GetOrCreateKeyToValuesMap(entityType, keyType);
			return sourceEnumerable
				.Cast<object>()
				.Select(sourceElement => {
					if (sourceElement == null || TypeUtils.IsDefaultValue(keyType.UnwrapNullable(), sourceElement))
						return new EntityMappingInfo();

					if (keyType.IsTuple()) 
						sourceElement = tupleToValueTuple.DynamicInvoke(sourceElement);

					var keyValues = keyToValuesMap.DynamicInvoke(sourceElement) as object[]
						?? throw new InvalidOperationException($"Invalid key(s) returned for entity of type {entityType.FullName ?? entityType.Name}");

					var expr = GetEntityPredicate(entityType, keyValues, key);
					var deleg = expr.Compile();

					object localEntity;
					if (retrievalMode != EntitiesRetrievalMode.Remote) {
						localEntity = local
							.Cast<object>()
							.FirstOrDefault(e => (bool)deleg.DynamicInvoke(e));

						if (localEntity == null && retrievalMode == EntitiesRetrievalMode.LocalOrAttach)
							AttachEntity(entityType, db, ref localEntity, keyValues, key);
					}
					else
						localEntity = null;

					return new EntityMappingInfo {
						LocalEntity = localEntity,
						Expression = localEntity != null ? null : expr,
						Delegate = deleg
					};
				})
				.ToArray();
		}
	}
}
