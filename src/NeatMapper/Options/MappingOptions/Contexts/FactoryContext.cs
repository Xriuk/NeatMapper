using System;

namespace NeatMapper {
	/// <summary>
	/// Indicates that the current map will be part (along with others) of a mapping/matching factory
	/// (see <see cref="IMapperFactory"/>, <see cref="IMatcherFactory"/>, <see cref="IAsyncMapperFactory"/>).
	/// This allows mappers/matchers and maps to optimize the results to increase the performance of the map
	/// (eg. by caching where possible).
	/// </summary>
	[Obsolete("All mappers should use factories by default, and there should not be distinction between maps " +
		"and factories, thus all methods should be optimized when possible. This class will be removed " +
		"in future versions.")]
	public sealed class FactoryContext {
		/// <summary>
		/// Singleton instance of the class.
		/// </summary>
		public static readonly FactoryContext Instance = new FactoryContext();


		private FactoryContext() { }
	}
}
