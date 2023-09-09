namespace NeatMapper.Core {
	public interface IAsyncNewMap<TSource, TDestination> {
		public static abstract Task<TDestination> Map(TSource source, AsyncMappingContext context);
	}
}
