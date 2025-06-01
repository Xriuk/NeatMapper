using System;

namespace NeatMapper {
	internal sealed class DisposableMergeMapFactory : DefaultMergeMapFactory {
		private readonly IDisposable?[] _disposables;

		public DisposableMergeMapFactory(Type sourceType, Type destinationType, Func<object?, object?, object?> mapDelegate, params IDisposable?[] disposables) :
			base(sourceType, destinationType, mapDelegate) {

			_disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
		}


		protected override void Dispose(bool disposing) {
			// We are under lock, no one can access the parent so we can safely check if if was disposed without Interlocked
			lock (_disposables) {
				if (disposing && _disposed != 1) {
					foreach (var disposable in _disposables) {
						disposable?.Dispose();
					}
				}
			}

			base.Dispose(disposing);
		}
	}

	internal sealed class DisposableMergeMapFactory<TSource, TDestination> : DefaultMergeMapFactory<TSource, TDestination> {
		private readonly IDisposable?[] _disposables;

		public DisposableMergeMapFactory(Func<TSource?, TDestination?, TDestination?> mapDelegate, params IDisposable?[] disposables) :
			base(mapDelegate) {

			_disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
		}


		protected override void Dispose(bool disposing) {
			// We are under lock, no one can access the parent so we can safely check if if was disposed without Interlocked
			lock (_disposables) {
				if (disposing && _disposed != 1) { 
					foreach (var disposable in _disposables) {
						disposable?.Dispose();
					}
				}
			}

			base.Dispose(disposing);
		}
	}
}
