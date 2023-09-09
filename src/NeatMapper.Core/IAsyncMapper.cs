namespace NeatMapper.Core {
	public interface IAsyncMapper {
		public Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default);

		public Task<TDestination> MapAsync<TSource, TDestination>(TSource source, TDestination destination, CancellationToken cancellationToken = default);
	}
}
