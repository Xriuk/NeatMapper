#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NeatMapper.Transitive {
	internal class DefaultTransitiveMergeMapFactories<TSource, TDestination> : TransitiveMergeMapFactories<TSource, TDestination> {
		protected int _disposed = 0;
		protected readonly IDisposable[] _disposables;

		internal DefaultTransitiveMergeMapFactories(IEnumerable<TransitiveMergeMapFactory<TSource, TDestination>> factories, params IDisposable[] disposables)
			: this(typeof(TSource), typeof(TDestination), factories, disposables) { }
		protected DefaultTransitiveMergeMapFactories(Type sourceType, Type destinationType, IEnumerable<TransitiveMergeMapFactory<TSource, TDestination>> factories, params IDisposable[] disposables) {
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			Factories = factories?.ToArray() ?? throw new ArgumentNullException(nameof(factories));
			_disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
		}


		public override Type SourceType { get; }

		public override Type DestinationType { get; }

		public override IEnumerable<TransitiveMergeMapFactory<TSource, TDestination>> Factories { get; }


		protected override void Dispose(bool disposing) {
			if (disposing && Interlocked.CompareExchange(ref _disposed, 1, 0) == 0) {
				foreach (var disposable in _disposables) {
					disposable?.Dispose();
				}
			}
		}
	}

	internal sealed class DefaultTransitiveMergeMapFactories : DefaultTransitiveMergeMapFactories<object, object> {
		internal DefaultTransitiveMergeMapFactories(Type sourceType, Type destinationType, IEnumerable<TransitiveMergeMapFactory<object, object>> factories, params IDisposable[] disposables) :
			base(sourceType, destinationType, factories, disposables) { }
	}
}
