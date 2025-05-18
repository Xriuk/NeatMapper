using System;

namespace NeatMapper {
	internal class DisposableMatchMapFactory<TSource, TDestination> : DefaultMatchMapFactory<TSource, TDestination> {
		protected readonly IDisposable?[] _disposables;

		internal DisposableMatchMapFactory(Func<TSource?, TDestination?, bool> mapDelegate, params IDisposable?[] disposables) :
			this(typeof(TSource), typeof(TDestination), mapDelegate, disposables){ }
		protected DisposableMatchMapFactory(Type sourceType, Type destinationType, Func<TSource?, TDestination?, bool> mapDelegate, params IDisposable?[] disposables) :
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

	internal sealed class DisposableMatchMapFactory : DisposableMatchMapFactory<object, object> {
		internal DisposableMatchMapFactory(Type sourceType, Type destinationType, Func<object?, object?, bool> mapDelegate, params IDisposable?[] disposables) :
			base(sourceType, destinationType, mapDelegate, disposables) {}
	}
}
