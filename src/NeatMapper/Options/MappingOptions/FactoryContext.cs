namespace NeatMapper {
	/// <summary>
	/// Indicates that the current map will be part (along with others) of a mapping factory
	/// (see <see cref="IMapperFactory"/>).
	/// This allows mappers and maps to optimize the results to increase the performance of the map
	/// (eg. by caching where possible).
	/// </summary>
	public sealed class FactoryContext {
		/// <summary>
		/// Singleton instance of the class.
		/// </summary>
		public static readonly FactoryContext Instance = new FactoryContext();


		private FactoryContext() { }
	}
}
