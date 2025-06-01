using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	internal class DefaultAsyncNewMapFactory : IAsyncNewMapFactory {
		protected int _disposed = 0;
		private readonly Func<object?, CancellationToken, Task<object?>> _mapDelegate;

		public DefaultAsyncNewMapFactory(Type sourceType, Type destinationType, Func<object?, CancellationToken, Task<object?>> mapDelegate) {
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public Type SourceType { get; }

		public Type DestinationType { get; }

		public Task<object?> Invoke(object? source, CancellationToken cancellationToken = default) {
			TypeUtils.CheckObjectType(source, SourceType, nameof(source));

			if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);

			return _mapDelegate.Invoke(source, cancellationToken);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing)
				Interlocked.CompareExchange(ref _disposed, 1, 0);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}

	internal class DefaultAsyncNewMapFactory<TSource, TDestination> : AsyncNewMapFactory<TSource, TDestination> {
		protected int _disposed = 0;
		private readonly Func<TSource?, CancellationToken, Task<TDestination?>> _mapDelegate;

		public DefaultAsyncNewMapFactory(Func<TSource?, CancellationToken, Task<TDestination?>> mapDelegate) {
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public override Task<TDestination?> Invoke(TSource? source, CancellationToken cancellationToken) {
			if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);
			return _mapDelegate.Invoke(source, cancellationToken);
		}

		protected override void Dispose(bool disposing) {
			if (disposing)
				Interlocked.CompareExchange(ref _disposed, 1, 0);
		}
	}
}
