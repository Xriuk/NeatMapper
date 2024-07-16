using System;

namespace NeatMapper {
	internal class LambdaDisposable : IDisposable {
		private readonly Action _dispose;

		internal LambdaDisposable(Action dispose) {
			_dispose = dispose;
		}


		private void Dispose(bool disposing) {
			if (disposing) 
				_dispose.Invoke();
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
