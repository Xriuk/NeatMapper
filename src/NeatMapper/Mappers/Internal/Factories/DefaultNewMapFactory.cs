using System;
using System.Threading;

namespace NeatMapper {
	internal class DefaultNewMapFactory : INewMapFactory {
		protected int _disposed = 0;
		private readonly Func<object?, object?> _mapDelegate;

		public DefaultNewMapFactory(Type sourceType, Type destinationType, Func<object?, object?> mapDelegate){
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public Type SourceType { get; }

		public Type DestinationType { get; }


		public object? Invoke(object? source) {
			TypeUtils.CheckObjectType(source, SourceType, nameof(source));

			if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);

			return _mapDelegate.Invoke(source);
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

	internal class DefaultNewMapFactory<TSource, TDestination> : NewMapFactory<TSource, TDestination> {
		protected int _disposed = 0;
		private readonly Func<TSource?, TDestination?> _mapDelegate;

		public DefaultNewMapFactory(Func<TSource?, TDestination?> mapDelegate) {
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public override TDestination? Invoke(TSource? source) {
			if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);

			return _mapDelegate.Invoke(source);
		}

		protected override void Dispose(bool disposing) {
			if (disposing)
				Interlocked.CompareExchange(ref _disposed, 1, 0);
		}
	}
}
