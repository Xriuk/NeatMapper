using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	internal class DisposableAsyncNewMapFactory<TSource, TDestination> : DefaultAsyncNewMapFactory<TSource, TDestination> {
		protected readonly IDisposable?[] _disposables;

		internal DisposableAsyncNewMapFactory(Func<TSource?, CancellationToken, Task<TDestination?>> mapDelegate, params IDisposable?[] disposables) :
			this(typeof(TSource), typeof(TDestination), mapDelegate, disposables){ }
		protected DisposableAsyncNewMapFactory(Type sourceType, Type destinationType, Func<TSource?, CancellationToken, Task<TDestination?>> mapDelegate, params IDisposable?[] disposables) :
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

	internal sealed class DisposableAsyncNewMapFactory : DisposableAsyncNewMapFactory<object, object> {
		internal DisposableAsyncNewMapFactory(Type sourceType, Type destinationType, Func<object?, CancellationToken, Task<object?>> mapDelegate, params IDisposable?[] disposables) :
			base(sourceType, destinationType, mapDelegate, disposables) {}
	}
}
