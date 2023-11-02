#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System.Linq.Expressions;
using System;
using System.Linq;
using System.Collections.Generic;

namespace NeatMapper.EntityFrameworkCore {
	internal static class ExpressionUtils {
		internal class LambdaParameterJoiner : ExpressionVisitor {
			Expression[] parameters;
			ParameterExpression[] parametersToReplace = Array.Empty<ParameterExpression>();

			public LambdaParameterJoiner(params Expression[] parameters) {
				this.parameters = parameters;
			}

			override protected Expression VisitParameter(ParameterExpression node) {
				var parameter = Array.IndexOf(parametersToReplace, node);
				if (parameter < 0 || parameter >= parameters.Length)
					return base.VisitParameter(node);

				return parameters[parameter];
			}

			public Expression SetupAndVisitBody(LambdaExpression lambda) {
				// Map parameters order to their instances
				parametersToReplace = lambda.Parameters.ToArray();

				return Visit(lambda.Body);
			}
		}


		public static LambdaExpression Or(IEnumerable<LambdaExpression> exprs) {
			if(!exprs.Any())
				throw new ArgumentException();
			if (exprs.Any(e => e.Type != exprs.First().Type))
				throw new ArgumentException("Different lambda types");
			if(exprs.Count() == 1)
				return exprs.Single();

			var parameters = exprs.First().Parameters;
			var joiner = new LambdaParameterJoiner(parameters.ToArray());
			return Expression.Lambda(exprs.Select(e => joiner.SetupAndVisitBody(e)).Aggregate(Expression.OrElse), parameters);
		}
	}
}
