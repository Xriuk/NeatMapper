using System;
using System.Collections;
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
		/// <summary>
		/// Checks if the projector could project a given object to another, will check if the given projector
		/// supports <see cref="IProjectorCanProject"/> first otherwise will try to project the two types.
		/// It does not guarantee that the actual map will succeed.
		/// </summary>
		/// <inheritdoc cref="IProjectorCanProject.CanProject(Type, Type, MappingOptions)"/>
		public static bool CanProject(this IProjector projector,
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

			if (projector == null)
				throw new ArgumentNullException(nameof(projector));
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Check if the projector implements IMapperCanMap, if it throws it means that the map can be checked only when projecting
			if (projector is IProjectorCanProject projectorCanProject)
				return projectorCanProject.CanProject(sourceType, destinationType, mappingOptions);

			// Try projecting the two types
			try {
				projector.Project(sourceType, destinationType, mappingOptions);
				return true;
			}
			catch (MapNotFoundException) {
				return false;
			}
			catch (Exception e) {
				throw new InvalidOperationException(
					"Cannot verify if the projector supports the given projection because it threw an exception while trying to project the types. " +
					"Check inner exception for details.", e);
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="CanProject(IProjector, Type, Type, MappingOptions)"/>
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

		/// <inheritdoc cref="CanProject(IProjector, Type, Type, MappingOptions)"/>
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
		/// <inheritdoc cref="CanProject(IProjector, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IProjectorCanProject.CanProject(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IProjectorCanProject.CanProject(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IProjectorCanProject.CanProject(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be projected
		/// to an object of type <typeparamref name="TDestination"/>.
		/// </returns>
		/// <inheritdoc cref="IProjectorCanProject.CanProject(Type, Type, MappingOptions)" path="/exception"/>
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
	}
}
