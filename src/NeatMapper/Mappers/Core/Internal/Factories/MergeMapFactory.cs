#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;

namespace NeatMapper {
	internal class MergeMapFactory<TSource, TDestination> : IMergeMapFactory<TSource, TDestination> {
		private int _disposed = 0;
		private readonly Func<TSource, TDestination, TDestination> _mapDelegate;

		internal MergeMapFactory(Func<TSource, TDestination, TDestination> mapDelegate)
			: this(typeof(TSource), typeof(TDestination), mapDelegate) { }
		protected MergeMapFactory(Type sourceType, Type destinationType, Func<TSource, TDestination, TDestination> mapDelegate) {
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public Type SourceType { get; }

		public Type DestinationType { get; }


		public TDestination Invoke(TSource source, TDestination destination) {
			if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);
			return _mapDelegate.Invoke(source, destination);
		}

		public virtual void Dispose() {
			if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
				throw new ObjectDisposedException(null);
		}

		object IMergeMapFactory.Invoke(object source, object destination) {
			return Invoke((TSource)source, (TDestination)destination);
		}
	}

	internal class MergeMapFactory : MergeMapFactory<object, object> {
		internal MergeMapFactory(Type sourceType, Type destinationType, Func<object, object, object> mapDelegate) :
			base(sourceType, destinationType, mapDelegate) {}
	}
}
