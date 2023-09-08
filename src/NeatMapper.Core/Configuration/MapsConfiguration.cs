namespace NeatMapper.Core.Configuration {
	public sealed class MapsConfiguration {
		public ICollection<Type> MapTypes { get; set; } = new List<Type>();
	}
}
