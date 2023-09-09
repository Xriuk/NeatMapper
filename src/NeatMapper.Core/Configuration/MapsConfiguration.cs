namespace NeatMapper.Core.Configuration {
	public sealed class MapperConfigurationOptions {
		public ICollection<Type> MapTypes { get; init; } = new List<Type>();
	}
}
