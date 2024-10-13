using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper {
	public static class ProjectorExtensions {
		#region Project
		#region Runtime
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)"/>
		public static LambdaExpression
			Project(this IProjector projector,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (projector == null)
				throw new ArgumentNullException(nameof(projector));
			return projector.Project(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)"/>
		public static LambdaExpression Project(this IProjector projector,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (projector == null)
				throw new ArgumentNullException(nameof(projector));
			return projector.Project(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// An expression which can be used to project an instance of <typeparamref name="TSource"/> type (which may be null)
		/// to an instance <typeparamref name="TDestination"/> type (which may be null).
		/// </returns>
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions)" path="/exception"/>
		public static
			Expression<Func<
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
				,
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			>>
			Project<TSource, TDestination>(this IProjector projector,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (projector == null)
				throw new ArgumentNullException(nameof(projector));
			return (Expression<Func<TSource, TDestination>>)projector.Project(typeof(TSource), typeof(TDestination), mappingOptions);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(IProjector, MappingOptions)"/>
		public static
			Expression<Func<
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
				,
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			>>
			Project<TSource, TDestination>(this IProjector projector,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (projector == null)
				throw new ArgumentNullException(nameof(projector));
			return (Expression<Func<TSource, TDestination>>)projector.Project(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(IProjector, MappingOptions)"/>
		public static
			Expression<Func<
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
				,
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			>>
			Project<TSource, TDestination>(this IProjector projector,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (projector == null)
				throw new ArgumentNullException(nameof(projector));
			return (Expression<Func<TSource, TDestination>>)projector.Project(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion
		#endregion

		#region CanProject
		#region Runtime
		/// <inheritdoc cref="IProjector.CanProject(Type, Type, MappingOptions)"/>
		public static bool CanProject(this IProjector projector,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return projector.CanProject(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="IProjector.CanProject(Type, Type, MappingOptions)"/>
		public static bool CanProject(this IProjector projector,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return projector.CanProject(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IProjector.CanProject(Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IProjector.CanProject(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IProjector.CanProject(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IProjector.CanProject(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be projected
		/// to an object of type <typeparamref name="TDestination"/>.
		/// </returns>
		/// <inheritdoc cref="IProjector.CanProject(Type, Type, MappingOptions)" path="/exception"/>
		public static bool CanProject<TSource, TDestination>(this IProjector projector,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return projector.CanProject(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="CanProject{TSource, TDestination}(IProjector, MappingOptions)"/>
		public static bool CanProject<TSource, TDestination>(this IProjector projector,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return projector.CanProject(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="CanProject{TSource, TDestination}(IProjector, MappingOptions)"/>
		public static bool CanProject<TSource, TDestination>(this IProjector projector,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return projector.CanProject(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region GetMaps
		/// <summary>
		/// Retrieves a collection of type pairs which can be projected to create new objects, will check
		/// if the given projector supports <see cref="IProjectorMaps"/> otherwise will return an empty result.
		/// It does not guarantee that the actual projections will succeed.
		/// </summary>
		/// <inheritdoc cref="IProjectorMaps.GetMaps(MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetMaps(this IProjector projector,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (projector is IProjectorMaps maps)
				return maps.GetMaps(mappingOptions);
			else
				return Enumerable.Empty<(Type, Type)>();
		}

		/// <inheritdoc cref="GetMaps(IProjector, MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetMaps(this IProjector projector,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			if (projector is IProjectorMaps maps)
				return maps.GetMaps(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
			else
				return Enumerable.Empty<(Type, Type)>();
		}

		/// <inheritdoc cref="GetMaps(IProjector, MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetMaps(this IProjector projector,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			if (projector is IProjectorMaps maps)
				return maps.GetMaps(mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
			else
				return Enumerable.Empty<(Type, Type)>();
		}
		#endregion
	}
}
