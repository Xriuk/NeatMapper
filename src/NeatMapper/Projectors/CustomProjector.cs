using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IProjector"/> which projects objects by using <see cref="IProjectionMap{TSource, TDestination}"/>,
	/// also supports nested maps which get expanded into the final map.
	/// </summary>
	public sealed class CustomProjector : IProjector, IProjectorMaps {
		/// <summary>
		/// Replaces a nested map invocation with the map itself.
		/// </summary>
		private class NestedProjectionExpander : ExpressionVisitor {
			private static object? RetrieveValueRecursively(Expression expr) {
				// Navigate up recursively until ConstantExpression and retrieve the projector from it down again
				if (expr != null) { 
					object? value;
					if (expr is ConstantExpression cons)
						return cons.Value;
					else if (expr is MemberExpression memb) {
						value = memb.Expression != null ? RetrieveValueRecursively(memb.Expression) : null;
						if(memb.Member is PropertyInfo prop) {
							if(value != null || prop.GetGetMethod()?.IsStatic == true)
								return prop.GetValue(value);
						}
						else if(memb.Member is FieldInfo field && (value != null || field.IsStatic))
							return field.GetValue(value);
					}
				}
				
				throw new InvalidOperationException("The provided expression is not a member access or constant expression");
			}

			// DEV: find a better way to unwrap expressions
			private static object? CompileAndRunExpression(Expression body) {
				try { 
					return Expression.Lambda(body).Compile().DynamicInvoke();
				}
				catch(Exception e){
					throw new InvalidOperationException(
						"Error during expression evaluation, check the inner exception for details. " +
						"Parameters of the projection expression cannot be referenced in nested projections.", e);
				}
			}


			protected override Expression VisitMethodCall(MethodCallExpression node) {
				// Expand projector.Project into the corresponding expressions
				if (node.Method.DeclaringType == typeof(NestedProjector) &&
					node.Method.Name == nameof(NestedProjector.Project) &&
					RetrieveValueRecursively(node.Object!) is NestedProjector nestedProjector) {

					// Validate mapping options if not null
					MappingOptions? mappingOptions;
					if(node.Arguments.Count == 2) {
						var mappingOptionsType = node.Arguments[1].Type;
						if(mappingOptionsType == typeof(MappingOptions))
							mappingOptions = CompileAndRunExpression(node.Arguments[1]) as MappingOptions;
						else if(mappingOptionsType == typeof(IEnumerable)) {
							var ienumerable = CompileAndRunExpression(node.Arguments[1]) as IEnumerable;
							if(ienumerable?.Cast<object>().Any(o => o != null) == true)
								mappingOptions = new MappingOptions(ienumerable);
							else
								mappingOptions = null;
						}
						else if(mappingOptionsType == typeof(object[])) {
							var objects = CompileAndRunExpression(node.Arguments[1]) as object[];
							if(objects?.Any(o => o != null) == true)
								mappingOptions = new MappingOptions(objects);
							else
								mappingOptions = null;
						}
						else
							throw new InvalidOperationException($"Invalid mapping options type {mappingOptionsType.FullName ?? mappingOptionsType.Name} in nested map");
					}
					else
						mappingOptions = null;

					var argumentType = node.Arguments[0].Type;

					// Retrieve the map types, the source may be inferred or explicit
					(Type From, Type To) types;
					var genericArguments = node.Method.GetGenericArguments();
					if (genericArguments.Length == 1) 
						types = (argumentType, genericArguments[0]);
					else 
						types = (genericArguments[0], genericArguments[1]);

					// Retrieve the nested expression from the projector
					var nestedExpression = nestedProjector.Projector.Project(types.From, types.To, mappingOptions);

					// Replace the parameter with the argument
					if (!nestedExpression.Parameters[0].Type.IsAssignableFrom(argumentType)) { 
						throw new InvalidOperationException($"Incompatible types between passed argument {argumentType.FullName ?? argumentType.Name} " +
							$"and retrieved map for types {nestedExpression.Parameters[0].Type.FullName ?? nestedExpression.Parameters[0].Type.Name} -> " +
							$"{nestedExpression.Parameters[1].Type.FullName ?? nestedExpression.Parameters[1].Type.Name}");
					}
					return new LambdaParameterReplacer(node.Arguments[0]).SetupAndVisitBody(nestedExpression);
				}

				// Expand projector.Inline into the corresponding expressions
				if (node.Method.DeclaringType == typeof(NestedProjector) &&
					node.Method.Name == nameof(NestedProjector.Inline) &&
					CompileAndRunExpression(node.Arguments[0]) is LambdaExpression nestedExpression2) {

					return new LambdaParameterReplacer(node.Arguments[1]).SetupAndVisitBody(nestedExpression2);
				}

				return base.VisitMethodCall(node);
			}
		}

		/// <summary>
		/// Configuration for class and additional maps for the projector.
		/// </summary>
		internal readonly CustomMapsConfiguration _canProjectConfiguration;

		/// <summary>
		/// Configuration for class and additional maps for the projector.
		/// </summary>
		internal readonly CustomMapsConfiguration _projectConfiguration;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> and output <see cref="ProjectionContext"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<ProjectionContext> _contextsCache;


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
			CustomMapsOptions? mapsOptions = null,
			CustomProjectionAdditionalMapsOptions? additionalMapsOptions = null,
			IServiceProvider? serviceProvider = null) {

			var typesToScan = (mapsOptions ?? new CustomMapsOptions()).TypesToScan;
			_canProjectConfiguration = new CustomMapsConfiguration(
				(_, i) => {
					if (!i.IsGenericType)
						return false;
					var type = i.GetGenericTypeDefinition();
					return type == typeof(ICanProject<,>)
#if NET7_0_OR_GREATER
						|| type == typeof(ICanProjectStatic<,>)
#endif
					;
				},
				typesToScan,
				additionalMapsOptions?._canMaps.Values);
			_projectConfiguration = new CustomMapsConfiguration(
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
				typesToScan,
				additionalMapsOptions?._maps.Values);
			serviceProvider ??= EmptyServiceProvider.Instance;
			_contextsCache = new MappingOptionsFactoryCache<ProjectionContext>(options => {
				var overrideOptions = options.GetOptions<ProjectorOverrideMappingOptions>();
				return new ProjectionContext(
					overrideOptions?.ServiceProvider ?? serviceProvider,
					overrideOptions?.Projector ?? this,
					options
				);
			});
		}


		public bool CanProject(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanProjectInternal(sourceType, destinationType, mappingOptions, out _, out _);
		}

		public LambdaExpression Project(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if(!CanProjectInternal(sourceType, destinationType, mappingOptions, out var map, out var context))
				throw new MapNotFoundException((sourceType, destinationType));

			LambdaExpression result;
			try {
				result = map.Invoke(context) as LambdaExpression
					?? throw new InvalidOperationException($"Null expression returned for types {sourceType.FullName ?? sourceType.Name} -> {destinationType.FullName ?? destinationType.Name}");
			}
			catch(MappingException e) {
				throw new ProjectionException(e.InnerException!, (sourceType, destinationType));
			}

			Expression expr;
			try {
				expr = new NestedProjectionExpander().Visit(result);
			}
			catch(Exception e) {
				throw new ProjectionException(e, (sourceType, destinationType));
			}

			// Should not happen
			if (expr == null)
				throw new InvalidOperationException($"Null expression returned for types {sourceType.FullName ?? sourceType.Name} -> {destinationType.FullName ?? destinationType.Name}");
			else
				TypeUtils.CheckObjectType(expr, typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(sourceType, destinationType)));

			return (LambdaExpression)expr;
		}

		public IEnumerable<(Type From, Type To)> GetMaps(MappingOptions?mappingOptions = null) {
			return _projectConfiguration.GetMaps();
		}


		private bool CanProjectInternal(
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions,
			out Func<ProjectionContext, object?> map,
			out ProjectionContext context) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (_projectConfiguration.TryGetContextMap<ProjectionContext>((sourceType, destinationType), out map)) {
				context = _contextsCache.GetOrCreate(mappingOptions);

				if (_canProjectConfiguration.TryGetContextMap<ProjectionContext>((sourceType, destinationType), out var canProject))
					return (bool)canProject.Invoke(context)!;
				else
					return true;
			}
			else {
				context = null!;
				return false;
			}
		}
	}
}
