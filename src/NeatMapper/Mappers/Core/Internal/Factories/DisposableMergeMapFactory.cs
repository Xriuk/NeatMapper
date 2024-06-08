#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

namespace NeatMapper {
	internal class DisposableMergeMapFactory<TSource, TDestination> : DefaultMergeMapFactory<TSource, TDestination> {
		protected readonly IDisposable[] _disposables;

		internal DisposableMergeMapFactory(Func<TSource, TDestination, TDestination> mapDelegate, params IDisposable[] disposables) :
			this(typeof(TSource), typeof(TDestination), mapDelegate, disposables){ }
		protected DisposableMergeMapFactory(Type sourceType, Type destinationType, Func<TSource, TDestination, TDestination> mapDelegate, params IDisposable[] disposables) :
			base(sourceType, destinationType, mapDelegate) {

			_disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
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

	internal class DisposableMergeMapFactory : DisposableMergeMapFactory<object, object> {
		internal DisposableMergeMapFactory(Type sourceType, Type destinationType, Func<object, object, object> mapDelegate, params IDisposable[] disposables) :
			base(sourceType, destinationType, mapDelegate, disposables) {}
	}
}
