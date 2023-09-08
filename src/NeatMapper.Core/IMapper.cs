namespace NeatMapper.Core {
	public interface IMapper {
		public void Map<TSource, TDestination>(TSource source, TDestination destination);
	}
}
