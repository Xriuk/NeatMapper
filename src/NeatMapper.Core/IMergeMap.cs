namespace NeatMapper.Core {
	public interface IMergeMap<TSource, TDestination> {
		public static abstract void Map(TSource source, TDestination destination, MappingContext context);
	}
}
