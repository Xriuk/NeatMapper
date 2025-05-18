using System;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// Configures options for retrieving entitites, for <see cref="EntityFrameworkCoreMapper"/>
	/// and <see cref="AsyncEntityFrameworkCoreMapper"/>.<br/>
	/// Can be overridden during mapping with <see cref="EntityFrameworkCoreMappingOptions"/>.
	/// </summary>
	public sealed class EntityFrameworkCoreOptions {
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public EntityFrameworkCoreOptions() {
			EntitiesRetrievalMode = EntitiesRetrievalMode.LocalOrRemote;
			ThrowOnDuplicateEntity = false;
			IgnoreNullEntities = false;
		}
		/// <summary>
		/// Creates a new instance by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from.</param>
		public EntityFrameworkCoreOptions(EntityFrameworkCoreOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			EntitiesRetrievalMode = options.EntitiesRetrievalMode;
			ThrowOnDuplicateEntity = options.ThrowOnDuplicateEntity;
			IgnoreNullEntities = options.IgnoreNullEntities;
		}


		/// <inheritdoc cref="EntityFrameworkCore.EntitiesRetrievalMode"/>
		/// <remarks>Defaults to <see cref="EntitiesRetrievalMode.LocalOrRemote"/>.</remarks>
		public EntitiesRetrievalMode EntitiesRetrievalMode { get; set; }

		/// <summary>
		/// If <see langword="true"/> will throw a <see cref="DuplicateEntityException"/> when a duplicate entity is found
		/// for the same key while merging, otherwise will return the entity from the <see cref="Microsoft.EntityFrameworkCore.DbContext"/>.
		/// </summary>
		/// <remarks>Defaults to <see langword="false"/>.</remarks>
		public bool ThrowOnDuplicateEntity { get; set; }

		/// <summary>
		/// If <see langword="true"/> will not add <see langword="null"/> entities to the collection result,
		/// otherwise will add them making each result entity match with the corresponding source key by index.
		/// For merge collections this will not affect existing null entities inside the destination collection.
		/// </summary>
		/// <remarks>Defaults to <see langword="false"/>.</remarks>
		public bool IgnoreNullEntities { get; set; }
	}
}
