using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NeatMapper {
	public static class QueryableExtensions {
		/// <summary>
		/// <see cref="Queryable.Select{TSource, TResult}(IQueryable{TSource}, Expression{Func{TSource, TResult}})"/>
		/// </summary>
		private static readonly MethodInfo Queryable_Select = typeof(Queryable).GetMethods().Single(m => {
			if (m.Name != nameof(Queryable.Select))
				return false;
			var parameters = m.GetParameters();
			if (parameters.Length != 2 || !parameters[1].ParameterType.IsGenericType)
				return false;
			var genericArguments = parameters[1].ParameterType.GetGenericArguments();
			return genericArguments.Length == 1 && genericArguments[0].IsGenericType && genericArguments[0].GetGenericTypeDefinition() == typeof(Func<,>);
		});


		#region Project
		#region Runtime destination
		/// <summary>
		/// Projects a queryable into another one. The source type is <see cref="IQueryable.ElementType"/>.
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
		public static IQueryable Project(this IQueryable queryable,
			IProjector projector,
			Type destinationElementType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return queryable.Project(projector, queryable.ElementType, destinationElementType, mappingOptions);
		}

		/// <inheritdoc cref="Project(IQueryable, IProjector, Type, MappingOptions)"/>
		public static IQueryable Project(this IQueryable queryable,
			IProjector projector,
			Type destinationElementType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return queryable.Project(projector, destinationElementType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Project(IQueryable, IProjector, Type, MappingOptions)"/>
		public static IQueryable Project(this IQueryable queryable,
			IProjector projector, Type destinationElementType, params object[] mappingOptions) {

			return queryable.Project(projector, destinationElementType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Runtime source and destination
		/// <summary>
		/// Projects a queryable into another one.
		/// </summary>
		/// <inheritdoc cref="Project(IQueryable, IProjector, Type, MappingOptions)"/>
		/// <param name="sourceElementType">Type of the source element, used to retrieve the available maps.</param>
		public static IQueryable Project(this IQueryable queryable,
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
			if (!sourceElementType.IsAssignableFrom(queryable.ElementType)) { 
				throw new ArgumentException($"Source type {sourceElementType.FullName ?? sourceElementType.Name} is not assignable " +
					$"from queryable type {queryable.ElementType.FullName ?? queryable.ElementType.Name}.");
			}

			return (IQueryable)Queryable_Select.MakeGenericMethod(sourceElementType, destinationElementType)
				.Invoke(null, new object[] {
					queryable,
					projector.Project(sourceElementType, destinationElementType, mappingOptions) });

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Project(IQueryable, IProjector, Type, Type, MappingOptions)"/>
		public static IQueryable Project(this IQueryable queryable,
			IProjector projector,
			Type sourceElementType,
			Type destinationElementType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return queryable.Project(projector, sourceElementType, destinationElementType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Project(IQueryable, IProjector, Type, Type, MappingOptions)"/>
		public static IQueryable Project(this IQueryable queryable,
			IProjector projector, Type sourceElementType, Type destinationElementType, params object[] mappingOptions) {

			return queryable.Project(projector, sourceElementType, destinationElementType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit destination, inferred source
		/// <inheritdoc cref="Project(IQueryable, IProjector, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TDestination"><inheritdoc cref="Project(IQueryable, IProjector, Type, Type, MappingOptions)" path="/param[@name='destinationElementType']"/></typeparam>
		/// <inheritdoc cref="Project(IQueryable, IProjector, Type, MappingOptions)" path="/param[@name='projector']"/>
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>The projected queryable, the actual elements may be null.</returns>
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/exception"/>
		public static IQueryable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> Project<TDestination>(this IQueryable queryable,
			IProjector projector,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return (IQueryable<TDestination>)queryable.Project(projector, queryable.ElementType, typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="Project{TDestination}(IQueryable, IProjector, MappingOptions)"/>
		public static IQueryable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> Project<TDestination>(this IQueryable queryable,
			IProjector projector,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return queryable.Project<TDestination>(projector, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Project{TDestination}(IQueryable, IProjector, MappingOptions)"/>
		public static IQueryable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> Project<TDestination>(this IQueryable queryable, IProjector projector, params object[] mappingOptions) {

			return queryable.Project<TDestination>(projector, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="Project(IQueryable, IProjector, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="Project(IQueryable, IProjector, Type, Type, MappingOptions)" path="/param[@name='sourceElementType']"/></typeparam>
		/// <inheritdoc cref="Project{TDestination}(IQueryable, IProjector, MappingOptions)" path="/typeparam[@name='TDestination']"/>
		/// <inheritdoc cref="Project(IQueryable, IProjector, Type, MappingOptions)" path="/param[@name='projector']"/>
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="Project{TDestination}(IQueryable, IProjector, MappingOptions)" path="/returns"/>
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/exception"/>
		public static IQueryable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
#pragma warning disable CS1712
			> Project<TSource, TDestination>(this IQueryable<
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

			return queryable.Select(projector.Project<TSource, TDestination>(mappingOptions));
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(IQueryable{TSource}, IProjector, MappingOptions)"/>
		public static IQueryable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> Project<TSource, TDestination>(this IQueryable<
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

			return queryable.Select(projector.Project<TSource, TDestination>(mappingOptions));
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(IQueryable{TSource}, IProjector, MappingOptions)"/>
		public static IQueryable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> Project<TSource, TDestination>(this IQueryable<
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			> queryable,
			IProjector projector, params object[] mappingOptions) {

			return queryable.Select(projector.Project<TSource, TDestination>(mappingOptions));
		}
		#endregion
		#endregion
	}
}
