using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NeatMapper.Tests {
	internal static class TestUtils {
		public static void AssertMapNotFound(Func<object> action) {
			var exc = Assert.ThrowsException<ArgumentException>(action);
			Assert.IsTrue(exc.Message.StartsWith("No map could be found for the given types"));
		}

		public async static Task AssertMapNotFound(Func<Task> action) {
			var exc = await Assert.ThrowsExceptionAsync<ArgumentException>(action);
			Assert.IsTrue(exc.Message.StartsWith("No map could be found for the given types"));
		}
	}
}
