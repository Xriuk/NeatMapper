using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NeatMapper.Tests {
	public static class TestUtils {
		public static void AssertDuplicateMap(Action action) {
			var exc = Assert.ThrowsException<InvalidOperationException>(action);

			Assert.IsTrue(exc.Message.StartsWith("Duplicate interface") || exc.Message.StartsWith("Duplicate map"));
		}

		public static MapNotFoundException AssertMapNotFound(Func<object> action) {
			return Assert.ThrowsException<MapNotFoundException>(action);
		}

		public static Task<MapNotFoundException> AssertMapNotFound(Func<Task> action) {
			return Assert.ThrowsExceptionAsync<MapNotFoundException>(action);
		}

		public static void AssertExpressionsEqual(LambdaExpression expected, LambdaExpression actual, string message = "Expressions are not equal") {
			var expectedString = ExpressionStringBuilder.ExpressionToString(expected);
			var actualString = ExpressionStringBuilder.ExpressionToString(actual);
			if(expectedString != actualString) { 
				Assert.Fail($"{message}\n" +
					$"Expected:\n{expectedString}\n\n" +
					$"Actual:\n{actualString}");
			}
		}
	}
}
