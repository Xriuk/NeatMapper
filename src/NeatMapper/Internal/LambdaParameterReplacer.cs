using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace NeatMapper {
	internal sealed class LambdaParameterReplacer : ExpressionVisitor {
		private ReadOnlyCollection<ParameterExpression>? parameters;
		private readonly Expression[] replacements;

		public LambdaParameterReplacer(params Expression[] replacements) {
			this.replacements = replacements;
		}


		override protected Expression VisitParameter(ParameterExpression node) {
			var parameter = parameters?.IndexOf(node);
			if (parameter == null || parameter < 0 || parameter >= parameters!.Count)
				return base.VisitParameter(node);

			if (replacements[parameter.Value].Type != parameters[parameter.Value].Type)
				return Expression.Convert(replacements[parameter.Value], parameters[parameter.Value].Type);
			else
				return replacements[parameter.Value];
		}

		public Expression SetupAndVisitBody(LambdaExpression lambda) {
			if(lambda.Parameters.Count != replacements.Length)
				throw new ArgumentException("Mismatching number of lambda parameters and replacements", nameof(lambda));
			parameters = lambda.Parameters;
			return Visit(lambda.Body);
		}
	}
}
