using System;

namespace NeatMapper {
	internal sealed class DisposableMatchMapFactory : DefaultMatchMapFactory {
		private readonly IDisposable?[] _disposables;

		public DisposableMatchMapFactory(Type sourceType, Type destinationType, Func<object?, object?, bool> mapDelegate, params IDisposable?[] disposables) :
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

	internal sealed class DisposableMatchMapFactory<TSource, TDestination> : DefaultMatchMapFactory<TSource, TDestination> {
		private readonly IDisposable?[] _disposables;

		public DisposableMatchMapFactory(Func<TSource?, TDestination?, bool> mapDelegate, params IDisposable?[] disposables) :
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
