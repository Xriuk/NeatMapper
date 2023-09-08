namespace NeatMapper.Core {
	public interface IMap<TSource, TDestination> {
		public static abstract TDestination Map(TSource source, MappingContext context);
	}
}
