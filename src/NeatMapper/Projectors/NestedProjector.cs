#if NET5_0_OR_GREATER
#pragma warning disable IDE0079
#pragma warning disable CA1822
#endif

using System;
using System.Collections;
using System.ComponentModel;

namespace NeatMapper {
	/// <summary>
	/// <para>
	/// Projector to be used for nested projections.
	/// </para>
	/// <para>
	/// This class is only used inside expressions where it is expanded into the actual nested maps,
	/// so its methods should not be used outside of expressions.
	/// </para>
	/// </summary>
	public sealed class NestedProjector {
		/// <summary>
		/// Actual underlying projector to use to retrieve maps.
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
			throw new InvalidOperationException("NestedProjector.Project cannot be used outside expressions");
		}

		/// <inheritdoc cref="Project{TDestination}(object?, MappingOptions?)"/>
		public TDestination Project<TDestination>(object? source, IEnumerable? mappingOptions) {
			throw new InvalidOperationException("NestedProjector.Project cannot be used outside expressions");
		}

		/// <inheritdoc cref="Project{TDestination}(object?, MappingOptions?)"/>
		public TDestination Project<TDestination>(object? source, params object?[]? mappingOptions) {
			throw new InvalidOperationException("NestedProjector.Project cannot be used outside expressions");
		}

		/// <inheritdoc cref="Project{TDestination}(object?, MappingOptions?)"/>
		public TDestination Project<TDestination>(object? source) {
			throw new InvalidOperationException("NestedProjector.Project cannot be used outside expressions");
		}

		/// <inheritdoc cref="Project{TDestination}(object?, MappingOptions?)" path="/summary"/>
		/// <typeparam name="TSource">Source type of the projection, used to retrieve the available maps.</typeparam>
		/// <inheritdoc cref="Project{TDestination}(object?, MappingOptions?)" path="/typeparam[@name='TDestination']"/>
		/// <param name="source">Source object.</param>
		/// <inheritdoc cref="Project{TDestination}(object?, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="Project{TDestination}(object?, MappingOptions?)" path="/returns"/>
#if !NETCOREAPP3_1
#pragma warning disable CS1712
#endif
		public TDestination Project<TSource, TDestination>(TSource? source, MappingOptions? mappingOptions) {
#if !NETCOREAPP3_1
#pragma warning restore CS1712
#endif

			throw new InvalidOperationException("NestedProjector.Project cannot be used outside expressions");
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(TSource, MappingOptions?)"/>
		public TDestination Project<TSource, TDestination>(TSource? source, IEnumerable? mappingOptions) {
			throw new InvalidOperationException("NestedProjector.Project cannot be used outside expressions");
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(TSource, MappingOptions?)"/>
		public TDestination Project<TSource, TDestination>(TSource? source, params object?[]? mappingOptions) {
			throw new InvalidOperationException("NestedProjector.Project cannot be used outside expressions");
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(TSource, MappingOptions?)"/>
		public TDestination Project<TSource, TDestination>(TSource? source) {
			throw new InvalidOperationException("NestedProjector.Project cannot be used outside expressions");
		}
	}
}

#if NET5_0_OR_GREATER
#pragma warning restore CA1822
#pragma warning restore IDE0079
#endif