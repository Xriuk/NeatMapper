using System;

namespace NeatMapper {
	internal class DisposableMergeMapFactory<TSource, TDestination> : DefaultMergeMapFactory<TSource, TDestination> {
		protected readonly IDisposable?[] _disposables;

		internal DisposableMergeMapFactory(Func<TSource?, TDestination?, TDestination?> mapDelegate, params IDisposable?[] disposables) :
			this(typeof(TSource), typeof(TDestination), mapDelegate, disposables){ }
		protected DisposableMergeMapFactory(Type sourceType, Type destinationType, Func<TSource?, TDestination?, TDestination?> mapDelegate, params IDisposable?[] disposables) :
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

	internal sealed class DisposableMergeMapFactory : DisposableMergeMapFactory<object, object> {
		internal DisposableMergeMapFactory(Type sourceType, Type destinationType, Func<object?, object?, object?> mapDelegate, params IDisposable?[] disposables) :
			base(sourceType, destinationType, mapDelegate, disposables) {}
	}
}
