using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	internal sealed class DisposableAsyncMergeMapFactory : DefaultAsyncMergeMapFactory {
		private readonly IDisposable?[] _disposables;

		public DisposableAsyncMergeMapFactory(Type sourceType, Type destinationType, Func<object?, object?, CancellationToken, Task<object?>> mapDelegate, params IDisposable?[] disposables) :
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

	internal sealed class DisposableAsyncMergeMapFactory<TSource, TDestination> : DefaultAsyncMergeMapFactory<TSource, TDestination> {
		private readonly IDisposable?[] _disposables;

		public DisposableAsyncMergeMapFactory(Func<TSource?, TDestination?, CancellationToken, Task<TDestination?>> mapDelegate, params IDisposable?[] disposables) :
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
