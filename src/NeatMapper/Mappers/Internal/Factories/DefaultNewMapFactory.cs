using System;
using System.Threading;

namespace NeatMapper {
	internal class DefaultNewMapFactory<TSource, TDestination> : NewMapFactory<TSource, TDestination> {
		protected int _disposed = 0;
		private readonly Func<TSource?, TDestination?> _mapDelegate;

		internal DefaultNewMapFactory(Func<TSource?, TDestination?> mapDelegate) 
			: this(typeof(TSource), typeof(TDestination), mapDelegate) { }
		protected DefaultNewMapFactory(Type sourceType, Type destinationType, Func<TSource?, TDestination?> mapDelegate) {
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public override Type SourceType { get; }

		public override Type DestinationType { get; }


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

	internal sealed class DefaultNewMapFactory : DefaultNewMapFactory<object, object> {
		internal DefaultNewMapFactory(Type sourceType, Type destinationType, Func<object?, object?> mapDelegate) :
			base(sourceType, destinationType, mapDelegate) {}
	}
}
