#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;

namespace NeatMapper {
	internal class MatchMapFactory<TSource, TDestination> : IMatchMapFactory<TSource, TDestination> {
		protected int _disposed = 0;
		protected readonly Func<TSource, TDestination, bool> _mapDelegate;

		internal MatchMapFactory(Func<TSource, TDestination, bool> mapDelegate)
			: this(typeof(TSource), typeof(TDestination), mapDelegate) { }
		protected MatchMapFactory(Type sourceType, Type destinationType, Func<TSource, TDestination, bool> mapDelegate) {
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public Type SourceType { get; }

		public Type DestinationType { get; }


		public bool Invoke(TSource source, TDestination destination) {
			if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);
			return _mapDelegate.Invoke(source, destination);
		}

		public virtual void Dispose() {
			if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
				throw new ObjectDisposedException(null);
		}


		bool IMatchMapFactory.Invoke(object source, object destination) {
			return Invoke((TSource)source, (TDestination)destination);
		}
	}

	internal class MatchMapFactory : MatchMapFactory<object, object> {
		internal MatchMapFactory(Type sourceType, Type destinationType, Func<object, object, bool> mapDelegate) :
			base(sourceType, destinationType, mapDelegate) { }
	}
}
