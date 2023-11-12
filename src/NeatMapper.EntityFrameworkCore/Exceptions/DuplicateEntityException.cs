using System;

namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// <para>
	/// Exception thrown when a duplicate entity is encountered when merge matching.
	/// </para>
	/// <para>
	/// This happens when a non-null destination is provided when an entity is not found
	/// (either if a null key is passed or the entity is not found on the db),
	/// or when a non-null destination different from the retrieved entity is provided.
	/// </para>
	/// </summary>
	public sealed class DuplicateEntityException : Exception {
		public DuplicateEntityException(string message) : base(message) { }
	}
}
