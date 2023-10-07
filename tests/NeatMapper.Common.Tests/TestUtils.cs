using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace NeatMapper.Tests {
	public static class TestUtils {
		public static void AssertDuplicateMap(Action action) {
			var exc = Assert.ThrowsException<InvalidOperationException>(action);

			Assert.IsTrue(exc.Message.StartsWith("Duplicate interface") || exc.Message.StartsWith("Duplicate map"));
		}

		public static void AssertMapNotFound(Func<object> action) {
			Assert.ThrowsException<MapNotFoundException>(action);
		}

		public static Task AssertMapNotFound(Func<Task> action) {
			return Assert.ThrowsExceptionAsync<MapNotFoundException>(action);
		}
	}
}
