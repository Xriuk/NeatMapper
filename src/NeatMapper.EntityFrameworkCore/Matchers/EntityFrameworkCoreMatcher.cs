using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Concurrent;
using System.Linq;
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
		/// <see cref="IMapper"/> used to retrieve keys from entities.
		/// </summary>
		private readonly ProjectionMapper _entityKeyMapper;

		/// <summary>
		/// <see cref="IMatcher"/> used to compare single keys or tuples.
		/// </summary>
		private readonly CompositeMatcher _keyMatcher;


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
			IServiceProvider? serviceProvider = null) {

			_model = model ?? throw new ArgumentNullException(nameof(model));
			_dbContextType = dbContextType ?? throw new ArgumentNullException(nameof(dbContextType));
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
			_entityKeyMapper = new ProjectionMapper(new EntityFrameworkCoreProjector(model, dbContextType, serviceProvider));
			_keyMatcher = new CompositeMatcher(new CompositeMatcherOptions {
				Matchers = [
					StructuralEquatableMatcher.Instance,
					ObjectEqualsMatcher.Instance
				]
			});
		}


		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMatchInternal(sourceType, destinationType, mappingOptions, out _, out _);
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatchInternal(sourceType, destinationType, mappingOptions, out var keyType, out var entityType))
				throw new MapNotFoundException((sourceType, destinationType));

			NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			NeatMapper.TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			if (source == null || destination == null)
				return false;

			// Check if we are matching an entity with its key,
			// or two entities
			object? sourceKey;
			object? destinationKey;
			if (keyType != null) {
				object entityObject;
				if (sourceType == keyType) {
					destinationKey = source;
					entityObject = destination;
				}
				else {
					destinationKey = destination;
					entityObject = source;
				}

				// Retrieve the key from the entity
				try {
					sourceKey = _entityKeyMapper.Map(entityObject, entityType, keyType, mappingOptions);
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (MappingException e) {
					throw new MatcherException(e.InnerException!, (sourceType, destinationType));
				}
				catch (Exception e) {
					throw new MatcherException(e, (sourceType, destinationType));
				}
			}
			else {
				// Retrieve the key type
				var keyTypes = EfCoreUtils.RetrieveEntityType(_model, entityType)!
					.FindPrimaryKey()!
					.Properties.Where(p => !p.GetContainingForeignKeys().Any(k => k.IsOwnership))
						.Select(p => p.ClrType);

				// Composite key types are converted to ValueTuples
				if (keyTypes.Count() == 1)
					keyType = keyTypes.Single()!;
				else
					keyType = TupleUtils.GetValueTupleConstructor(keyTypes.ToArray()).DeclaringType!;

				// Retrieve the key from the source and destination entities
				INewMapFactory entityToKeyFactory;
				try {
					entityToKeyFactory = _entityKeyMapper.MapNewFactory(entityType, keyType, mappingOptions);
				}
				catch {
					throw new MapNotFoundException((sourceType, destinationType));
				}
				using (entityToKeyFactory) { 
					try {
						sourceKey = entityToKeyFactory.Invoke(source);
						destinationKey = entityToKeyFactory.Invoke(destination);
					}
					catch (OperationCanceledException) {
						throw;
					}
					catch (MappingException e) {
						throw new MatcherException(e.InnerException!, (sourceType, destinationType));
					}
					catch (Exception e) {
						throw new MatcherException(e, (sourceType, destinationType));
					}
				}
			}

			// Match the two keys
			try {
				return _keyMatcher.Match(sourceKey, keyType, destinationKey, keyType);
			}
			catch (OperationCanceledException) {
				throw;
			}
			catch (MatcherException e) {
				throw new MatcherException(e.InnerException!, (sourceType, destinationType));
			}
			catch (Exception e) {
				throw new MatcherException(e, (sourceType, destinationType));
			}
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if(!CanMatchInternal(sourceType, destinationType, mappingOptions, out var keyType, out var entityType))
				throw new MapNotFoundException((sourceType, destinationType));

			// Check if we are matching an entity with its key,
			// or two entities
			bool isEntityKey;
			if(keyType == null) {
				// Retrieve the key type
				var keyTypes = EfCoreUtils.RetrieveEntityType(_model, entityType)!
					.FindPrimaryKey()!
					.Properties.Where(p => !p.GetContainingForeignKeys().Any(k => k.IsOwnership))
						.Select(p => p.ClrType);

				// Composite key types are converted to ValueTuples
				if (keyTypes.Count() == 1)
					keyType = keyTypes.Single()!;
				else 
					keyType = TupleUtils.GetValueTupleConstructor(keyTypes.ToArray()).DeclaringType!;

				isEntityKey = false;
			}
			else
				isEntityKey = true;

			INewMapFactory entityToKeyFactory;
			try {
				entityToKeyFactory = _entityKeyMapper.MapNewFactory(entityType, keyType, mappingOptions);
			}
			catch {
				throw new MapNotFoundException((sourceType, destinationType));
			}

			try {
				IMatchMapFactory keyMatchFactory;
				try {
					keyMatchFactory = _keyMatcher.MatchFactory(keyType, keyType, mappingOptions);
				}
				catch {
					throw new MapNotFoundException((sourceType, destinationType));
				}

				try {
					if (isEntityKey) {
						return new DisposableMatchMapFactory(
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

								// Retrieve the key from the entity
								object? entityKey;
								try {
									entityKey = entityToKeyFactory.Invoke(entityObject);
								}
								catch (OperationCanceledException) {
									throw;
								}
								catch(MappingException e) {
									throw new MatcherException(e.InnerException!, (sourceType, destinationType));
								}
								catch (Exception e) {
									throw new MatcherException(e, (sourceType, destinationType));
								}

								// Match the two keys
								try {
									return keyMatchFactory.Invoke(entityKey, keyObject);
								}
								catch (OperationCanceledException) {
									throw;
								}
								catch (MatcherException e) {
									throw new MatcherException(e.InnerException!, (sourceType, destinationType));
								}
								catch (Exception e) {
									throw new MatcherException(e, (sourceType, destinationType));
								}
							}, entityToKeyFactory, keyMatchFactory);
					}
					else {
						return new DisposableMatchMapFactory(
							sourceType, destinationType,
							(source, destination) => {
								NeatMapper.TypeUtils.CheckObjectType(source, sourceType, nameof(source));
								NeatMapper.TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

								if (source == null || destination == null)
									return false;

								// Retrieve the key from the source and destination entities
								object? sourceKey;
								object? destinationKey;
								try {
									sourceKey = entityToKeyFactory.Invoke(source);
									destinationKey = entityToKeyFactory.Invoke(destination);
								}
								catch (OperationCanceledException) {
									throw;
								}
								catch (MappingException e) {
									throw new MatcherException(e.InnerException!, (sourceType, destinationType));
								}
								catch (Exception e) {
									throw new MatcherException(e, (sourceType, destinationType));
								}

								// Match the two keys
								try {
									return keyMatchFactory.Invoke(sourceKey, destinationKey);
								}
								catch (OperationCanceledException) {
									throw;
								}
								catch (MatcherException e) {
									throw new MatcherException(e.InnerException!, (sourceType, destinationType));
								}
								catch (Exception e) {
									throw new MatcherException(e, (sourceType, destinationType));
								}
							}, entityToKeyFactory, keyMatchFactory);
					}
				}
				catch {
					keyMatchFactory.Dispose();
					throw;
				}
			}
			catch {
				entityToKeyFactory.Dispose();
				throw;
			}
		}


		bool CanMatchInternal(
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions,
			out Type? keyType,
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
				entityType = null!;
				return false;
			}

			if (!entityType.IsClass)
				return false;

			// Check that the entity has a key and that it matches the key type
			// For owned entities foreign keys are excluded
			var key = EfCoreUtils.RetrieveEntityType(_model, entityType)?.FindPrimaryKey();
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
					if (keyPropertiesCount != keyTypes.Length || !keyTypes.Zip(keyProperties, (k1, k2) => (KeyType: k1, PropertyType: k2.ClrType)).All(keys => keys.KeyType == keys.PropertyType))
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
		private DbContext? RetrieveDbContext(MappingOptions? mappingOptions) {
			var dbContext = mappingOptions?.GetOptions<EntityFrameworkCoreMappingOptions>()?.DbContextInstance;
			if (dbContext != null && dbContext.GetType() != _dbContextType)
				dbContext = null;

			dbContext ??= (mappingOptions?.GetOptions<MatcherOverrideMappingOptions>()?.ServiceProvider ?? _serviceProvider)
				.GetService(_dbContextType) as DbContext;

			return dbContext;
		}
	}
}
