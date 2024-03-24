#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;

namespace NeatMapper {
	internal class NewMapFactory<TSource, TDestination> : INewMapFactory<TSource, TDestination> {
		protected int _disposed = 0;
		protected readonly Func<TSource, TDestination> _mapDelegate;

		internal NewMapFactory(Func<TSource, TDestination> mapDelegate) 
			: this(typeof(TSource), typeof(TDestination), mapDelegate) { }
		protected NewMapFactory(Type sourceType, Type destinationType, Func<TSource, TDestination> mapDelegate) {
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public Type SourceType { get; }

		public Type DestinationType { get; }


		public TDestination Invoke(TSource source) {
			if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);
			return _mapDelegate.Invoke(source);
		}

		public virtual void Dispose() {
			if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
				throw new ObjectDisposedException(null);
		}


		object INewMapFactory.Invoke(object source) {
			return Invoke((TSource)source);
		}
	}

	internal class NewMapFactory : NewMapFactory<object, object> {
		internal NewMapFactory(Type sourceType, Type destinationType, Func<object, object> mapDelegate) :
			base(sourceType, destinationType, mapDelegate) {}
	}
}
