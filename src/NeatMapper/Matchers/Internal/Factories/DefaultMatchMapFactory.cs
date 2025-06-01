using System;
using System.Threading;

namespace NeatMapper {
	internal class DefaultMatchMapFactory : IMatchMapFactory {
		protected int _disposed = 0;
		private readonly Func<object?, object?, bool> _mapDelegate;

		public DefaultMatchMapFactory(Type sourceType, Type destinationType, Func<object?, object?, bool> mapDelegate) {
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public Type SourceType { get; }

		public Type DestinationType { get; }

		public bool Invoke(object? source, object? destination) {
			TypeUtils.CheckObjectType(source, SourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, DestinationType, nameof(destination));

			if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);

			return _mapDelegate.Invoke(source, destination);
		}

		protected virtual void Dispose(bool disposing){
			if (disposing)
				Interlocked.CompareExchange(ref _disposed, 1, 0);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}

	internal class DefaultMatchMapFactory<TSource, TDestination> : MatchMapFactory<TSource, TDestination> {
		protected int _disposed = 0;
		private readonly Func<TSource?, TDestination?, bool> _mapDelegate;

		public DefaultMatchMapFactory(Func<TSource?, TDestination?, bool> mapDelegate){
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public override bool Invoke(TSource? source, TDestination? destination) {
			if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);

			return _mapDelegate.Invoke(source, destination);
		}

		protected override void Dispose(bool disposing) {
			if (disposing)
				Interlocked.CompareExchange(ref _disposed, 1, 0);
		}
	}
}
