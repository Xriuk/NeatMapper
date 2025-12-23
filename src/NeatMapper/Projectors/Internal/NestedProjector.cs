#if NET5_0_OR_GREATER
#pragma warning disable IDE0079
#pragma warning disable CA1822
#endif

using System;
using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;

namespace NeatMapper {
	/// <summary>
	/// <para>
	/// Projector to be used for nested projections.
	/// </para>
	/// <para>
	/// This class is only used inside expressions where it is expanded into the actual nested maps,
	/// so its methods should not be used outside of expressions (except CanProject).
	/// </para>
	/// </summary>
	public sealed class NestedProjector {
		/// <summary>
		/// Actual underlying projector to use to retrieve the maps.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IProjector Projector { get; }


		/// <summary>
		/// Creates a new instance of <see cref="NestedProjector"/>.
		/// </summary>
		/// <param name="projector">Underlying projector to forward the actual projections to.</param>
		internal NestedProjector(IProjector projector) {
			Projector = projector ?? throw new ArgumentNullException(nameof(projector));
		}


		#region CanProject
		/// <inheritdoc cref="ProjectorExtensions.CanProject{TSource, TDestination}(IProjector, MappingOptions?)"/>
		public bool CanProject<TSource, TDestination>(MappingOptions? mappingOptions) {
			return Projector.CanProject<TSource, TDestination>(mappingOptions);
		}

		/// <inheritdoc cref="ProjectorExtensions.CanProject{TSource, TDestination}(IProjector, MappingOptions?)"/>
		public bool CanProject<TSource, TDestination>(IEnumerable? mappingOptions) {
			return Projector.CanProject<TSource, TDestination>(mappingOptions);
		}

		/// <inheritdoc cref="ProjectorExtensions.CanProject{TSource, TDestination}(IProjector, MappingOptions?)"/>
		public bool CanProject<TSource, TDestination>(params object?[]? mappingOptions) {
			return Projector.CanProject<TSource, TDestination>(mappingOptions);
		}
		#endregion

		#region Project
		#region Explicit destination, inferred source
		/// <summary>
		/// Projects an object by injecting it into a projection map.
		/// </summary>
		/// <typeparam name="TDestination">Destination type of the projection, used to retrieve the available maps.</typeparam>
		/// <param name="source">
		/// <para>
		/// Source object, the source type will be retrieved from it and will be used to retrieve the available maps.
		/// </para>
		/// <para>
		/// NOTE: Contrary to how <see cref="MapperExtensions.Map{TDestination}(IMapper, object, MappingOptions?)"/>
		/// behaves for example, the inferred type will be the type of the variable passed and not the runtime type
		/// (the one obtained by using <see cref="object.GetType()"/>), because expressions are created and replaced
		/// before they are actually run, so we have no way of getting the runtime type.
		/// This allows the passed variable to be null as long as it is typed.
		/// </para>
		/// <code>
		/// LimitedProduct limitedProd = new LimitedProduct();
		/// Product prod = limitedProd;
		/// // Here the inferred type for the source is Product, despite prod being actually a LimitedProduct
		/// context.Projector.Project&lt;ProductDto&gt;(prod);
		/// </code>
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the projector and/or the maps, null to ignore.<br/>
		/// The passed options should NOT come from the <paramref name="source"/> object as they are replaced before the final map is run,
		/// you can access members from the context of the nested map or externally.
		/// </param>
		/// <returns>The projected object.</returns>
		public TDestination Project<TDestination>(object? source, MappingOptions? mappingOptions) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Project)} cannot be used outside of expressions");
		}

		/// <inheritdoc cref="Project{TDestination}(object?, MappingOptions?)"/>
		public TDestination Project<TDestination>(object? source, IEnumerable? mappingOptions) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Project)} cannot be used outside of expressions");
		}

		/// <inheritdoc cref="Project{TDestination}(object?, MappingOptions?)"/>
		public TDestination Project<TDestination>(object? source, params object?[]? mappingOptions) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Project)} cannot be used outside of expressions");
		}

		/// <inheritdoc cref="Project{TDestination}(object?, MappingOptions?)"/>
		public TDestination Project<TDestination>(object? source) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Project)} cannot be used outside of expressions");
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="Project{TDestination}(object?, MappingOptions?)" path="/summary"/>
		/// <typeparam name="TSource">Source type of the projection, used to retrieve the available maps.</typeparam>
		/// <inheritdoc cref="Project{TDestination}(object?, MappingOptions?)" path="/typeparam[@name='TDestination']"/>
		/// <param name="source">Source object.</param>
		/// <inheritdoc cref="Project{TDestination}(object?, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="Project{TDestination}(object?, MappingOptions?)" path="/returns"/>
#if !NETCOREAPP3_1
#pragma warning disable CS1712
#endif
		public TDestination Project<TSource, TDestination>(TSource source, MappingOptions? mappingOptions) {
#if !NETCOREAPP3_1
#pragma warning restore CS1712
#endif

			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Project)} cannot be used outside of expressions");
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(TSource, MappingOptions?)"/>
		public TDestination Project<TSource, TDestination>(TSource source, IEnumerable? mappingOptions) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Project)} cannot be used outside of expressions");
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(TSource, MappingOptions?)"/>
		public TDestination Project<TSource, TDestination>(TSource source, params object?[]? mappingOptions) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Project)} cannot be used outside of expressions");
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(TSource, MappingOptions?)"/>
		public TDestination Project<TSource, TDestination>(TSource source) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Project)} cannot be used outside of expressions");
		}
		#endregion
		#endregion

		#region Inline
#pragma warning disable CS0162
		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<TResult>(Expression<Func<TResult>> expression) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(); // Not compiled, used as a type safety check
		}

		/// <summary>
		/// Projects objects by injecting them into an inline expression. This allows incorporating
		/// external expressions into projection maps.
		/// </summary>
		/// <typeparam name="TSource">The type of the first parameter.</typeparam>
		/// <typeparam name="TDestination">The type of the second parameter.</typeparam>
		/// <param name="expression">
		/// Expression to inline in the projection, its arguments will be replaced with
		/// the provided parameters.
		/// </param>
		/// <param name="source">First parameter used to replace the corresponding argument in the expression.</param>
		/// <returns>The projected object.</returns>
		public TDestination Inline<TSource, TDestination>(Expression<Func<TSource, TDestination>> expression, TSource source) { // DEV: refactor to Func<T, TResult>
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(source); // Not compiled, used as a type safety check
		}
		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> expression, T1 arg1, T2 arg2) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2); // Not compiled, used as a type safety check
		}
		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<T1, T2, T3, TResult>(Expression<Func<T1, T2, T3, TResult>> expression, T1 arg1, T2 arg2, T3 arg3) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2, arg3); // Not compiled, used as a type safety check
		}
		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<T1, T2, T3, T4, TResult>(Expression<Func<T1, T2, T3, T4, TResult>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2, arg3, arg4); // Not compiled, used as a type safety check
		}

		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<T1, T2, T3, T4, T5, TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2, arg3, arg4, arg5); // Not compiled, used as a type safety check
		}
		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<T1, T2, T3, T4, T5, T6, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6); // Not compiled, used as a type safety check
		}
		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<T1, T2, T3, T4, T5, T6, T7, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7); // Not compiled, used as a type safety check
		}
		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8); // Not compiled, used as a type safety check
		}

		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9); // Not compiled, used as a type safety check
		}
		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10); // Not compiled, used as a type safety check
		}
		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11); // Not compiled, used as a type safety check
		}
		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12); // Not compiled, used as a type safety check
		}

		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13); // Not compiled, used as a type safety check
		}
		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14); // Not compiled, used as a type safety check
		}
		/// <inheritdoc cref="Inline{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}(Expression{Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}}, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)"/>
		public TResult Inline<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15); // Not compiled, used as a type safety check
		}
		/// <summary>
		/// Projects objects by injecting them into an inline expression. This allows incorporating
		/// external expressions into projection maps.
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter.</typeparam>
		/// <typeparam name="T2">The type of the second parameter.</typeparam>
		/// <typeparam name="T3">The type of the third parameter.</typeparam>
		/// <typeparam name="T4">The type of the fourth parameter.</typeparam>
		/// <typeparam name="T5">The type of the fifth parameter.</typeparam>
		/// <typeparam name="T6">The type of the sixth parameter.</typeparam>
		/// <typeparam name="T7">The type of the seventh parameter.</typeparam>
		/// <typeparam name="T8">The type of the eighth parameter.</typeparam>
		/// <typeparam name="T9">The type of the ninth parameter.</typeparam>
		/// <typeparam name="T10">The type of the tenth parameter.</typeparam>
		/// <typeparam name="T11">The type of the eleventh parameter.</typeparam>
		/// <typeparam name="T12">The type of the twelfth parameter.</typeparam>
		/// <typeparam name="T13">The type of the thirteenth parameter.</typeparam>
		/// <typeparam name="T14">The type of the fourteenth parameter.</typeparam>
		/// <typeparam name="T15">The type of the fifteenth parameter.</typeparam>
		/// <typeparam name="T16">The type of the sixteenth parameter.</typeparam>
		/// <typeparam name="TResult">The type of the return value.</typeparam>
		/// <param name="expression">
		/// Expression to inline in the projection, its arguments will be replaced with
		/// the provided parameters.
		/// </param>
		/// <param name="arg1">
		/// First parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg2">
		/// Second parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg3">
		/// Third parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg4">
		/// Fourth parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg5">
		/// Fifth parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg6">
		/// Sixth parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg7">
		/// Seventh parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg8">
		/// Eighth parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg9">
		/// Ninth parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg10">
		/// Tenth parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg11">
		/// Eleventh parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg12">
		/// Twelfth parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg13">
		/// Thirteenth parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg14">
		/// Fourteenth parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg15">
		/// Fifteenth parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <param name="arg16">
		/// Sixteenth parameter used to replace the corresponding argument in the expression.
		/// </param>
		/// <returns>The projected object.</returns>
		public TResult Inline<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16) {
			throw new InvalidOperationException($"{nameof(NestedProjector)}.{nameof(Inline)} cannot be used outside of expressions");
			expression.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16); // Not compiled, used as a type safety check
		}
#pragma warning restore CS0162
		#endregion
	}
}

#if NET5_0_OR_GREATER
#pragma warning restore CA1822
#pragma warning restore IDE0079
#endif