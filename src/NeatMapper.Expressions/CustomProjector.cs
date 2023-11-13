using System.Linq.Expressions;

namespace NeatMapper.Expressions {
	internal class CustomProjector : IProjector {
		protected class ProjectionVisitor : ExpressionVisitor {
			protected override Expression VisitMethodCall(MethodCallExpression node) {
				// Expand mapper.Project (methods and extensions) into the corresponding expressions
				if(node.Object != null) {
					// Check if the method is the implementation of the interface
					var objectType = node.Object.GetType();
					var projectorInterface = objectType.GetInterfaces().FirstOrDefault(i =>
						i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IProjectionMap<,>)
#if NET7_0_OR_GREATER
							//|| type == typeof(INewMapStatic<,>)
#endif
					));
					
					if(projectorInterface != null && node.Method == objectType.GetInterfaceMap(projectorInterface).TargetMethods.Single()) {
						// DEV: type parameters should be known statically (cannot contain reference to parameters or external variables)
					}
				}
				else if (node.Method.DeclaringType == typeof(ProjectorExtensions)) {
					// DEV: type parameters should be known statically (cannot contain reference to parameters or external variables)
				}

				return base.VisitMethodCall(node);
			}
		}


		internal readonly CustomMapsConfiguration _configuration;

		public CustomProjector(CustomProjectionsOptions? projectionsOptions = null) {
			_configuration = new CustomMapsConfiguration(
				(_, i) => {
					if (!i.IsGenericType)
						return false;
					var type = i.GetGenericTypeDefinition();
					return type == typeof(IProjectionMap<,>)
#if NET7_0_OR_GREATER
						//|| type == typeof(INewMapStatic<,>)
#endif
					;
				},
				(projectionsOptions ?? new CustomProjectionsOptions()).TypesToScan,
				null
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
			
			// Should not happen
			if (result == null)
				throw new InvalidOperationException($"Null expression returned for types {sourceType.FullName ?? sourceType.Name} -> {destinationType.FullName ?? destinationType.Name}");
			else if (!destinationType.IsAssignableFrom(result.GetType()))
				throw new InvalidOperationException($"Object of type {result.GetType().FullName ?? result.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}");

			return (LambdaExpression)new ProjectionVisitor().Visit((LambdaExpression)result);
		}
	}
}
