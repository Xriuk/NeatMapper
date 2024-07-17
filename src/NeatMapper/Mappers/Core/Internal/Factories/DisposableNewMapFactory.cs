#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

namespace NeatMapper {
	internal class DisposableNewMapFactory<TSource, TDestination> : DefaultNewMapFactory<TSource, TDestination> {
		protected readonly IDisposable[] _disposables;

		internal DisposableNewMapFactory(Func<TSource, TDestination> mapDelegate, params IDisposable[] disposables) :
			this(typeof(TSource), typeof(TDestination), mapDelegate, disposables){ }
		protected DisposableNewMapFactory(Type sourceType, Type destinationType, Func<TSource, TDestination> mapDelegate, params IDisposable[] disposables) :
			base(sourceType, destinationType, mapDelegate) {

			_disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
		}


		protected override void Dispose(bool disposing) {
			// We are under lock, no one can access the parent so we can safely check if if was disposed without Interlocked
			lock (_disposables) { 
				if (disposing && _disposed != 1) { 
					foreach(var disposable in _disposables) {
						disposable?.Dispose();
					}
				}
			}

			base.Dispose(disposing);
		}
	}

	internal sealed class DisposableNewMapFactory : DisposableNewMapFactory<object, object> {
		internal DisposableNewMapFactory(Type sourceType, Type destinationType, Func<object, object> mapDelegate, params IDisposable[] disposables) :
			base(sourceType, destinationType, mapDelegate, disposables) {}
	}
}
