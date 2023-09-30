namespace NeatMapper.Common.Mapper {
	public abstract class BaseContext {
		/// <summary>
		/// Scoped service provider which can be used to retrieve additional services
		/// </summary>
		public IServiceProvider ServiceProvider { get; internal set; } = null!;
	}
}
