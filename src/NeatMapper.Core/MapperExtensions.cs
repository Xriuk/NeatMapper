namespace NeatMapper.Core {
	public static class MapperExtensions {
		public static TDestination Map<TDestination>(this IMapper mapper, object source) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			return (TDestination)mapper.Map(source, source.GetType(), typeof(TDestination))!;
		}

		public static TDestination Map<TSource, TDestination>(this IMapper mapper, TSource source) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return (TDestination)mapper.Map(source, typeof(TSource), typeof(TDestination))!;
		}

		public static TDestination Map<TSource, TDestination>(this IMapper mapper, TSource source, TDestination destination) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return (TDestination)mapper.Map(source, typeof(TSource), destination, typeof(TDestination))!;
		}
	}
}
