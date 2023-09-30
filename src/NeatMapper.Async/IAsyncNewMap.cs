namespace NeatMapper.Async {
	public interface IAsyncNewMap<TSource, TDestination> {
		public static abstract Task<TDestination> MapAsync(TSource source, AsyncMappingContext context);
	}
}
