#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

namespace NeatMapper {
	internal sealed class LambdaDisposable : IDisposable {
		private Action _dispose;

		internal LambdaDisposable(Action dispose) {
			_dispose = dispose
				?? throw new ArgumentNullException(nameof(dispose));
		}


		private void Dispose(bool disposing) {
			if (disposing) { 
				_dispose?.Invoke();
				_dispose = null;
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
