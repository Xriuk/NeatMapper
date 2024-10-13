using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NeatMapper.Transitive {
	public static class QueryableExtensions {
		private static readonly ConditionalWeakTable<IProjector, GraphCreator> _graphsCache =
			new ConditionalWeakTable<IProjector, GraphCreator>();


		#region ProjectTransitive
		#region Runtime destination, inferred source
		/// <summary>
		/// Projects a queryable into another one transitively, that is by mapping the elements with multiple
		/// <see cref="Queryable.Select{TSource, TResult}(IQueryable{TSource}, System.Linq.Expressions.Expression{Func{TSource, TResult}})"/>.
		/// The source type is <see cref="IQueryable.ElementType"/>.
		/// </summary>
		/// <param name="projector">Projector to use.</param>
		/// <param name="destinationElementType">
		/// Type of the destination element, used to retrieve the available maps.
		/// </param>
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// The projected queryable with <see cref="IQueryable.ElementType"/> equal to <paramref name="destinationElementType"/>.
		/// The actual elements may be null.
		/// </returns>
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IQueryable ProjectTransitive(this IQueryable queryable,
			IProjector projector,
			Type destinationElementType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return queryable.ProjectTransitive(projector, queryable.ElementType, destinationElementType, mappingOptions);
		}

		/// <inheritdoc cref="ProjectTransitive(IQueryable, IProjector, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IQueryable ProjectTransitive(this IQueryable queryable,
			IProjector projector,
			Type destinationElementType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return queryable.ProjectTransitive(projector, queryable.ElementType, destinationElementType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="ProjectTransitive(IQueryable, IProjector, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IQueryable ProjectTransitive(this IQueryable queryable,
			IProjector projector, Type destinationElementType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return queryable.ProjectTransitive(projector, queryable.ElementType, destinationElementType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Runtime source and destination
		/// <summary>
		/// Projects a queryable into another one transitively, that is by mapping the elements with multiple
		/// <see cref="Queryable.Select{TSource, TResult}(IQueryable{TSource}, System.Linq.Expressions.Expression{Func{TSource, TResult}})"/>.
		/// </summary>
		/// <inheritdoc cref="ProjectTransitive(IQueryable, IProjector, Type, MappingOptions)"/>
		/// <param name="sourceElementType">Type of the source element, used to retrieve the available maps.</param>
		public static IQueryable ProjectTransitive(this IQueryable queryable,
			IProjector projector,
			Type sourceElementType,
			Type destinationElementType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (queryable == null)
				throw new ArgumentNullException(nameof(queryable));
			if (projector == null)
				throw new ArgumentNullException(nameof(projector));
			if (sourceElementType == null)
				throw new ArgumentNullException(nameof(sourceElementType));
			if (destinationElementType == null)
				throw new ArgumentNullException(nameof(destinationElementType));

			// We cannot project identical types
			if (sourceElementType == destinationElementType)
				throw new MapNotFoundException((sourceElementType, destinationElementType));

			GraphCreator graphCreator;
			lock (_graphsCache) { 
				if(!_graphsCache.TryGetValue(projector, out graphCreator)) {
					graphCreator = new GraphCreator(options =>
						(options.GetOptions<ProjectorOverrideMappingOptions>()?.Projector
							?? projector).GetMaps(options).Distinct(), null);
					try {
						_graphsCache.Add(projector, graphCreator);
					}
					catch (Exception e){
						throw new InvalidOperationException("Could not create graph cache for the provided projector, see inner exception for details.", e);
					}
				}
			}

			var typesPath = graphCreator.GetOrCreateTypesPath(sourceElementType, destinationElementType, mappingOptions)
				?? throw new MapNotFoundException((sourceElementType, destinationElementType));

			projector = mappingOptions.GetOptions<ProjectorOverrideMappingOptions>()?.Projector
				?? projector;

			return typesPath.Zip(typesPath.Skip(1), (t1, t2) => (From: t1, To: t2))
				.Aggregate(queryable, (result, types) => result.Project(projector, types.From, types.To, mappingOptions));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="ProjectTransitive(IQueryable, IProjector, Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IQueryable ProjectTransitive(this IQueryable queryable,
			IProjector projector,
			Type sourceElementType,
			Type destinationElementType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return queryable.ProjectTransitive(projector, sourceElementType, destinationElementType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="ProjectTransitive(IQueryable, IProjector, Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IQueryable ProjectTransitive(this IQueryable queryable,
			IProjector projector, Type sourceElementType, Type destinationElementType, params object[] mappingOptions) {

			return queryable.ProjectTransitive(projector, sourceElementType, destinationElementType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit destination, inferred source
		/// <inheritdoc cref="ProjectTransitive(IQueryable, IProjector, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TDestination"><inheritdoc cref="ProjectTransitive(IQueryable, IProjector, Type, Type, MappingOptions)" path="/param[@name='destinationElementType']"/></typeparam>
		/// <inheritdoc cref="ProjectTransitive(IQueryable, IProjector, Type, MappingOptions)" path="/param[@name='projector']"/>
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>The projected queryable, the actual elements may be null.</returns>
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IQueryable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> ProjectTransitive<TDestination>(this IQueryable queryable,
			IProjector projector,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return (IQueryable<TDestination>)queryable.ProjectTransitive(projector, queryable.ElementType, typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="ProjectTransitive{TDestination}(IQueryable, IProjector, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IQueryable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> ProjectTransitive<TDestination>(this IQueryable queryable,
			IProjector projector,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return queryable.ProjectTransitive<TDestination>(projector, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="ProjectTransitive{TDestination}(IQueryable, IProjector, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IQueryable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> ProjectTransitive<TDestination>(this IQueryable queryable, IProjector projector, params object[] mappingOptions) {

			return queryable.ProjectTransitive<TDestination>(projector, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="ProjectTransitive(IQueryable, IProjector, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="ProjectTransitive(IQueryable, IProjector, Type, Type, MappingOptions)" path="/param[@name='sourceElementType']"/></typeparam>
		/// <inheritdoc cref="ProjectTransitive{TDestination}(IQueryable, IProjector, MappingOptions)" path="/typeparam[@name='TDestination']"/>
		/// <inheritdoc cref="ProjectTransitive(IQueryable, IProjector, Type, MappingOptions)" path="/param[@name='projector']"/>
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="ProjectTransitive{TDestination}(IQueryable, IProjector, MappingOptions)" path="/returns"/>
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IQueryable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
#pragma warning disable CS1712
			> ProjectTransitive<TSource, TDestination>(this IQueryable<
#pragma warning restore CS1712
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			> queryable,
			IProjector projector,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return (IQueryable<TDestination>)queryable.ProjectTransitive(projector, typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="ProjectTransitive{TSource, TDestination}(IQueryable{TSource}, IProjector, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IQueryable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> ProjectTransitive<TSource, TDestination>(this IQueryable<
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			> queryable,
			IProjector projector,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return (IQueryable<TDestination>)queryable.ProjectTransitive(projector, typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="ProjectTransitive{TSource, TDestination}(IQueryable{TSource}, IProjector, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IQueryable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> ProjectTransitive<TSource, TDestination>(this IQueryable<
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			> queryable,
			IProjector projector, params object[] mappingOptions) {

			return (IQueryable<TDestination>)queryable.ProjectTransitive(projector, typeof(TSource), typeof(TDestination), mappingOptions);
		}
		#endregion
		#endregion
	}
}
