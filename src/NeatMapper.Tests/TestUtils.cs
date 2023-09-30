using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core.Mapper;

namespace NeatMapper.Tests {
	public static class TestUtils {
		public static void AssertMapNotFound(Func<object?> action) {
			Assert.ThrowsException<MapNotFoundException>(action);
		}

		public static Task AssertMapNotFound(Func<Task> action) {
			return Assert.ThrowsExceptionAsync<MapNotFoundException>(action);
		}
	}
}
