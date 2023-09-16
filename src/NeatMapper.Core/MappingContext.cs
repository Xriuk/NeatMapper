namespace NeatMapper.Core {
	public sealed class MappingContext {
		internal MappingContext() { }


		/// <summary>
		/// Mapper which can be used for nested mappings
		/// </summary>
		public IMapper Mapper { get; internal set; } = null!;

		/// <summary>
		/// Scoped service provider which can be used to retrieve additional services
		/// </summary>
		public IServiceProvider ServiceProvider { get; internal set; } = null!;
	}
}
