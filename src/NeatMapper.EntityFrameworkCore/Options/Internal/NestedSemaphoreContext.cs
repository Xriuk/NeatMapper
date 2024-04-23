namespace NeatMapper.EntityFrameworkCore {
	/// <summary>
	/// Indicates that the current map/match is happening inside a <see cref="Microsoft.EntityFrameworkCore.DbContext"/>,
	/// so another semaphore lock is not needed (and would cause a deadlock). Used inside <see cref="EntityFrameworkCoreMatcher"/>
	/// to avoid implementing re-entrant locks.
	/// </summary>
	internal class NestedSemaphoreContext {
		/// <summary>
		/// Singleton instance of the class.
		/// </summary>
		public static readonly NestedSemaphoreContext Instance = new NestedSemaphoreContext();


		private NestedSemaphoreContext() { }
	}
}
