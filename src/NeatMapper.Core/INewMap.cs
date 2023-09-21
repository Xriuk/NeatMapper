namespace NeatMapper.Core {
	public interface INewMap<TSource, TDestination> {
		public static abstract TDestination? Map(TSource? source, MappingContext context);
	}
}
