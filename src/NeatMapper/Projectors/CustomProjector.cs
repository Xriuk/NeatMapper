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
		/// Replaces a nested map invocation with the map itself, also inlines custom Expressions.
		/// </summary>
		private class NestedProjectionExpander : ExpressionVisitor {
			protected override Expression VisitMethodCall(MethodCallExpression node) {
				if (node.Method.DeclaringType == typeof(NestedProjector)) {
					// Expand projector.Project into the corresponding expressions
					if (node.Method.Name == nameof(NestedProjector.Project) &&
						CompileAndRunExpression(node.Object!) is NestedProjector nestedProjector) {

						// Validate mapping options if not null
						MappingOptions? mappingOptions;
						if (node.Arguments.Count == 2) {
							var mappingOptionsType = node.Arguments[1].Type;
							if (mappingOptionsType == typeof(MappingOptions))
								mappingOptions = CompileAndRunExpression(node.Arguments[1]) as MappingOptions;
							else if (mappingOptionsType == typeof(IEnumerable)) {
								var ienumerable = CompileAndRunExpression(node.Arguments[1]) as IEnumerable;
								if (ienumerable?.Cast<object>().Any(o => o != null) == true)
									mappingOptions = new MappingOptions(ienumerable);
								else
									mappingOptions = null;
							}
							else if (mappingOptionsType == typeof(object[])) {
								var objects = CompileAndRunExpression(node.Arguments[1]) as object[];
								if (objects?.Any(o => o != null) == true)
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
						return new LambdaParameterReplacer(Visit(node.Arguments[0])).SetupAndVisitBody(nestedExpression);
					}

					// Expand projector.Inline into the corresponding expressions,
					// supports all arguments
					if (node.Method.Name == nameof(NestedProjector.Inline) &&
						CompileAndRunExpression(node.Arguments[0]) is LambdaExpression nestedExpression2 &&
							Visit(nestedExpression2) is LambdaExpression nestedExpression3) {

						return new LambdaParameterReplacer(node.Arguments.Skip(1).Select(Visit).ToArray()!).SetupAndVisitBody(nestedExpression3);
					}

					// Expand projector.Merge and projector.ConstructAndMerge into the corresponding expressions
					if (node.Method.Name == nameof(NestedProjector.Merge) || node.Method.Name == nameof(NestedProjector.ConstructAndMerge)) {
						var genericArguments = node.Method.GetGenericArguments();

						// Retrieve the provided constructor and its arguments (if any),
						// or find a matching constructor
						NewExpression constructor;
						if (node.Method.Name == nameof(NestedProjector.ConstructAndMerge)) {
							if (node.Arguments[0] is not NewExpression expr ||
								Visit(expr) is not NewExpression expr2) {

								throw new InvalidOperationException(
									"Constructor must be a new instance of a class (new TType(...))");
							}
							constructor = expr2;
						}
						else {
							if (genericArguments[0].IsInterface || genericArguments[0].IsAbstract) {
								throw new InvalidOperationException(
									$"TType must be a concrete type, or you can use the method " +
									$"{nameof(NestedProjector.ConstructAndMerge)} to provide " +
									$"a constructor instead");
							}
							constructor = Expression.New(genericArguments[0]);
						}

						// Retrieve all the members from the initializations
						var expressions = ((NewArrayExpression)node.Arguments[node.Method.Name == nameof(NestedProjector.Merge) ? 0 : 1]).Expressions;
						var initializations = expressions
							.Select(Visit)
							.Where(e => e is MemberInitExpression ||
								(e is UnaryExpression u && u.NodeType == ExpressionType.Convert && u.Type.IsInterface && u.Operand is MemberInitExpression))
							.ToList();
						if (initializations.Count != expressions.Count) {
							throw new InvalidOperationException(
								"Initializations must all be class members initializations " +
								"(new TType{ ... } or new TType(...){ ... }) or type-casts " +
								"to interfaces of class members initializations " +
								"((IInterface)new TType{ ... } or (IInterface)new TType(...){ ... })");
						}
						if (initializations.Any(i => !i!.Type.IsAssignableFrom(constructor.Type))) {
							throw new InvalidOperationException(
								"Initializations must not have types derived from the constructor type, " +
								"only base types. Also interfaces should be restricted to their members only " +
								"by casting them explicitly");
						}

						// Initialize members, for interfaces (type casts) we select only the defined properties,
						// and we reassign them because we could have the following scenario:
						// Class A declares property X from interface Y, if we have another class B
						// implementing the same property X from interface Y we cannot use
						// the same member from class A as they are effectively different members
						Expression body = Expression.MemberInit(
							constructor,
							initializations
								.SelectMany(i => {
									if (i is MemberInitExpression m)
										return m.Bindings;
									else if (i is UnaryExpression u && u.Operand is MemberInitExpression m2) {
										var interfaceMap = m2.Type.GetInterfaceMap(u.Type).TargetMethods;
										return m2.Bindings.Where(b => b.Member is not PropertyInfo p || p.GetAccessors(true).Any(a => interfaceMap.Contains(a)))
											.Select<MemberBinding, MemberBinding>(b => {
												var member = constructor.Type.GetMember(b.Member.Name, MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
													.First();
												return b.BindingType switch {
													MemberBindingType.Assignment => Expression.Bind(member, ((MemberAssignment)b).Expression),
													MemberBindingType.MemberBinding => Expression.MemberBind(member, ((MemberMemberBinding)b).Bindings),
													MemberBindingType.ListBinding => Expression.ListBind(member, ((MemberListBinding)b).Initializers),
													_ => throw new InvalidOperationException(),
												};
											});
									}
									else
										throw new InvalidOperationException();
								})
								.GroupBy(i => (i.Member.MetadataToken, i.Member.Module))
								.Select(i => i.Last()));

						// Cast result if needed
						if (body.Type != genericArguments[0])
							body = Expression.Convert(body, genericArguments[0]);

						return body;
					}
				}

				return base.VisitMethodCall(node);


				// DEV: find a better way to unwrap expressions.
				// Maybe there isn't? Because expressions might also be methods returned values
				static object? CompileAndRunExpression(Expression body) {
					try {
						return Expression.Lambda(body).Compile().DynamicInvoke();
					}
					catch (Exception e) {
						throw new InvalidOperationException(
							"Error during expression evaluation, check the inner exception for details. " +
							"Parameters of the projection expression cannot be referenced in nested projections.", e);
					}
				}
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
			if(!CanProjectInternal(sourceType, destinationType, mappingOptions, out var map, out var context) || map == null)
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

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition) {
				map = null!;
				context = null!;

				return _projectConfiguration.HasOpenGenericMap((sourceType, destinationType));
			}
			else if (_projectConfiguration.TryGetContextMap<ProjectionContext>((sourceType, destinationType), out map)) {
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
