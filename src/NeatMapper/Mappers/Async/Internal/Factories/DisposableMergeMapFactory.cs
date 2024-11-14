using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	internal class DisposableAsyncMergeMapFactory<TSource, TDestination> : DefaultAsyncMergeMapFactory<TSource, TDestination> {
		protected readonly IDisposable?[] _disposables;

		internal DisposableAsyncMergeMapFactory(Func<TSource?, TDestination?, CancellationToken, Task<TDestination?>> mapDelegate, params IDisposable?[] disposables) :
			this(typeof(TSource), typeof(TDestination), mapDelegate, disposables){ }
		protected DisposableAsyncMergeMapFactory(Type sourceType, Type destinationType, Func<TSource?, TDestination?, CancellationToken, Task<TDestination?>> mapDelegate, params IDisposable?[] disposables) :
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

	internal sealed class DisposableAsyncMergeMapFactory : DisposableAsyncMergeMapFactory<object, object> {
		internal DisposableAsyncMergeMapFactory(Type sourceType, Type destinationType, Func<object?, object?, CancellationToken, Task<object?>> mapDelegate, params IDisposable?[] disposables) :
			base(sourceType, destinationType, mapDelegate, disposables) {}
	}
}
