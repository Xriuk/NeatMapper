#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading.Tasks;

namespace NeatMapper {
	internal class DisposableAsyncMergeMapFactory<TSource, TDestination> : AsyncMergeMapFactory<TSource, TDestination> {
		protected readonly IDisposable[] _disposables;

		internal DisposableAsyncMergeMapFactory(Func<TSource, TDestination, Task<TDestination>> mapDelegate, params IDisposable[] disposables) :
			this(typeof(TSource), typeof(TDestination), mapDelegate, disposables){ }
		protected DisposableAsyncMergeMapFactory(Type sourceType, Type destinationType, Func<TSource, TDestination, Task<TDestination>> mapDelegate, params IDisposable[] disposables) :
			base(sourceType, destinationType, mapDelegate) {
			_disposables = disposables;
		}


		public override void Dispose() {
			lock (_disposables) {
				base.Dispose();

				foreach (var disposable in _disposables) {
					disposable?.Dispose();
				}
			}
		}
	}

	internal class DisposableAsyncMergeMapFactory : DisposableAsyncMergeMapFactory<object, object> {
		internal DisposableAsyncMergeMapFactory(Type sourceType, Type destinationType, Func<object, object, Task<object>> mapDelegate, params IDisposable[] disposables) :
			base(sourceType, destinationType, mapDelegate, disposables) {}
	}
}
