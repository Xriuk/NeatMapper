#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

namespace NeatMapper {
	internal class DisposableMatchMapFactory<TSource, TDestination> : MatchMapFactory<TSource, TDestination> {
		protected readonly IDisposable[] _disposables;

		internal DisposableMatchMapFactory(Func<TSource, TDestination, bool> mapDelegate, params IDisposable[] disposables) :
			this(typeof(TSource), typeof(TDestination), mapDelegate, disposables){ }
		protected DisposableMatchMapFactory(Type sourceType, Type destinationType, Func<TSource, TDestination, bool> mapDelegate, params IDisposable[] disposables) :
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

	internal class DisposableMatchMapFactory : DisposableMatchMapFactory<object, object> {
		internal DisposableMatchMapFactory(Type sourceType, Type destinationType, Func<object, object, bool> mapDelegate, params IDisposable[] disposables) :
			base(sourceType, destinationType, mapDelegate, disposables) {}
	}
}
