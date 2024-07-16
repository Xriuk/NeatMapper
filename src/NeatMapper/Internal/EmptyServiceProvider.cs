#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

namespace NeatMapper {
	internal class EmptyServiceProvider : IServiceProvider {
		internal static readonly EmptyServiceProvider Instance = new EmptyServiceProvider();


		private EmptyServiceProvider() { }


		public object GetService(Type serviceType) {
			return null;
		}
	}
}
