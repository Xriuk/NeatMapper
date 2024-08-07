﻿#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	internal class DisposableAsyncNewMapFactory<TSource, TDestination> : DefaultAsyncNewMapFactory<TSource, TDestination> {
		protected readonly IDisposable[] _disposables;

		internal DisposableAsyncNewMapFactory(Func<TSource, CancellationToken, Task<TDestination>> mapDelegate, params IDisposable[] disposables) :
			this(typeof(TSource), typeof(TDestination), mapDelegate, disposables){ }
		protected DisposableAsyncNewMapFactory(Type sourceType, Type destinationType, Func<TSource, CancellationToken, Task<TDestination>> mapDelegate, params IDisposable[] disposables) :
			base(sourceType, destinationType, mapDelegate) {

			_disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
		}


		protected override void Dispose(bool disposing) {
			lock (_disposables) { 
				base.Dispose(disposing);

				if (disposing) {
					foreach (var disposable in _disposables) {
						disposable?.Dispose();
					}
				}
			}
		}
	}

	internal class DisposableAsyncNewMapFactory : DisposableAsyncNewMapFactory<object, object> {
		internal DisposableAsyncNewMapFactory(Type sourceType, Type destinationType, Func<object, CancellationToken, Task<object>> mapDelegate, params IDisposable[] disposables) :
			base(sourceType, destinationType, mapDelegate, disposables) {}
	}
}
