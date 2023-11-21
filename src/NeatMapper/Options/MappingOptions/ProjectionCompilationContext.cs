namespace NeatMapper {
	/// <summary>
	/// Indicates that the current projection will be compiled (along with others) into the corresponding
	/// delegate.
	/// </summary>
	public sealed class ProjectionCompilationContext {
		/// <summary>
		/// Singleton instance of the class.
		/// </summary>
		public static readonly ProjectionCompilationContext Instance = new ProjectionCompilationContext();

		private ProjectionCompilationContext() { }
	}
}
