namespace NeatMapper.Core {
	public interface IAsyncMergeMap<TSource, TDestination> {
		public static abstract Task<TDestination> Map(TSource source, TDestination destination, AsyncMappingContext context);
	}
}
