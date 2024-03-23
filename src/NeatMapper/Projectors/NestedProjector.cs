#pragma warning disable CA1822

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


		internal NestedProjector(IProjector projector) {
			Projector = projector ?? throw new ArgumentNullException(nameof(projector));
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
		/// NOTE: Contrary to how <see cref="MapperExtensions.Map{TDestination}(IMapper, object, MappingOptions)"/>
		/// behaves for example, the inferred type will be the type of the variable passed and not the runtime type
		/// (the one obtained by using <see cref="object.GetType()"/>), because expressions are created and replaced
		/// before they are actually run, so we have no way of getting the runtime type.
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
		/// <returns>The projected object</returns>
		public TDestination Project<TDestination>(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions) {

			throw new InvalidOperationException("NestedProjector cannot be used outside expressions");
		}

		/// <inheritdoc cref="Project{TDestination}(object, MappingOptions)"/>
		public TDestination Project<TDestination>(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			throw new InvalidOperationException("NestedProjector cannot be used outside expressions");
		}

		/// <inheritdoc cref="Project{TDestination}(object, MappingOptions)"/>
		public TDestination Project<TDestination>(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			params object[] mappingOptions) {

			throw new InvalidOperationException("NestedProjector cannot be used outside expressions");
		}

		/// <inheritdoc cref="Project{TDestination}(object, MappingOptions)"/>
		public TDestination Project<TDestination>(object source) {
			throw new InvalidOperationException("NestedProjector cannot be used outside expressions");
		}

		/// <inheritdoc cref="Project{TDestination}(object, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource">Source type of the projection, used to retrieve the available maps.</typeparam>
		/// <inheritdoc cref="Project{TDestination}(object, MappingOptions)" path="/typeparam[@name='TDestination']"/>
		/// <param name="source">Source object.</param>
		/// <inheritdoc cref="Project{TDestination}(object, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="Project{TDestination}(object, MappingOptions)" path="/returns"/>
#pragma warning disable CS1712
		public TDestination Project<TSource, TDestination>(
#pragma warning restore CS1712
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions) {

			throw new InvalidOperationException("NestedProjector cannot be used outside expressions");
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(TSource, MappingOptions)"/>
		public TDestination Project<TSource, TDestination>(
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			throw new InvalidOperationException("NestedProjector cannot be used outside expressions");
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(TSource, MappingOptions)"/>
		public TDestination Project<TSource, TDestination>(
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
			params object[] mappingOptions) {

			throw new InvalidOperationException("NestedProjector cannot be used outside expressions");
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(TSource, MappingOptions)"/>
		public TDestination Project<TSource, TDestination>(
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source) {

			throw new InvalidOperationException("NestedProjector cannot be used outside expressions");
		}
	}
}

#pragma warning restore CA1822