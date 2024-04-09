namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// Specifies how the entities will be retrieved when mapping from keys to entities.
	/// </summary>
	public enum EntitiesRetrievalMode {
		/// <summary>
		/// Only local entities will be returned, null will be returned if not found.
		/// </summary>
		Local,
		/// <summary>
		/// <para>
		/// Local entities will be searched first and those missing will be attached to the context
		/// (via <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}.Attach(TEntity)"/>) with their state set to
		/// <see cref="Microsoft.EntityFrameworkCore.EntityState.Unchanged"/>, with just the keys properties set, and returned.
		/// </para>
		/// <para>
		/// When merging entities, if not found locally and a destination is provided, it will be attached to the context
		/// instead of creating a new entity.
		/// </para>
		/// <para>
		/// This can be used when you want to update entities without retrieving them first, or you just need
		/// to set a navigation to an entity which cannot be set with just the foreign key.
		/// </para>
		/// </summary>
		LocalOrAttach,
		/// <summary>
		/// Local entities will be searched first and those missing will be queried from the db, null will be returned if not found.
		/// </summary>
		LocalOrRemote,
		/// <summary>
		/// Entities will be queried directly from the db, the context will then handle merging them together with local ones, 
		/// null will be returned if not found.
		/// </summary>
		Remote
	}
}
