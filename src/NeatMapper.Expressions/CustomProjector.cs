using System.Linq.Expressions;
using System.Reflection;

namespace NeatMapper.Expressions {
	public sealed class CustomProjector : IProjector {
		private class LambdaParameterReplacer : ExpressionVisitor {
			private ParameterExpression[] parameters;
			private Expression[] replacements;

			public LambdaParameterReplacer(params Expression[] replacements) {
				this.parameters = Array.Empty<ParameterExpression>();
				this.replacements = replacements;
			}


			override protected Expression VisitParameter(ParameterExpression node) {
				var parameter = Array.IndexOf(parameters, node);
				if (parameter < 0 || parameter >= parameters.Length)
					return base.VisitParameter(node);

				if(replacements[parameter].Type != parameters[parameter].Type)
					return Expression.Convert(replacements[parameter], parameters[parameter].Type);
				else
					return replacements[parameter];
			}

			public Expression SetupAndVisit(LambdaExpression lambda) {
				parameters = lambda.Parameters.ToArray();
				return Visit(lambda.Body);
			}
		}

		private class NestedProjectionExpander : ExpressionVisitor {
			protected object? RetrieveValueRecursively(Expression? expr) {
				if(expr != null) { 
					object? value;
					if (expr.NodeType == ExpressionType.Constant && expr is ConstantExpression cons)
						return cons.Value;
					else if (expr.NodeType == ExpressionType.MemberAccess && expr is MemberExpression memb) {
						value = RetrieveValueRecursively(memb.Expression);
						if(value != null) {
							if(memb.Member is PropertyInfo prop)
								return prop.GetValue(value);
							else if(memb.Member is FieldInfo field)
								return field.GetValue(value);
						}
					}
				}
				
				throw new InvalidOperationException("The provided expression is not a member access or constant expression");
			}

			protected override Expression VisitMethodCall(MethodCallExpression node) {
				// Expand mapper.Project into the corresponding expressions
				// Navigate up recursively until ConstantExpression and retrieve the projector from it down again
				if (node.Method.DeclaringType == typeof(NestedProjector) &&
					RetrieveValueRecursively(node.Object) is NestedProjector nestedProjector) {

					var argumentType = node.Arguments.Single().Type;

					// Retrieve the map types, the source may be inferred or explicit
					(Type From, Type To) types;
					var genericArguments = node.Method.GetGenericArguments();
					if (genericArguments.Length == 1) 
						types = (argumentType, genericArguments[0]);
					else 
						types = (genericArguments[0], genericArguments[0]);

					// Retrieve the nested expression from the projector
					var nestedExpression = nestedProjector.Projector.Project(types.From, types.To);

					// Replace the parameter with the argument
					if (!nestedExpression.Parameters.Single().Type.IsAssignableFrom(argumentType)) { 
						throw new InvalidOperationException($"Incompatible types between passed argument {argumentType.FullName ?? argumentType.Name} " +
							$"and retrieved map for types {nestedExpression.Parameters[0].Type.FullName ?? nestedExpression.Parameters[0].Type.Name} -> " +
							$"{nestedExpression.Parameters[1].Type.FullName ?? nestedExpression.Parameters[1].Type.Name}");
					}
					return new LambdaParameterReplacer(node.Arguments.Single()).SetupAndVisit(nestedExpression);
				}

				return base.VisitMethodCall(node);
			}
		}


		internal readonly CustomMapsConfiguration _configuration;

		public CustomProjector(CustomProjectionsOptions? projectionsOptions = null, CustomAdditionalProjectionMapsOptions? additionalMapsOptions = null) {
			_configuration = new CustomMapsConfiguration(
				(_, i) => {
					if (!i.IsGenericType)
						return false;
					var type = i.GetGenericTypeDefinition();
					return type == typeof(IProjectionMap<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(IProjectionMapStatic<,>)
#endif
					;
				},
				(projectionsOptions ?? new CustomProjectionsOptions()).TypesToScan,
				additionalMapsOptions?._maps.Values
			);
		}


		public LambdaExpression Project(Type sourceType, Type destinationType) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			var map = _configuration.GetMap((sourceType, destinationType));

			object result;
			try {
				result = map.Invoke(new object[] { new ProjectionContext(EmptyServiceProvider.Instance, this) });
			}
			catch (MapNotFoundException) {
				throw;
			}
			catch(MappingException e) {
				throw new ProjectionException(e.InnerException!, (sourceType, destinationType));
			}

			try {
				result = new NestedProjectionExpander().Visit((LambdaExpression)result);
			}
			catch(Exception e) {
				throw new ProjectionException(e, (sourceType, destinationType));
			}

			// Should not happen
			if (result == null)
				throw new InvalidOperationException($"Null expression returned for types {sourceType.FullName ?? sourceType.Name} -> {destinationType.FullName ?? destinationType.Name}");
			else if (!typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(sourceType, destinationType)).IsAssignableFrom(result.GetType()))
				throw new InvalidOperationException($"Object of type {result.GetType().FullName ?? result.GetType().Name} is not assignable to type {typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(sourceType, destinationType)).FullName}");

			return (LambdaExpression)result;
		}
	}
}
