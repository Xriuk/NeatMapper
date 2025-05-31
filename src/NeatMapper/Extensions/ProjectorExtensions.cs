using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	public static class ProjectorExtensions {
		#region CanProject
		#region Runtime
		/// <inheritdoc cref="IProjector.CanProject(Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanProject(this IProjector projector, Type sourceType, Type destinationType, IEnumerable? mappingOptions) {
			return projector.CanProject(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="IProjector.CanProject(Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanProject(this IProjector projector, Type sourceType, Type destinationType, params object?[]? mappingOptions) {
			return projector.CanProject(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IProjector.CanProject(Type, Type, MappingOptions?)" path="/summary"/>
		/// <typeparam name="TSource">
		/// Type of the object to project, used to retrieve the available maps.
		/// </typeparam>
		/// <typeparam name="TDestination">
		/// Type of the destination object to project to, used to retrieve the available maps.
		/// </typeparam>
		/// <inheritdoc cref="IProjector.CanProject(Type, Type, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be projected
		/// to an object of type <typeparamref name="TDestination"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanProject<TSource, TDestination>(this IProjector projector, MappingOptions? mappingOptions = null) {
			return projector.CanProject(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="CanProject{TSource, TDestination}(IProjector, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanProject<TSource, TDestination>(this IProjector projector, IEnumerable? mappingOptions) {
			return projector.CanProject(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="CanProject{TSource, TDestination}(IProjector, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanProject<TSource, TDestination>(this IProjector projector, params object?[]? mappingOptions) {
			return projector.CanProject(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region Project
		#region Runtime
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions?)"/>
		public static LambdaExpression Project(this IProjector projector, Type sourceType, Type destinationType, IEnumerable? mappingOptions) {
			if (projector == null)
				throw new ArgumentNullException(nameof(projector));

			return projector.Project(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions?)"/>
		public static LambdaExpression Project(this IProjector projector, Type sourceType, Type destinationType, params object?[]? mappingOptions) {
			if (projector == null)
				throw new ArgumentNullException(nameof(projector));

			return projector.Project(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions?)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IProjector.Project(Type, Type, MappingOptions?)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IProjector.Project(Type, Type, MappingOptions?)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// An expression which can be used to project an instance of <typeparamref name="TSource"/> type (which may be null)
		/// to an instance <typeparamref name="TDestination"/> type (which may be null).
		/// </returns>
		/// <inheritdoc cref="IProjector.Project(Type, Type, MappingOptions?)" path="/exception"/>
		public static Expression<Func<TSource, TDestination>> Project<TSource, TDestination>(this IProjector projector,
			MappingOptions? mappingOptions = null) {

			if (projector == null)
				throw new ArgumentNullException(nameof(projector));

			return (Expression<Func<TSource, TDestination>>)projector.Project(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(IProjector, MappingOptions?)"/>
		public static Expression<Func<TSource, TDestination>> Project<TSource, TDestination>(this IProjector projector,
			IEnumerable? mappingOptions) {

			if (projector == null)
				throw new ArgumentNullException(nameof(projector));

			return (Expression<Func<TSource, TDestination>>)projector.Project(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(IProjector, MappingOptions?)"/>
		public static Expression<Func<TSource, TDestination>> Project<TSource, TDestination>(this IProjector projector,
			params object?[]? mappingOptions) {

			if (projector == null)
				throw new ArgumentNullException(nameof(projector));

			return (Expression<Func<TSource, TDestination>>)projector.Project(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region GetMaps
		/// <summary>
		/// Retrieves a collection of type pairs which can be projected to create new objects, will check
		/// if the given projector supports <see cref="IProjectorMaps"/> otherwise will return an empty result.
		/// It does not guarantee that the actual projections will succeed.
		/// </summary>
		/// <inheritdoc cref="IProjectorMaps.GetMaps(MappingOptions?)"/>
		public static IEnumerable<(Type From, Type To)> GetMaps(this IProjector projector, MappingOptions? mappingOptions = null) {
			if (projector is IProjectorMaps maps)
				return maps.GetMaps(mappingOptions);
			else
				return [];
		}

		/// <inheritdoc cref="GetMaps(IProjector, MappingOptions?)"/>
		public static IEnumerable<(Type From, Type To)> GetMaps(this IProjector projector, IEnumerable? mappingOptions) {
			if (projector is IProjectorMaps maps)
				return maps.GetMaps(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
			else
				return [];
		}

		/// <inheritdoc cref="GetMaps(IProjector, MappingOptions?)"/>
		public static IEnumerable<(Type From, Type To)> GetMaps(this IProjector projector, params object?[]? mappingOptions) {
			if (projector is IProjectorMaps maps)
				return maps.GetMaps(mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
			else
				return [];
		}
		#endregion
	}
}
