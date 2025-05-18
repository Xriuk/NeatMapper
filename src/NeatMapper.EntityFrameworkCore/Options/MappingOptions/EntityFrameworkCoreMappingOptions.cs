using Microsoft.EntityFrameworkCore;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// Options for <see cref="EntityFrameworkCoreMatcher"/>, <see cref="EntityFrameworkCoreMapper"/>
	/// and <see cref="AsyncEntityFrameworkCoreMapper"/>, these will override any configuration options
	/// defined in <see cref="EntityFrameworkCoreOptions"/>.
	/// </summary>
	public sealed class EntityFrameworkCoreMappingOptions {
		/// <inheritdoc cref="EntityFrameworkCoreMappingOptions(EntitiesRetrievalMode?, DbContext?, bool?, bool?)"/>
		public EntityFrameworkCoreMappingOptions(
			EntitiesRetrievalMode? entitiesRetrievalMode,
			DbContext? dbContextInstance,
			bool? throwOnDuplicateEntity) : this(entitiesRetrievalMode, dbContextInstance, throwOnDuplicateEntity, null) {}
		/// <summary>
		/// Creates a new instance of <see cref="EntityFrameworkCoreMappingOptions"/>.
		/// </summary>
		/// <param name="entitiesRetrievalMode">
		/// <inheritdoc cref="EntitiesRetrievalMode" path="/summary"/>
		/// <inheritdoc cref="EntitiesRetrievalMode" path="/remarks"/>
		/// </param>
		/// <param name="dbContextInstance">
		/// <inheritdoc cref="DbContextInstance" path="/summary"/>
		/// <inheritdoc cref="DbContextInstance" path="/remarks"/>
		/// </param>
		/// <param name="ignoreNullEntities">
		/// <inheritdoc cref="IgnoreNullEntities" path="/summary"/>
		/// <inheritdoc cref="IgnoreNullEntities" path="/remarks"/>
		/// </param>
		public EntityFrameworkCoreMappingOptions(
			EntitiesRetrievalMode? entitiesRetrievalMode = null,
			DbContext? dbContextInstance = null,
			bool? throwOnDuplicateEntity = null,
			bool? ignoreNullEntities = null) {

			EntitiesRetrievalMode = entitiesRetrievalMode;
			DbContextInstance = dbContextInstance;
			ThrowOnDuplicateEntity = throwOnDuplicateEntity;
			IgnoreNullEntities = ignoreNullEntities;
		}


		/// <inheritdoc cref="EntityFrameworkCore.EntitiesRetrievalMode"/>
		/// <remarks><see langword="null"/> to use global setting.</remarks>
		public EntitiesRetrievalMode? EntitiesRetrievalMode { get; init; }

		/// <summary>
		/// Overrides the db instance to use for the mapping, the type must be correct, the mapper will pick it up
		/// only if it matches the type passed in the constructor.<br/>
		/// Will be also used to retrieve shadow keys for tracked entities.
		/// </summary>
		/// <remarks>
		/// <see langword="null"/> to inject the context from the <see cref="System.IServiceProvider"/> of the mapper.
		/// </remarks>
		public DbContext? DbContextInstance { get; init; }

		/// <summary>
		/// <inheritdoc cref="EntityFrameworkCoreOptions.ThrowOnDuplicateEntity" path="/summary"/>
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="EntityFrameworkCoreOptions"/></remarks>
		public bool? ThrowOnDuplicateEntity { get; init; }

		/// <summary>
		/// <inheritdoc cref="EntityFrameworkCoreOptions.IgnoreNullEntities" path="/summary"/>
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting from <see cref="EntityFrameworkCoreOptions"/></remarks>
		public bool? IgnoreNullEntities { get; init; }
	}
}
