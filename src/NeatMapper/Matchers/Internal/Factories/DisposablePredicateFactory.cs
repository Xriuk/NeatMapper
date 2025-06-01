using System;
using System.Threading;

namespace NeatMapper {
	internal sealed class DisposablePredicateFactory : IPredicateFactory {
		private int _disposed = 0;
		private readonly Func<object?, bool> _predicateDelegate;
		private readonly IDisposable?[] _disposables;

		public DisposablePredicateFactory(object? comparand, Type comparandType, Type comparerType, Func<object?, bool> predicateDelegate, params IDisposable?[] disposables) {
			Comparand = comparand;
			ComparandType = comparandType ?? throw new ArgumentNullException(nameof(comparandType));
			ComparerType = comparerType ?? throw new ArgumentNullException(nameof(comparerType));
			_predicateDelegate = predicateDelegate ?? throw new ArgumentNullException(nameof(predicateDelegate));
			_disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
		}


		public object? Comparand { get; }

		public Type ComparandType { get; }

		public Type ComparerType { get; }

		public bool Invoke(object? comparer) {
			TypeUtils.CheckObjectType(comparer, ComparerType, nameof(comparer));

			if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);

			return _predicateDelegate.Invoke(comparer);
		}

		public void Dispose() {
			if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0) {
				foreach (var disposable in _disposables) {
					disposable?.Dispose();
				}
			}
		}
	}

	internal sealed class DisposablePredicateFactory<TComparer> : PredicateFactory<TComparer> {
		private int _disposed = 0;
		private readonly Func<object?, bool> _predicateDelegate;
		private readonly IDisposable?[] _disposables;

		public DisposablePredicateFactory(object? comparand, Type comparandType, Func<object?, bool> predicateDelegate, params IDisposable?[] disposables) {
			Comparand = comparand;
			ComparandType = comparandType ?? throw new ArgumentNullException(nameof(comparandType));
			_predicateDelegate = predicateDelegate ?? throw new ArgumentNullException(nameof(predicateDelegate));
			_disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
		}


		public override object? Comparand { get; }

		public override Type ComparandType { get; }

		public override bool Invoke(TComparer? comparer) {
			TypeUtils.CheckObjectType(comparer, ComparerType, nameof(comparer));

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

	internal sealed class DisposablePredicateFactory<TComparand, TComparer> : PredicateFactory<TComparand, TComparer> {
		private int _disposed = 0;
		private readonly Func<TComparer?, bool> _predicateDelegate;
		private readonly IDisposable?[] _disposables;

		public DisposablePredicateFactory(TComparand? comparand, Func<TComparer?, bool> predicateDelegate, params IDisposable?[] disposables) {
			Comparand = comparand;
			_predicateDelegate = predicateDelegate ?? throw new ArgumentNullException(nameof(predicateDelegate));
			_disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
		}


		public override object? Comparand { get; }

		public override bool Invoke(TComparer? comparer) {
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
}
