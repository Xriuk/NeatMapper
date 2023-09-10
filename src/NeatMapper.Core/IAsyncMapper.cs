namespace NeatMapper.Core {
	public interface IAsyncMapper {
		public Task<object?> MapAsync(object? source, Type sourceType, Type destinationType, CancellationToken cancellationToken = default);

		public Task<object?> MapAsync(object? source, Type sourceType, object? destination, Type destinationType, CancellationToken cancellationToken = default);
	}
}
