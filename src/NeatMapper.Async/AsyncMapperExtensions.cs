using NeatMapper.Async.Internal;

namespace NeatMapper.Async {
	public static class AsyncMapperExtensions {
		public static Task<TDestination> MapAsync<TDestination>(this IAsyncMapper mapper, object source, CancellationToken cancellationToken = default) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, source.GetType(), typeof(TDestination), cancellationToken));
		}

		public static Task<TDestination> MapAsync<TSource, TDestination>(this IAsyncMapper mapper, TSource source, CancellationToken cancellationToken = default) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, typeof(TSource), typeof(TDestination), cancellationToken))!;
		}

		public static Task<TDestination> MapAsync<TSource, TDestination>(this IAsyncMapper mapper, TSource source, TDestination destination, CancellationToken cancellationToken = default) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, typeof(TSource), destination, typeof(TDestination), cancellationToken))!;
		}
	}
}
