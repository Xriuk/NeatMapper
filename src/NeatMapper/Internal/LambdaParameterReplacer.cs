#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper {
	internal class LambdaParameterReplacer : ExpressionVisitor {
		private ParameterExpression[] parameters;
		private readonly Expression[] replacements;

		public LambdaParameterReplacer(params Expression[] replacements) {
			this.parameters = Array.Empty<ParameterExpression>();
			this.replacements = replacements;
		}


		override protected Expression VisitParameter(ParameterExpression node) {
			var parameter = Array.IndexOf(parameters, node);
			if (parameter < 0 || parameter >= parameters.Length)
				return base.VisitParameter(node);

			if (replacements[parameter].Type != parameters[parameter].Type)
				return Expression.Convert(replacements[parameter], parameters[parameter].Type);
			else
				return replacements[parameter];
		}

		public Expression SetupAndVisitBody(LambdaExpression lambda) {
			if(lambda.Parameters.Count != replacements.Length)
				throw new ArgumentException("Mismatching number of lambda parameters and replacements", nameof(lambda));
			parameters = lambda.Parameters.ToArray();
			return Visit(lambda.Body);
		}
	}
}
