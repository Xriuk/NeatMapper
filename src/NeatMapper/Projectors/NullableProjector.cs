using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IProjector"/> which projects <see cref="Nullable{T}"/> and its underlying types
	/// by using another <see cref="IProjector"/>.
	/// <list type="bullet">
	/// <item>
	/// <b>Type -&gt; <see cref="Nullable{T}"/></b><br/>
	/// Projects the value to the underlying type and casts the result to <see cref="Nullable{T}"/>.
	/// </item>
	/// <item>
	/// <b><see cref="Nullable{T}"/> -&gt; Type</b>:
	/// <list type="bullet">
	/// <item>
	/// If <see cref="Nullable{T}.Value"/> is <see langword="null"/> and the Type is a reference type (nullable)
	/// <see langword="null"/> is returned.
	/// </item>
	/// <item>
	/// If <see cref="Nullable{T}.Value"/> is <see langword="null"/> and the Type is not a reference type
	/// <see cref="ProjectionException"/> is thrown.
	/// </item>
	/// <item>
	/// If <see cref="Nullable{T}.Value"/> is not <see langword="null"/> the underlying type is projected.
	/// </item>
	/// </list>
	/// </item>
	/// <item>
	/// <b><see cref="Nullable{T}"/> -&gt; <see cref="Nullable{T}"/></b><br/>
	/// Tries to project the underlying types in this order:
	/// <list type="number">
	/// <item><see cref="Nullable{T}"/> -&gt; Type and casts the result to <see cref="Nullable{T}"/>.</item>
	/// <item>If <see cref="Nullable{T}.Value"/> is <see langword="null"/> returns <see langword="null"/>.</item>
	/// <item>Type -&gt; <see cref="Nullable{T}"/>.</item>
	/// <item>Type1 -&gt; Type2 and casts the result to <see cref="Nullable{T}"/>.</item>
	/// </list>
	/// </item>
	/// </list>
	/// </summary>
	public sealed class NullableProjector : IProjector, IProjectorMaps {
		/// <summary>
		/// <see cref="IProjector"/> which is used to project concrete types of <see cref="Nullable{T}"/>,
		/// will be also provided as a nested projector in <see cref="ProjectorOverrideMappingOptions"/>
		/// (if not already present).
		/// </summary>
		private readonly IProjector _concreteProjector;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;

		/// <summary>
		/// Creates a new instance of <see cref="NullableProjector"/>.
		/// </summary>
		/// <param name="concreteProjector">
		/// <see cref="IProjector"/> to use to project underlying types.<br/>
		/// Can be overridden during mapping with <see cref="ProjectorOverrideMappingOptions"/>.
		/// </param>
		public NullableProjector(IProjector concreteProjector) {
			_concreteProjector = concreteProjector
				?? throw new ArgumentNullException(nameof(concreteProjector));
			var nestedProjectionContext = new NestedProjectionContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<ProjectorOverrideMappingOptions, NestedProjectionContext>(
				m => m?.Projector != null ? m : new ProjectorOverrideMappingOptions(_concreteProjector, m?.ServiceProvider),
				n => n != null ? new NestedProjectionContext(nestedProjectionContext.ParentProjector, n) : nestedProjectionContext, options.Cached));
		}


		public bool CanProject(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanProjectInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
		}

		public LambdaExpression Project(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanProjectInternal(sourceType, destinationType, ref mappingOptions, out var concreteProjector, out var underlyingTypes) || concreteProjector == null)
				throw new MapNotFoundException(types);

			LambdaExpression concreteProjection;
			try { 
				concreteProjection = concreteProjector.Project(underlyingTypes.From, underlyingTypes.To, mappingOptions);
			}
			catch (OperationCanceledException) {
				throw;
			}
			catch (MapNotFoundException) {
				throw new MapNotFoundException(types);
			}
			catch (Exception e) {
				throw new ProjectionException(e, types);
			}

			var source = Expression.Parameter(sourceType, "source");

			// source.Value
			// source
			Expression sourceExpr;
			if (sourceType != underlyingTypes.From)
				sourceExpr = Expression.Property(source, nameof(Nullable<int>.Value));
			else
				sourceExpr = source;

			var body = new LambdaParameterReplacer(sourceExpr).SetupAndVisitBody(concreteProjection);
			if (underlyingTypes.To != destinationType)
				body = Expression.Convert(body, destinationType);

			if (sourceType != underlyingTypes.From) {
				// source == null ? null : ...
				// source == null ? throw new ... : ...
				body = Expression.Condition(
					Expression.Equal(source, Expression.Constant(null, sourceType)),
					(!destinationType.IsValueType || destinationType.IsNullable() ?
						Expression.Constant(null, destinationType) :
						Expression.Throw(
							Expression.New(
								typeof(ProjectionException).GetConstructor([typeof(Exception), typeof((Type, Type))])!,
								Expression.New(
									typeof(InvalidOperationException).GetConstructor([typeof(string)])!,
									Expression.Constant("Cannot map null value because the destination type is not nullable.")),
								Expression.Constant(types)),
							destinationType)),
					body);
			}

			return Expression.Lambda(typeof(Func<,>).MakeGenericType(sourceType, destinationType), body, source);
		}

		public IEnumerable<(Type From, Type To)> GetMaps(MappingOptions? mappingOptions = null) {
			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var concreteProjector = mappingOptions.GetOptions<ProjectorOverrideMappingOptions>()?.Projector
				?? _concreteProjector;

			return concreteProjector.GetMaps(mappingOptions).SelectMany(NullifyType);


			static IEnumerable<(Type From, Type To)> NullifyType((Type From, Type To) type) {
				yield return type;

				var isFromNullable = type.From.IsValueType && !type.From.IsNullable();
				if (isFromNullable)
					yield return (typeof(Nullable<>).MakeGenericType(type.From), type.To);

				var isToNullable = type.To.IsValueType && !type.To.IsNullable();
				if (isToNullable)
					yield return (type.From, typeof(Nullable<>).MakeGenericType(type.To));

				if (isFromNullable && isToNullable)
					yield return (typeof(Nullable<>).MakeGenericType(type.From), typeof(Nullable<>).MakeGenericType(type.To));
			}
		}


		private bool CanProjectInternal(
			Type sourceType,
			Type destinationType,
			[NotNullWhen(true)] ref MappingOptions? mappingOptions,
			out IProjector concreteProjector,
			out (Type From, Type To) underlyingTypes) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition) {
				concreteProjector = null!;
				underlyingTypes = default;

				return sourceType == typeof(Nullable<>) || destinationType == typeof(Nullable<>);
			}
			else {
				// DEV: disallow nested projection if one of the most recent projectors is ourselves or a composite projector containing us?

				mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

				concreteProjector = mappingOptions.GetOptions<ProjectorOverrideMappingOptions>()?.Projector
					?? _concreteProjector;

				Type? destinationNullableUnderlying;
				if (destinationType.IsNullable()) {
					destinationNullableUnderlying = Nullable.GetUnderlyingType(destinationType)!;
					// Type1? -> Type2
					if (concreteProjector.CanProject(sourceType, destinationNullableUnderlying, mappingOptions)) {
						underlyingTypes = (sourceType, destinationNullableUnderlying);
						return true;
					}
				}
				else
					destinationNullableUnderlying = null;

				Type? sourceNullableUnderlying;
				if (sourceType.IsNullable()) {
					sourceNullableUnderlying = Nullable.GetUnderlyingType(sourceType)!;
					// Type1 -> Type2?
					if (concreteProjector.CanProject(sourceNullableUnderlying, destinationType, mappingOptions)) {
						underlyingTypes = (sourceNullableUnderlying, destinationType);
						return true;
					}
				}
				else
					sourceNullableUnderlying = null;


				// Type1 -> Type2
				if (sourceNullableUnderlying != null && destinationNullableUnderlying != null &&
					concreteProjector.CanProject(sourceNullableUnderlying, destinationNullableUnderlying, mappingOptions)) {

					underlyingTypes = (sourceNullableUnderlying, destinationNullableUnderlying);
					return true;
				}
				else {
					underlyingTypes = default;
					return false;
				}
			}
		}
	}
}
