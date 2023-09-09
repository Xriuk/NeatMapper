namespace NeatMapper.Core {
	public interface IMapper {
		public TDestination Map<TSource, TDestination>(TSource source);

		public TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
	}
}
