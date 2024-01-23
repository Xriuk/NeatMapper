#if !NET6_0_OR_GREATER
using Microsoft.EntityFrameworkCore; 
#endif
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// <see cref="IMatcher"/> which matches entities with their keys, even composite keys
	/// as <see cref="Tuple"/> or <see cref="ValueTuple"/>. Allows also to match entities with other entities.
	/// </summary>
	public sealed class EntityFrameworkCoreMatcher : IMatcher, IMatcherCanMatch {
		private readonly IModel _model;
		// entity: (entity, key) => bool
		private readonly IDictionary<Type, Delegate> _entityKeyComparerCache = new Dictionary<Type, Delegate>();
		private readonly IDictionary<Type, Delegate> _tupleToValueTupleCache = new Dictionary<Type, Delegate>();
		// entity: (entity, entity) => bool
		private readonly IDictionary<Type, Delegate> _entityEntityComparerCache = new Dictionary<Type, Delegate>();

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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source != null && !sourceType.IsAssignableFrom(source.GetType()))
				throw new ArgumentException($"Object of type {source.GetType().FullName ?? source.GetType().Name} is not assignable to type {sourceType.FullName ?? sourceType.Name}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));
			if (destination != null && !destinationType.IsAssignableFrom(destination.GetType()))
				throw new ArgumentException($"Object of type {destination.GetType().FullName ?? destination.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}", nameof(destination));

			if (!CanMatch(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			try { 
				// Check if we are matching an entity with its key or two entities
				if(sourceType != destinationType) {
					object keyObject;
					Type keyType;
					object entityObject;
					Type entityType;
					if (sourceType.IsKeyType() || sourceType.IsCompositeKeyType()) {
						keyObject = source;
						keyType = sourceType;
						entityObject = destination;
						entityType = destinationType;
					}
					else {
						keyObject = destination;
						keyType = destinationType;
						entityObject = source;
						entityType = sourceType;
					}

					if(entityObject == null || keyObject == null || TypeUtils.IsDefaultValue(keyType, keyObject))
						return false;

					// Create and cache the delegate if needed
					Delegate entityKeyComparer;
					lock (_entityKeyComparerCache) {
						if (!_entityKeyComparerCache.TryGetValue(entityType, out entityKeyComparer)) {
							var entityParam = Expression.Parameter(entityType, "entity");
							var keyParam = Expression.Parameter(keyType.IsCompositeKeyType() ? TupleUtils.GetValueTupleConstructor(keyType.UnwrapNullable().GetGenericArguments()).DeclaringType : keyType.UnwrapNullable(), "key");
							var modelEntity = _model.FindEntityType(entityType);
							var key = modelEntity.FindPrimaryKey();
							Expression body;
							if (key.Properties.Count == 1) {
								// entity.Id == key
								body = Expression.Equal(Expression.Property(entityParam, key.Properties[0].PropertyInfo), keyParam);
							}
							else{
								// entity.Key1 == key.Key1 && ...
								body = key.Properties
									.Select((p, i) => Expression.Equal(
										Expression.Property(entityParam, p.PropertyInfo),
										Expression.PropertyOrField(keyParam, "Item" + (i + 1))))
									.Aggregate(Expression.AndAlso);
							}
							entityKeyComparer = Expression.Lambda(typeof(Func<,,>).MakeGenericType(entityType, keyParam.Type, body.Type), body, entityParam, keyParam).Compile();
							_entityKeyComparerCache.Add(entityType, entityKeyComparer);
						}
					}

					if (keyType.IsTuple()) {
						Delegate tupleToValueTuple;
						lock (_tupleToValueTupleCache) {
							if (!_tupleToValueTupleCache.TryGetValue(entityType, out tupleToValueTuple)) {
								var keyParam = Expression.Parameter(keyType, "key");
								// new ValueTuple<...>(key.Item1, ...)
								Expression body = Expression.New(
									TupleUtils.GetValueTupleConstructor(keyParam.Type.GetGenericArguments()),
									Enumerable.Range(1, keyParam.Type.GetGenericArguments().Length).Select(n => Expression.Property(keyParam, "Item" + n)));
								tupleToValueTuple = Expression.Lambda(typeof(Func<,>).MakeGenericType(keyParam.Type, body.Type), body, keyParam).Compile();
								_tupleToValueTupleCache.Add(entityType, tupleToValueTuple);
							}
						}

						keyObject = tupleToValueTuple.DynamicInvoke(keyObject);
					}

					return (bool)entityKeyComparer.DynamicInvoke(entityObject, keyObject);
				}
				else {
					if (source == null || destination == null)
						return false;

					// Create and cache the delegate if needed
					Delegate entityEntityComparer;
					lock (_entityEntityComparerCache) {
						if (!_entityEntityComparerCache.TryGetValue(sourceType, out entityEntityComparer)) {
							var entity1Param = Expression.Parameter(sourceType, "entity1");
							var entity2Param = Expression.Parameter(sourceType, "entity2");
							var modelEntity = _model.FindEntityType(sourceType);
							var key = modelEntity.FindPrimaryKey();
							// entity1.Key1 == entity2.Key1 && ...
							Expression body = key.Properties
								.Select((p, i) => Expression.Equal(
									Expression.Property(entity1Param, p.PropertyInfo),
									Expression.Property(entity2Param, p.PropertyInfo)))
								.Aggregate(Expression.AndAlso);
							entityEntityComparer = Expression.Lambda(typeof(Func<,,>).MakeGenericType(sourceType, sourceType, body.Type), body, entity1Param, entity2Param).Compile();
							_entityEntityComparerCache.Add(sourceType, entityEntityComparer);
						}
					}

					return (bool)entityEntityComparer.DynamicInvoke(source, destination);
				}
			}
			catch (Exception e) {
				throw new MappingException(e, (sourceType, destinationType));
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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
	}
}
