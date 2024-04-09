#if !NET6_0_OR_GREATER
using Microsoft.EntityFrameworkCore; 
#endif
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// <see cref="IMatcher"/> which matches entities with their keys (even composite keys
	/// as <see cref="Tuple"/> or <see cref="ValueTuple"/>). Allows also matching entities with other entities.
	/// </summary>
	public sealed class EntityFrameworkCoreMatcher : IMatcher, IMatcherCanMatch, IMatcherFactory {
		/// <summary>
		/// Db model, shared between instances of the same DbContext type.
		/// </summary>
		private readonly IModel _model;

		/// <summary>
		/// Delegates which compare an entity with its key, keys are entity types, the order of the parameters is: entity, key.
		/// </summary>
		private readonly ConcurrentDictionary<Type, Func<object, object, bool>> _entityKeyComparerCache = new ConcurrentDictionary<Type, Func<object, object, bool>>();

		/// <summary>
		/// Delegates which compare an entity with another entity of the same type, keys are entity types.
		/// </summary>
		private readonly ConcurrentDictionary<Type, Func<object, object, bool>> _entityEntityComparerCache = new ConcurrentDictionary<Type, Func<object, object, bool>>();


		/// <summary>
		/// Creates a new instance of <see cref="EntityFrameworkCoreMatcher"/>.
		/// </summary>
		/// <param name="model">Model to use to retrieve keys of entities.</param>
		public EntityFrameworkCoreMatcher(IModel model) {
			_model = model ?? throw new ArgumentNullException(nameof(model));
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

		public bool CanMatch(
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

			// Check which type is the key and which is the entity, since it can be used both ways
			// We could also match two entities
			Type keyType;
			Type entityType;
			if (sourceType.IsKeyType() || sourceType.IsCompositeKeyType()) {
				keyType = sourceType;
				entityType = destinationType;
			}
			else if (destinationType.IsKeyType() || destinationType.IsCompositeKeyType()) {
				keyType = destinationType;
				entityType = sourceType;
			}
			else if(sourceType == destinationType) {
				keyType = null;
				entityType = sourceType;
			}
			else
				return false;

			if (!entityType.IsClass)
				return false;

			// Check if we already have a cached matching map
			if(keyType != null) {
				if(_entityKeyComparerCache.ContainsKey(entityType))
					return true;
			}
			else if(_entityEntityComparerCache.ContainsKey(entityType))
				return true;

			// Check if the entity is in the model
			var modelEntity = _model.FindEntityType(entityType);
			if (modelEntity == null || modelEntity.IsOwned())
				return false;

			// Check that the entity has a key and that it matches the key type
			var key = modelEntity.FindPrimaryKey();
			if (key == null || key.Properties.Count < 1 || !key.Properties.All(p => p.PropertyInfo != null))
				return false;
			if(keyType != null) { 
				if (keyType.IsCompositeKeyType()) {
					var keyTypes = !keyType.IsNullable() ? keyType.GetGenericArguments() : keyType.GetGenericArguments()[0].GetGenericArguments();
					if (key.Properties.Count != keyTypes.Length || !keyTypes.Zip(key.Properties, (k1, k2) => (k1, k2.ClrType)).All(keys => keys.Item1 == keys.Item2))
						return false;
				}
				else if (key.Properties.Count != 1 || (key.Properties[0].ClrType != keyType && !keyType.IsNullable(key.Properties[0].ClrType)))
					return false;
			}

			return true;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (!CanMatch(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			// Check if we are matching an entity with its key,
			// or two entities
			if (sourceType != destinationType) {
				Type keyType;
				Type entityType;
				if (sourceType.IsKeyType() || sourceType.IsCompositeKeyType()) {
					keyType = sourceType;
					entityType = destinationType;
				}
				else {
					keyType = destinationType;
					entityType = sourceType;
				}

				// Create and cache the delegate for the map if needed
				var entityKeyComparer = _entityKeyComparerCache.GetOrAdd(entityType, _ => {
					var entityParam = Expression.Parameter(typeof(object), "entity");
					var keyParam = Expression.Parameter(typeof(object), "key");
					var keyParamType = keyType.IsCompositeKeyType() ? TupleUtils.GetValueTupleConstructor(keyType.UnwrapNullable().GetGenericArguments()).DeclaringType : keyType.UnwrapNullable();
					var modelEntity = _model.FindEntityType(entityType);
					var key = modelEntity.FindPrimaryKey();
					Expression body;
					if (key.Properties.Count == 1) {
						// ((EntityType)entity).Id == (KeyType)key
						body = Expression.Equal(Expression.Property(Expression.Convert(entityParam, entityType), key.Properties[0].PropertyInfo), Expression.Convert(keyParam, keyParamType));
					}
					else {
						// ((EntityType)entity).Key1 == ((KeyType)key).Key1 && ...
						body = key.Properties
							.Select((p, i) => Expression.Equal(
								Expression.Property(Expression.Convert(entityParam, entityType), p.PropertyInfo),
								Expression.PropertyOrField(Expression.Convert(keyParam, keyParamType), "Item" + (i + 1))))
							.Aggregate(Expression.AndAlso);
					}
					return Expression.Lambda<Func<object, object, bool>>(body, entityParam, keyParam).Compile();
				});
				
				// If the key is a tuple we convert it to a value tuple, because maps are with value tuples only
				var tupleToValueTuple = keyType.IsTuple() ? EfCoreUtils.GetOrCreateTupleToValueTupleMap(keyType) : null;

				return new MatchMapFactory(
					sourceType, destinationType,
					(source, destination) => {
						NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));
						NeatMapper.TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

						if (TypeUtils.IsDefaultValue(sourceType, source) || TypeUtils.IsDefaultValue(destinationType, destination))
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
						if(tupleToValueTuple != null) 
							keyObject = tupleToValueTuple.DynamicInvoke(keyObject);

						return entityKeyComparer.Invoke(entityObject, keyObject);
					});
			}
			else {
				// Create and cache the delegate if needed
				var entityEntityComparer = _entityEntityComparerCache.GetOrAdd(sourceType, type => {
					var entity1Param = Expression.Parameter(typeof(object), "entity1");
					var entity2Param = Expression.Parameter(typeof(object), "entity2");
					var modelEntity = _model.FindEntityType(type);
					var key = modelEntity.FindPrimaryKey();
					// ((EntityType)entity1).Key1 == ((EntityType)entity2).Key1 && ...
					Expression body = key.Properties
						.Select((p, i) => Expression.Equal(
							Expression.Property(Expression.Convert(entity1Param, type), p.PropertyInfo),
							Expression.Property(Expression.Convert(entity2Param, type), p.PropertyInfo)))
						.Aggregate(Expression.AndAlso);
					return Expression.Lambda<Func<object, object, bool>>(body, entity1Param, entity2Param).Compile();
				});

				return new MatchMapFactory(
					sourceType, destinationType,
					(source, destination) => {
						NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));
						NeatMapper.TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

						if (source == null || destination == null)
							return false;

						return entityEntityComparer.Invoke(source, destination);
					});
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
