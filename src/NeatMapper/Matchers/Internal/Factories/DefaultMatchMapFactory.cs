#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;

namespace NeatMapper {
	internal class DefaultMatchMapFactory<TSource, TDestination> : MatchMapFactory<TSource, TDestination> {
		protected int _disposed = 0;
		protected readonly Func<TSource, TDestination, bool> _mapDelegate;

		internal DefaultMatchMapFactory(Func<TSource, TDestination, bool> mapDelegate)
			: this(typeof(TSource), typeof(TDestination), mapDelegate) { }
		protected DefaultMatchMapFactory(Type sourceType, Type destinationType, Func<TSource, TDestination, bool> mapDelegate) {
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public override Type SourceType { get; }

		public override Type DestinationType { get; }


		public override bool Invoke(TSource source, TDestination destination) {
			if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);
			return _mapDelegate.Invoke(source, destination);
		}

		protected override void Dispose(bool disposing) {
			if (disposing && Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
				throw new ObjectDisposedException(null);
		}
	}

	internal class DefaultMatchMapFactory : DefaultMatchMapFactory<object, object> {
		internal DefaultMatchMapFactory(Type sourceType, Type destinationType, Func<object, object, bool> mapDelegate) :
			base(sourceType, destinationType, mapDelegate) { }
	}
}
