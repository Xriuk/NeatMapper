#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;

namespace NeatMapper {
	internal class DefaultMergeMapFactory<TSource, TDestination> : MergeMapFactory<TSource, TDestination> {
		protected int _disposed = 0;
		private readonly Func<TSource, TDestination, TDestination> _mapDelegate;

		internal DefaultMergeMapFactory(Func<TSource, TDestination, TDestination> mapDelegate)
			: this(typeof(TSource), typeof(TDestination), mapDelegate) { }
		protected DefaultMergeMapFactory(Type sourceType, Type destinationType, Func<TSource, TDestination, TDestination> mapDelegate) {
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public override Type SourceType { get; }

		public override Type DestinationType { get; }


		public override TDestination Invoke(TSource source, TDestination destination) {
			if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);

			return _mapDelegate.Invoke(source, destination);
		}

		protected override void Dispose(bool disposing) {
			if (disposing)
				Interlocked.CompareExchange(ref _disposed, 1, 0);
		}
	}

	internal sealed class DefaultMergeMapFactory : DefaultMergeMapFactory<object, object> {
		internal DefaultMergeMapFactory(Type sourceType, Type destinationType, Func<object, object, object> mapDelegate) :
			base(sourceType, destinationType, mapDelegate) {}
	}
}
