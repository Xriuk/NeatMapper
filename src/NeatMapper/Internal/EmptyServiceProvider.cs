using System;

namespace NeatMapper {
	internal sealed class EmptyServiceProvider : IServiceProvider {
		internal static readonly EmptyServiceProvider Instance = new EmptyServiceProvider();


		private EmptyServiceProvider() { }


		public object? GetService(Type serviceType) {
			return null;
		}
	}
}
