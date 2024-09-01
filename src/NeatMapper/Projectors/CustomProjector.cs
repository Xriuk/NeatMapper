﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IProjector"/> which projects objects by using <see cref="IProjectionMap{TSource, TDestination}"/>,
	/// also supports nested maps which get expanded into the final map.
	/// </summary>
	public sealed class CustomProjector : IProjector, IProjectorCanProject, IProjectorMaps {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		/// <summary>
		/// Replaces a nested map invocation with the map itself.
		/// </summary>
		private class NestedProjectionExpander : ExpressionVisitor {
			private static object RetrieveValueRecursively(Expression expr) {
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

			// DEV: find a better way to unwrap expressions
			private static object CompileAndRunExpression(Expression body) {
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
				// Expand mapper.Project into the corresponding expressions
				if (node.Method.DeclaringType == typeof(NestedProjector) &&
					RetrieveValueRecursively(node.Object) is NestedProjector nestedProjector) {

					var argumentType = node.Arguments[0].Type;

					// Validate mapping options if not null
					MappingOptions mappingOptions;
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
					return new LambdaParameterReplacer(node.Arguments[0]).SetupAndVisitBody(nestedExpression);
				}

				return base.VisitMethodCall(node);
			}
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


		/// <summary>
		/// Configuration for class and additional maps for the projector.
		/// </summary>
		internal readonly CustomMapsConfiguration _configuration;

		/// <summary>
		/// Service provider available in the created <see cref="ProjectionContext"/>s.
		/// </summary>
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output <see cref="ProjectionContext"/>.
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, ProjectionContext> _contextsCache
			= new ConcurrentDictionary<MappingOptions, ProjectionContext>();

		/// <summary>
		/// Cached output <see cref="ProjectionContext"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have null keys) and <see cref="MappingOptions.Empty"/>,
		/// also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		private readonly ProjectionContext _contextsCacheNull;


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
			_contextsCacheNull = CreateProjectionContext(MappingOptions.Empty);
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

			var map = _configuration.GetContextMap<ProjectionContext>((sourceType, destinationType));

			ProjectionContext context;
			if (mappingOptions == null)
				context = _contextsCacheNull;
			else if(mappingOptions.Cached)
				context = _contextsCache.GetOrAdd(mappingOptions, CreateProjectionContext);
			else
				context = CreateProjectionContext(mappingOptions);

			object result;
			try {
				result = map.Invoke(context);
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
			else
				TypeUtils.CheckObjectType(result, typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(sourceType, destinationType)));

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
				_configuration.GetContextMap<ProjectionContext>((sourceType, destinationType));
				return true;
			}
			catch (MapNotFoundException) {
				return false;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public IEnumerable<(Type From, Type To)> GetMaps(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return _configuration.GetMaps();
		}


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private ProjectionContext CreateProjectionContext(MappingOptions options) {
			var overrideOptions = options.GetOptions<ProjectorOverrideMappingOptions>();
			return new ProjectionContext(
				overrideOptions?.ServiceProvider ?? _serviceProvider,
				overrideOptions?.Projector ?? this,
				options
			);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
