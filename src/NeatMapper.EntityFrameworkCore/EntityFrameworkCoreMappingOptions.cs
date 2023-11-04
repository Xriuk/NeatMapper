namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// Options for <see cref="EntityFrameworkCoreMapper"/> and <see cref="AsyncEntityFrameworkCoreMapper"/>,
	/// these will override any configuration options defined in <see cref="EntityFrameworkCoreOptions"/>
	/// </summary>
	public sealed class EntityFrameworkCoreMappingOptions {
		public EntityFrameworkCoreMappingOptions(EntitiesRetrievalMode? entitiesRetrievalMode = null) {
			EntitiesRetrievalMode = entitiesRetrievalMode;
		}


		/// <inheritdoc cref="EntityFrameworkCore.EntitiesRetrievalMode"/>
		/// <remarks><see langword="null"/> to use global setting</remarks>
		public EntitiesRetrievalMode? EntitiesRetrievalMode { get; set; }
	}
}
