using Microsoft.EntityFrameworkCore;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// Options for <see cref="EntityFrameworkCoreMapper"/> and <see cref="AsyncEntityFrameworkCoreMapper"/>,
	/// these will override any configuration options defined in <see cref="EntityFrameworkCoreOptions"/>.
	/// </summary>
	public sealed class EntityFrameworkCoreMappingOptions {
		public EntityFrameworkCoreMappingOptions(
			EntitiesRetrievalMode? entitiesRetrievalMode = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			DbContext?
#else
			DbContext
#endif
			dbContextInstance = null,
			bool? throwOnDuplicateEntity = null) {

			EntitiesRetrievalMode = entitiesRetrievalMode;
			DbContextInstance = dbContextInstance;
			ThrowOnDuplicateEntity = throwOnDuplicateEntity;
		}


		/// <inheritdoc cref="EntityFrameworkCore.EntitiesRetrievalMode"/>
		/// <remarks><see langword="null"/> to use global setting</remarks>
		public EntitiesRetrievalMode? EntitiesRetrievalMode {
			get;
#if NET5_0_OR_GREATER
			init;
#endif
		}

		/// <summary>
		/// Overrides the db instance to use for the mapping, the type must be correct, the mapper will pick it up
		/// only if it matches the type passed in the constructor.<br/>
		/// Will be also used to retrieve shadow keys for tracked entities.
		/// </summary>
		/// <remarks>
		/// <see langword="null"/> to inject the context from the <see cref="System.IServiceProvider"/> of the mapper.
		/// </remarks>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			DbContext?
#else
			DbContext
#endif
			DbContextInstance {
				get;
#if NET5_0_OR_GREATER
				init;
#endif
		}

		/// <summary>
		/// If <see langword="true"/> will throw a <see cref="DuplicateEntityException"/> when a duplicate entity is found
		/// for the same key while merging, otherwise will return the entity from the <see cref="DbContext"/>.
		/// </summary>
		/// <remarks><see langword="null"/> to use global setting.</remarks>
		public bool? ThrowOnDuplicateEntity {
			get;
#if NET5_0_OR_GREATER
			init;
#endif
		}
	}
}
