using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	internal class DefaultAsyncMergeMapFactory<TSource, TDestination> : AsyncMergeMapFactory<TSource, TDestination> {
		protected int _disposed = 0;
		private readonly Func<TSource?, TDestination?, CancellationToken, Task<TDestination?>> _mapDelegate;

		internal DefaultAsyncMergeMapFactory(Func<TSource?, TDestination?, CancellationToken, Task<TDestination?>> mapDelegate)
			: this(typeof(TSource), typeof(TDestination), mapDelegate) { }
		protected DefaultAsyncMergeMapFactory(Type sourceType, Type destinationType, Func<TSource?, TDestination?, CancellationToken, Task<TDestination?>> mapDelegate) {
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public override Type SourceType { get; }

		public override Type DestinationType { get; }


		public override Task<TDestination?> Invoke(TSource? source, TDestination? destination, CancellationToken cancellationToken) {
			if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);
			return _mapDelegate.Invoke(source, destination, cancellationToken);
		}

		protected override void Dispose(bool disposing) {
			if (disposing)
				Interlocked.CompareExchange(ref _disposed, 1, 0);
		}
	}

	internal sealed class DefaultAsyncMergeMapFactory : DefaultAsyncMergeMapFactory<object, object> {
		internal DefaultAsyncMergeMapFactory(Type sourceType, Type destinationType, Func<object?, object?, CancellationToken, Task<object?>> mapDelegate) :
			base(sourceType, destinationType, mapDelegate) {}
	}
}
