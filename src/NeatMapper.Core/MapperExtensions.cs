namespace NeatMapper.Core {
	public static class MapperExtensions {
		public static TDestination Map<TSource, TDestination>(this IMapper mapper, TSource source) {
			var destination = default(TDestination);
			mapper.Map(source, destination);
			return destination!;
		}
	}
}
