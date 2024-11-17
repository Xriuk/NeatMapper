using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	internal sealed class DefaultAsyncEnumerable<T> : IAsyncEnumerable<T> {
		private class DefaultAsyncEnumerator : IAsyncEnumerator<T>, IDisposable {
			private readonly IEnumerator<T> _enumerator;

			public DefaultAsyncEnumerator(IEnumerator<T> enumerator) {
				_enumerator = enumerator;
			}

			public T Current => _enumerator.Current;

			public void Dispose() {
				_enumerator.Dispose();
			}

			public ValueTask DisposeAsync() {
				((IDisposable)this).Dispose();
#if NET5_0_OR_GREATER
				return ValueTask.CompletedTask;
#else
				return default;
#endif
			}

			public ValueTask<bool> MoveNextAsync() {
#if NET5_0_OR_GREATER
				return ValueTask.FromResult(_enumerator.MoveNext());
#else
				return new ValueTask<bool>(_enumerator.MoveNext());
#endif
			}
		}


		public IEnumerable<T> _enumerable;

		public DefaultAsyncEnumerable(IEnumerable<T> enumerable) {
			_enumerable = enumerable;
		}

		public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken) {
			return new DefaultAsyncEnumerator(_enumerable.GetEnumerator());
		}
	}
}
