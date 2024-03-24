#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	internal class AsyncNewMapFactory<TSource, TDestination> : IAsyncNewMapFactory<TSource, TDestination> {
		protected int _disposed = 0;
		protected readonly Func<TSource, Task<TDestination>> _mapDelegate;

		internal AsyncNewMapFactory(Func<TSource, Task<TDestination>> mapDelegate) 
			: this(typeof(TSource), typeof(TDestination), mapDelegate) { }
		protected AsyncNewMapFactory(Type sourceType, Type destinationType, Func<TSource, Task<TDestination>> mapDelegate) {
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public Type SourceType { get; }

		public Type DestinationType { get; }


		public Task<TDestination> Invoke(TSource source) {
			if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);
			return _mapDelegate.Invoke(source);
		}

		public virtual void Dispose() {
			if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
				throw new ObjectDisposedException(null);
		}


		async Task<object> IAsyncNewMapFactory.Invoke(object source) {
			return await Invoke((TSource)source);
		}
	}

	internal class AsyncNewMapFactory : AsyncNewMapFactory<object, object> {
		internal AsyncNewMapFactory(Type sourceType, Type destinationType, Func<object, Task<object>> mapDelegate) :
			base(sourceType, destinationType, mapDelegate) {}
	}
}
