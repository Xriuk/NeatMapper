namespace NeatMapper {
	/// <summary>
	/// Indicates that the current projection will be compiled (along with others) into the corresponding
	/// delegate. This should be used because projectors may return a different expression based on whether
	/// an expression needs to be compiled or not, or may refuse compilation by throwing <see cref="MapNotFoundException"/>.
	/// </summary>
	public sealed class ProjectionCompilationContext {
		/// <summary>
		/// Singleton instance of the class.
		/// </summary>
		public static readonly ProjectionCompilationContext Instance = new ProjectionCompilationContext();


		private ProjectionCompilationContext() { }
	}
}
