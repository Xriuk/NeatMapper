#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace NeatMapper {
	internal sealed class CachedLazyEnumerable<T> : IEnumerable<T>, IDisposable where T : IDisposable {
		private int _disposed = 0;
		private IEnumerator<T> _enumerator;
		private readonly ConcurrentBag<T> _cache = new ConcurrentBag<T>();


		internal CachedLazyEnumerable(IEnumerable<T> enumerable) {
			_enumerator = enumerable.GetEnumerator();
		}


		public IEnumerator<T> GetEnumerator() {
			if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
				throw new ObjectDisposedException(null);

			// Enumerate the cache
			foreach (var cachedElement in _cache) {
				yield return cachedElement;
			}

			// Enumerate the collection, cache it and dispose enumerator
			while (true) {
				T current;
				lock (_cache) {
					if(_enumerator == null || !_enumerator.MoveNext())
						break;
					current = _enumerator.Current;
					_cache.Add(current);
				}

				yield return current;
			}

			lock (_cache) {
				DisposeEnumerator();
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		private void DisposeEnumerator() {
			_enumerator?.Dispose();
			_enumerator = null;
		}

		public void Dispose() {
			if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1) {
				lock (_cache) {
					DisposeEnumerator();

					foreach (var factory in _cache) {
						factory.Dispose();
					}

#if NET47_OR_GREATER
					while (!_cache.IsEmpty) {
						_cache.TryTake(out var _);
					}
#else
					_cache.Clear();
#endif
				}
			}
		}
	}
}
