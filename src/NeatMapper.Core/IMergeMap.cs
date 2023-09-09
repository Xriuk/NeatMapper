namespace NeatMapper.Core {
	public interface IMergeMap<TSource, TDestination> {
		public static abstract TDestination Map(TSource source, TDestination destination, MappingContext context);
	}
}
