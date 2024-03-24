#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	internal class AsyncMergeMapFactory<TSource, TDestination> : IAsyncMergeMapFactory<TSource, TDestination> {
		private int _disposed = 0;
		private readonly Func<TSource, TDestination, Task<TDestination>> _mapDelegate;

		internal AsyncMergeMapFactory(Func<TSource, TDestination, Task<TDestination>> mapDelegate)
			: this(typeof(TSource), typeof(TDestination), mapDelegate) { }
		protected AsyncMergeMapFactory(Type sourceType, Type destinationType, Func<TSource, TDestination, Task<TDestination>> mapDelegate) {
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public Type SourceType { get; }

		public Type DestinationType { get; }


		public Task<TDestination> Invoke(TSource source, TDestination destination) {
			if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);
			return _mapDelegate.Invoke(source, destination);
		}

		public virtual void Dispose() {
			if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
				throw new ObjectDisposedException(null);
		}

		async Task<object> IAsyncMergeMapFactory.Invoke(object source, object destination) {
			return await Invoke((TSource)source, (TDestination)destination);
		}
	}

	internal class AsyncMergeMapFactory : AsyncMergeMapFactory<object, object> {
		internal AsyncMergeMapFactory(Type sourceType, Type destinationType, Func<object, object, Task<object>> mapDelegate) :
			base(sourceType, destinationType, mapDelegate) {}
	}
}
