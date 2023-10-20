using System;

namespace NeatMapper.Internal {
	internal class EmptyServiceProvider : IServiceProvider {
		internal static readonly EmptyServiceProvider Instance = new EmptyServiceProvider();


		private EmptyServiceProvider() { }


		public object GetService(Type serviceType) {
			throw new NotImplementedException();
		}
	}
}
