namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// Specifies how the entities will be retrieved when mapping from keys to entities
	/// </summary>
	public enum EntitiesRetrievalMode {
		/// <summary>
		/// Only local entities will be returned
		/// </summary>
		Local,
		/// <summary>
		/// Local entities will be searched first and those missing will be attached to the context
		/// (via <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}.Attach(TEntity)"/>) with their state set to
		/// <see cref="Microsoft.EntityFrameworkCore.EntityState.Unchanged"/>, with just the keys properties set, and returned.<br/>
		/// This can be used when you want to update entities without retrieving them first, or you just need
		/// to set a navigation to an entity which cannot be set with the foreign key
		/// </summary>
		LocalOrAttach,
		/// <summary>
		/// Local entities will be searched first and those missing will be queried from the db
		/// </summary>
		LocalOrRemote,
		/// <summary>
		/// Entities will be queried directly from the db, the context will then handle merging them together with local ones
		/// </summary>
		Remote
	}
}
