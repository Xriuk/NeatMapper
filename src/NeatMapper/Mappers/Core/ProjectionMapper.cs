using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which maps objects by using an <see cref="IProjector"/> to retrieve mapping expressions
	/// which then get compiled and cached into delegates. Maps are created only once, so if the maps
	/// available inside the projector change later they are ignored, any mapping options are forwarded
	/// to the projector the first time the map is created.
	/// Supports only new maps and not merge maps.
	/// </summary>
	public sealed class ProjectionMapper : IMapper, IMapperCanMap {
		private readonly IProjector _projector;
		// (From, To): (source) => destination (can be null if no map exists)
		private readonly IDictionary<(Type From, Type To), Func<object, object>> _mapsCache = new Dictionary<(Type, Type), Func<object, object>>();
		private readonly NestedMappingContext _nestedMappingContext;

		/// <summary>
		/// Creates a new instance of <see cref="ProjectionMapper"/>.
		/// </summary>
		/// <param name="projector"><see cref="IProjector"/> to use to create maps.</param>
		public ProjectionMapper(IProjector projector) {
			_projector = projector ?? throw new ArgumentNullException(nameof(projector));
			_nestedMappingContext = new NestedMappingContext(this);
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source != null && !sourceType.IsAssignableFrom(source.GetType()))
				throw new ArgumentException($"Object of type {source.GetType().FullName ?? source.GetType().Name} is not assignable to type {sourceType.FullName ?? sourceType.Name}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			Func<object, object> map;
			lock (_mapsCache) {
				if(!_mapsCache.TryGetValue((sourceType, destinationType), out map)) {
					// Retrieve the projection from the projector
					LambdaExpression projection;
					try {
						projection = _projector.Project(sourceType, destinationType, MergeOrCreateMappingOptions(mappingOptions));
					}
					catch (MapNotFoundException){
						_mapsCache.Add((sourceType, destinationType), null);
						throw;
					}
					catch(ProjectionException e) {
						throw new MappingException(e.InnerException, (sourceType, destinationType));
					}

					// Compile the expression and wrap it to catch exceptions
					var deleg = projection.Compile();
					map = s => {
						try {
							return deleg.DynamicInvoke(s);
						}
						catch (ProjectionException e){
							throw new MappingException(e.InnerException, (sourceType, destinationType));
						}
						catch(Exception e) {
							throw new MappingException(e, (sourceType, destinationType));
						}
					};
					_mapsCache.Add((sourceType, destinationType), map);
				}
			}

			if(map == null)
				throw new MapNotFoundException((sourceType, destinationType));

			var result = map.Invoke(source);

			// Should not happen
			if (result != null && !destinationType.IsAssignableFrom(result.GetType()))
				throw new InvalidOperationException($"Object of type {result.GetType().FullName ?? result.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}");

			return result;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		// Adds a nested mapping context, this allows projections not suited to be compiled to throw and be ignored
		MappingOptions MergeOrCreateMappingOptions(MappingOptions options) {
			return (options ?? MappingOptions.Empty).ReplaceOrAdd<NestedMappingContext, ProjectionCompilationContext>(
				n => n != null ? new NestedMappingContext(this, n) : _nestedMappingContext,
				p => p ?? ProjectionCompilationContext.Instance);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
