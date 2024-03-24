using System;

namespace NeatMapper {
	internal class LambdaDisposable : IDisposable {
		private readonly Action _dispose;

		internal LambdaDisposable(Action dispose) {
			_dispose = dispose;
		}


		public void Dispose() {
			_dispose.Invoke();
		}
	}
}
