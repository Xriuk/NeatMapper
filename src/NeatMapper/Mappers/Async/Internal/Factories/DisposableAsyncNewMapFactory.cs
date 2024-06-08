#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading.Tasks;

namespace NeatMapper {
	internal class DisposableAsyncNewMapFactory<TSource, TDestination> : DefaultAsyncNewMapFactory<TSource, TDestination> {
		protected readonly IDisposable[] _disposables;

		internal DisposableAsyncNewMapFactory(Func<TSource, Task<TDestination>> mapDelegate, params IDisposable[] disposables) :
			this(typeof(TSource), typeof(TDestination), mapDelegate, disposables){ }
		protected DisposableAsyncNewMapFactory(Type sourceType, Type destinationType, Func<TSource, Task<TDestination>> mapDelegate, params IDisposable[] disposables) :
			base(sourceType, destinationType, mapDelegate) {

			_disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
		}


		public override void Dispose() {
			lock (_disposables) { 
				base.Dispose();

				foreach(var disposable in _disposables) {
					disposable?.Dispose();
				}
			}
		}
	}

	internal class DisposableAsyncNewMapFactory : DisposableAsyncNewMapFactory<object, object> {
		internal DisposableAsyncNewMapFactory(Type sourceType, Type destinationType, Func<object, Task<object>> mapDelegate, params IDisposable[] disposables) :
			base(sourceType, destinationType, mapDelegate, disposables) {}
	}
}
