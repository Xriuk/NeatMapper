﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	internal class DefaultAsyncNewMapFactory<TSource, TDestination> : AsyncNewMapFactory<TSource, TDestination> {
		protected int _disposed = 0;
		private readonly Func<TSource?, CancellationToken, Task<TDestination?>> _mapDelegate;

		internal DefaultAsyncNewMapFactory(Func<TSource?, CancellationToken, Task<TDestination?>> mapDelegate) 
			: this(typeof(TSource), typeof(TDestination), mapDelegate) { }
		protected DefaultAsyncNewMapFactory(Type sourceType, Type destinationType, Func<TSource?, CancellationToken, Task<TDestination?>> mapDelegate) {
			SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
			_mapDelegate = mapDelegate ?? throw new ArgumentNullException(nameof(mapDelegate));
		}


		public override Type SourceType { get; }

		public override Type DestinationType { get; }


		public override Task<TDestination?> Invoke(TSource? source, CancellationToken cancellationToken) {
			if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);
			return _mapDelegate.Invoke(source, cancellationToken);
		}

		protected override void Dispose(bool disposing) {
			if (disposing)
				Interlocked.CompareExchange(ref _disposed, 1, 0);
		}
	}

	internal sealed class DefaultAsyncNewMapFactory : DefaultAsyncNewMapFactory<object, object> {
		internal DefaultAsyncNewMapFactory(Type sourceType, Type destinationType, Func<object?, CancellationToken, Task<object?>> mapDelegate) :
			base(sourceType, destinationType, mapDelegate) {}
	}
}
