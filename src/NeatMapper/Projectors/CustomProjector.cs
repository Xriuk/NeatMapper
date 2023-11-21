using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IProjector"/> which projects objects by using <see cref="IProjectionMap{TSource, TDestination}"/>,
	/// also supports nested maps which get expanded into the final map.
	/// </summary>
	public sealed class CustomProjector : IProjector, IProjectorCanProject {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private class LambdaParameterReplacer : ExpressionVisitor {
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
			protected object RetrieveValueRecursively(Expression expr) {
				// Navigate up recursively until ConstantExpression and retrieve the projector from it down again
				if (expr != null) { 
					object value;
					if (expr is ConstantExpression cons)
						return cons.Value;
					else if (expr is MemberExpression memb) {
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
				if (node.Method.DeclaringType == typeof(NestedProjector) &&
					RetrieveValueRecursively(node.Object) is NestedProjector nestedProjector) {

					var argumentType = node.Arguments[0].Type;

					// Validate mapping options if not null
					MappingOptions mappingOptions;
					if(node.Arguments.Count == 2) {
						var mappingOptionsType = node.Arguments[1].Type;
						if(mappingOptionsType == typeof(MappingOptions))
							mappingOptions = RetrieveValueRecursively(node.Arguments[1]) as MappingOptions;
						else if(mappingOptionsType == typeof(IEnumerable)) {
							var ienumerable = RetrieveValueRecursively(node.Arguments[1]) as IEnumerable;
							if(ienumerable?.Cast<object>().Any() == true)
								mappingOptions = new MappingOptions(ienumerable);
							else
								mappingOptions = null;
						}
						else if(mappingOptionsType == typeof(object) && RetrieveValueRecursively(node.Arguments[1]) == null)
							mappingOptions = null;
						else
							throw new InvalidOperationException($"Invalid mapping options type {mappingOptionsType.FullName ?? mappingOptionsType.Name} in nested map");
					}
					else
						mappingOptions = null;

					// Retrieve the map types, the source may be inferred or explicit
					(Type From, Type To) types;
					var genericArguments = node.Method.GetGenericArguments();
					if (genericArguments.Length == 1) 
						types = (argumentType, genericArguments[0]);
					else 
						types = (genericArguments[0], genericArguments[0]);

					// Retrieve the nested expression from the projector
					var nestedExpression = nestedProjector.Projector.Project(types.From, types.To, mappingOptions);

					// Replace the parameter with the argument
					if (!nestedExpression.Parameters[0].Type.IsAssignableFrom(argumentType)) { 
						throw new InvalidOperationException($"Incompatible types between passed argument {argumentType.FullName ?? argumentType.Name} " +
							$"and retrieved map for types {nestedExpression.Parameters[0].Type.FullName ?? nestedExpression.Parameters[0].Type.Name} -> " +
							$"{nestedExpression.Parameters[1].Type.FullName ?? nestedExpression.Parameters[1].Type.Name}");
					}
					return new LambdaParameterReplacer(node.Arguments[0]).SetupAndVisit(nestedExpression);
				}

				return base.VisitMethodCall(node);
			}
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


		internal readonly CustomMapsConfiguration _configuration;
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Creates a new instance of <see cref="CustomProjector"/>.<br/>
		/// At least one between <paramref name="mapsOptions"/> and <paramref name="additionalMapsOptions"/>
		/// should be specified.
		/// </summary>
		/// <param name="mapsOptions">Options to retrieve user-defined maps for the projector, null to ignore.</param>
		/// <param name="additionalMapsOptions">Additional user-defined maps for the projector, null to ignore.</param>
		/// <param name="serviceProvider">
		/// Service provider to be passed to the maps inside <see cref="ProjectionContext"/>, 
		/// null to pass an empty service provider.<br/>
		/// Can be overridden during map creation with <see cref="ProjectorOverrideMappingOptions"/>.
		/// </param>
		public CustomProjector(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomMapsOptions?
#else
			CustomMapsOptions
#endif
			mapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomProjectionAdditionalMapsOptions?
#else
			CustomProjectionAdditionalMapsOptions
#endif
			additionalMapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) {

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
				(mapsOptions ?? new CustomMapsOptions()).TypesToScan,
				additionalMapsOptions?._maps.Values
			);
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
		}


		public LambdaExpression Project(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			var map = _configuration.GetMap((sourceType, destinationType));

			var overrideOptions = mappingOptions?.GetOptions<ProjectorOverrideMappingOptions>();
			var context = new ProjectionContext(
				overrideOptions?.ServiceProvider ?? _serviceProvider,
				overrideOptions?.Projector ?? this,
				mappingOptions ?? MappingOptions.Empty);

			object result;
			try {
				result = map.Invoke(new object[] { context });
			}
			catch (MapNotFoundException) {
				throw;
			}
			catch(MappingException e) {
				throw new ProjectionException(e.InnerException, (sourceType, destinationType));
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public bool CanProject(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			try {
				_configuration.GetMap((sourceType, destinationType));
				return true;
			}
			catch (MapNotFoundException) {
				return false;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
