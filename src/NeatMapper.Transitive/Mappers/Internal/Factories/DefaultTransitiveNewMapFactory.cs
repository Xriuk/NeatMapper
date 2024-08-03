#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Generic;
using System.Threading;

namespace NeatMapper.Transitive {
	internal class DefaultTransitiveNewMapFactory<TSource, TDestination> : TransitiveNewMapFactory<TSource, TDestination> {
		protected int _disposed = 0;
		private readonly Func<TSource, TDestination> _mapDelegate;
		protected readonly IDisposable[] _disposables;

		internal DefaultTransitiveNewMapFactory(IList<Type> types, Func<TSource, TDestination> mapDelegate, params IDisposable[] disposables)
			: this(typeof(TSource), typeof(TDestination), types, mapDelegate, disposables) { }
		protected DefaultTransitiveNewMapFactory(Type sourceType, Type destinationType, IList<Type> types, Func<TSource, TDestination> mapDelegate, params IDisposable[] disposables) {
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			Types = types ?? throw new ArgumentNullException(nameof(types));
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
			_disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));

			if (Types.Count < 2 || Types[0] != SourceType || Types[Types.Count - 1] != DestinationType)
				throw new ArgumentException("Types must include source and destination types.", nameof(types));
		}


		public override Type SourceType { get; }

		public override Type DestinationType { get; }

		public override IList<Type> Types { get; }


		public override TDestination Invoke(TSource source) {
			if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);

			return _mapDelegate.Invoke(source);
		}

		protected override void Dispose(bool disposing) {
			if (disposing && Interlocked.CompareExchange(ref _disposed, 1, 0) == 0) {
				foreach (var disposable in _disposables) {
					disposable?.Dispose();
				}
			}
		}
	}

	internal sealed class DefaultTransitiveNewMapFactory : DefaultTransitiveNewMapFactory<object, object> {
		internal DefaultTransitiveNewMapFactory(Type sourceType, Type destinationType, IList<Type> types, Func<object, object> mapDelegate, params IDisposable[] disposables) :
			base(sourceType, destinationType, types, mapDelegate, disposables) { }
	}
}
