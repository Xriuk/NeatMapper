using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which maps objects by using an <see cref="IProjector"/> to retrieve mapping expressions
	/// which then get compiled and cached into delegates. Maps are created only once, so if the maps
	/// available inside the projector change later they are ignored, any mapping options are forwarded
	/// to the projector the first time the map is created.<br/>
	/// Supports only new maps and not merge maps.
	/// </summary>
	public sealed class ProjectionMapper : IMapper, IMapperCanMap, IMapperFactory {
		private readonly IProjector _projector;
		// (From, To): (source) => destination (can be null if no map exists)
		private readonly IDictionary<(Type From, Type To), Func<object, object>> _mapsCache = new Dictionary<(Type, Type), Func<object, object>>();

		/// <summary>
		/// Creates a new instance of <see cref="ProjectionMapper"/>.
		/// </summary>
		/// <param name="projector"><see cref="IProjector"/> to use to create maps.</param>
		public ProjectionMapper(IProjector projector) {
			_projector = projector ?? throw new ArgumentNullException(nameof(projector));
		}


		#region IMapper methods
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			Map(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return MapNewFactory(sourceType, destinationType, mappingOptions).Invoke(source);
		}

		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			Map(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			destination,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			// Not mapping merge
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion

		#region IMapperCanMap methods
		public bool CanMapNew(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			lock (_mapsCache) {
				if (_mapsCache.TryGetValue((sourceType, destinationType), out var map))
					return map != null;
				else
					return _projector.CanProject(sourceType, destinationType, MergeOrCreateMappingOptions(mappingOptions));
			}
		}

		public bool CanMapMerge(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {
			return false;
		}
		#endregion

		#region IMapperFactory methods
		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?, object?
#else
			object, object
#endif
			> MapNewFactory(
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

			Func<object, object> map;
			lock (_mapsCache) {
				if (!_mapsCache.TryGetValue((sourceType, destinationType), out map)) {
					// Retrieve the projection from the projector
					LambdaExpression projection;
					try {
						projection = _projector.Project(sourceType, destinationType, MergeOrCreateMappingOptions(mappingOptions));
					}
					catch (ProjectionException e) {
						throw new MappingException(e.InnerException, (sourceType, destinationType));
					}
					catch (MapNotFoundException) {
						_mapsCache.Add((sourceType, destinationType), null);
						throw;
					}
					catch (TaskCanceledException) {
						throw;
					}

					// Compile the expression and wrap it to catch exceptions, we also cache it
					var deleg = projection.Compile();
					map = source => {
						TypeUtils.CheckObjectType(source, sourceType, nameof(source));

						object result;
						try {
							result = deleg.DynamicInvoke(source);
						}
						catch (TargetInvocationException e) {
							if (e.InnerException is TaskCanceledException || e.InnerException is MapNotFoundException)
								throw e.InnerException;
							else if (e.InnerException is ProjectionException pe)
								throw new MappingException(pe.InnerException, (sourceType, destinationType));
							else
								throw new MappingException(e.InnerException, (sourceType, destinationType));
						}
						catch (ProjectionException e) {
							throw new MappingException(e.InnerException, (sourceType, destinationType));
						}
						catch (MapNotFoundException) {
							throw;
						}
						catch (TaskCanceledException) {
							throw;
						}
						catch (Exception e) {
							throw new MappingException(e, (sourceType, destinationType));
						}

						// Should not happen
						TypeUtils.CheckObjectType(result, destinationType);

						return result;
					};
					_mapsCache.Add((sourceType, destinationType), map);
				}
			}

			if (map == null)
				throw new MapNotFoundException((sourceType, destinationType));

			return map;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?, object?, object?
#else
			object, object, object
#endif
			> MapMergeFactory(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			// Not mapping merge
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		// Adds a compilation context, this allows projections not suited to be compiled to throw and be ignored
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static MappingOptions MergeOrCreateMappingOptions(MappingOptions options) {
			return (options ?? MappingOptions.Empty)
				.ReplaceOrAdd<ProjectionCompilationContext>(_ => ProjectionCompilationContext.Instance);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
