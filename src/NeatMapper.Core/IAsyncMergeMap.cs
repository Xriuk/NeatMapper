namespace NeatMapper.Core {
	public interface IAsyncMergeMap<TSource, TDestination> {
		public static abstract Task<TDestination> MapAsync(TSource source, TDestination destination, AsyncMappingContext context);
	}
}
