using NeatMapper.Core.Internal;

namespace NeatMapper.Core {
	public static class MapperExtensions {
		public static TDestination Map<TDestination>(this IMapper mapper, object source) {
			if(source == null)
				throw new ArgumentNullException(nameof(source));
			return (TDestination)mapper.Map(source, source.GetType(), typeof(TDestination))!;
		}

		public static TDestination Map<TSource, TDestination>(this IMapper mapper, TSource source) {
			return (TDestination)mapper.Map(source, typeof(TSource), typeof(TDestination))!;
		}

		public static TDestination Map<TSource, TDestination>(this IMapper mapper, TSource source, TDestination destination) {
			return (TDestination)mapper.Map(source, typeof(TSource), destination, typeof(TDestination))!;
		}

		public static Task<TDestination> MapAsync<TDestination>(this IAsyncMapper mapper, object source, CancellationToken cancellationToken = default) {
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, source.GetType(), typeof(TDestination), cancellationToken));
		}

		public static Task<TDestination> MapAsync<TSource, TDestination>(this IAsyncMapper mapper, TSource source, CancellationToken cancellationToken = default) {
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, typeof(TSource), typeof(TDestination), cancellationToken))!;
		}

		public static Task<TDestination> MapAsync<TSource, TDestination>(this IAsyncMapper mapper, TSource source, TDestination destination, CancellationToken cancellationToken = default) {
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, typeof(TSource), destination, typeof(TDestination), cancellationToken))!;
		}
	}
}
