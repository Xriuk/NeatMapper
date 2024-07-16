#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NeatMapper {
	internal sealed class CachedLazyEnumerable<T> : IEnumerable<T>, IDisposable {
		private IEnumerator<T> _enumerator;
		private readonly ConcurrentBag<T> _cache = new ConcurrentBag<T>();

		public IEnumerable<T> Cached => _cache;

		internal CachedLazyEnumerable(IEnumerable<T> enumerable) {
			_enumerator = enumerable.GetEnumerator();
		}


		public IEnumerator<T> GetEnumerator() {
			// Enumerate the cache
			foreach (var cachedElement in _cache) {
				yield return cachedElement;
			}

			// Enumerate the collection
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

			Dispose();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				lock (_cache) {
					_enumerator?.Dispose();
					_enumerator = null;
				}
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
