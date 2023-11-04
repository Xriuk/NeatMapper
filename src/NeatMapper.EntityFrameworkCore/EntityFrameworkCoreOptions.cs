namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// Configures options for <see cref="EntityFrameworkCoreMapper"/> and <see cref="AsyncEntityFrameworkCoreMapper"/>
	/// </summary>
	public sealed class EntityFrameworkCoreOptions {
		/// <inheritdoc cref="EntityFrameworkCore.EntitiesRetrievalMode"/>
		/// <remarks>Defaults to <see cref="EntitiesRetrievalMode.LocalOrRemote"/></remarks>
		public EntitiesRetrievalMode EntitiesRetrievalMode { get; set; } = EntitiesRetrievalMode.LocalOrRemote;
	}
}
