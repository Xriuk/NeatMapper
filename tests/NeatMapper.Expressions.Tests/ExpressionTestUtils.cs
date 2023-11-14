using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace NeatMapper.Expressions.Tests {
	public static class ExpressionTestUtils {
		public static void AssertExpressionsEqual(LambdaExpression expected, LambdaExpression actual, string message = "Expressions are not equal") {
			var expectedString = ExpressionStringBuilder.ExpressionToString(expected);
			var actualString = ExpressionStringBuilder.ExpressionToString(actual);
			Assert.AreEqual(expectedString, actualString, $"{message}\n" +
				$"Expected:\n{expectedString}\n\n" +
				$"Actual:\n{actualString}");
		}
	}
}
