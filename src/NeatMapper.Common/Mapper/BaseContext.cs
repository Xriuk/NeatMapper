#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

namespace NeatMapper.Common.Mapper {
	/// <summary>
	/// Common class for mapping contexts
	/// </summary>
	public abstract class BaseContext {
		/// <summary>
		/// Scoped service provider which can be used to retrieve additional services
		/// </summary>
		public IServiceProvider ServiceProvider { get; internal set; }
	}
}
