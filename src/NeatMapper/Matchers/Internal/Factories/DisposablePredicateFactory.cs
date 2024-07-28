#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;

namespace NeatMapper {
	internal class DisposablePredicateFactory<TComparer> : PredicateFactory<TComparer> {
		protected int _disposed = 0;
		private readonly Func<TComparer, bool> _predicateDelegate;
		protected readonly IDisposable[] _disposables;

		internal DisposablePredicateFactory(Type comparandType, Func<TComparer, bool> predicateDelegate, params IDisposable[] disposables)
			: this(comparandType, typeof(TComparer), predicateDelegate, disposables) { }
		protected DisposablePredicateFactory(Type comparandType, Type comparerType, Func<TComparer, bool> predicateDelegate, params IDisposable[] disposables) {
			ComparandType = comparandType ?? throw new ArgumentNullException(nameof(comparandType));
			ComparerType = comparerType ?? throw new ArgumentNullException(nameof(comparerType));
			_predicateDelegate = predicateDelegate ?? throw new ArgumentNullException(nameof(predicateDelegate));
			_disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
		}


		public override Type ComparerType { get; }

		public override Type ComparandType { get; }


		public override bool Invoke(TComparer comparer) {
			if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);

			return _predicateDelegate.Invoke(comparer);
		}

		protected override void Dispose(bool disposing) {
			if (disposing && Interlocked.CompareExchange(ref _disposed, 1, 0) == 0) {
				foreach (var disposable in _disposables) {
					disposable?.Dispose();
				}
			}
		}
	}

	internal sealed class DisposablePredicateFactory : DisposablePredicateFactory<object> {
		internal DisposablePredicateFactory(Type comparandType, Type comparerType, Func<object, bool> predicateDelegate, params IDisposable[] disposables) :
			base(comparandType, comparerType, predicateDelegate, disposables) { }
	}
}
