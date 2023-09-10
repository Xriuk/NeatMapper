namespace NeatMapper.Core {
	public interface ICollectionElementComparer<TSource, TDestination> {
		public static abstract bool Match(TSource source, TDestination destination, MappingContext context);
	}
}
