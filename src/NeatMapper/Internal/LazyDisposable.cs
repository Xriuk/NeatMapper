using System;

namespace NeatMapper {
	internal sealed class LazyDisposable : IDisposable {
		private readonly Action _disposeCallback;

		public LazyDisposable(Action disposeCallback) {
			_disposeCallback = disposeCallback;
		}

		public void Dispose() {
			_disposeCallback.Invoke();
		}
	}
}
