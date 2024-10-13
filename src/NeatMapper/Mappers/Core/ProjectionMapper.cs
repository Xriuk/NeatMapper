using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which maps objects by using an <see cref="IProjector"/> to retrieve mapping expressions
	/// which then get compiled and cached into delegates. Maps are created only once, so if the maps
	/// available inside the projector change later they are ignored, any mapping options are forwarded
	/// to the projector the first time the map is created.<br/>
	/// Supports only new maps and not merge maps.
	/// </summary>
	public sealed class ProjectionMapper : IMapper, IMapperFactory, IMapperMaps {
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


		/// <summary>
		/// <see cref="IProjector"/> which is used to create expressions to compile into delegates.
		/// </summary>
		private readonly IProjector _projector;

		/// <summary>
		/// Compiled maps cache, value can be null if no map exists.
		/// </summary>
		private readonly ConcurrentDictionary<(Type From, Type To), Func<object, object>> _mapsCache =
			new ConcurrentDictionary<(Type, Type), Func<object, object>>();


		/// <summary>
		/// Creates a new instance of <see cref="ProjectionMapper"/>.
		/// </summary>
		/// <param name="projector"><see cref="IProjector"/> to use to create maps.</param>
		public ProjectionMapper(IProjector projector) {
			_projector = projector ?? throw new ArgumentNullException(nameof(projector));
		}


		#region IMapper methods
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

			if (_mapsCache.TryGetValue((sourceType, destinationType), out var map))
				return map != null;
			else
				return _projector.CanProject(sourceType, destinationType, MergeOrCreateMappingOptions(mappingOptions));
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

			using(var factory = MapNewFactory(sourceType, destinationType, mappingOptions)) { 
				return factory.Invoke(source);
			}
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

		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(
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

			var deleg = _mapsCache.GetOrAdd((sourceType, destinationType), types => {
				// Retrieve the projection from the projector
				LambdaExpression projection;
				try {
					projection = _projector.Project(sourceType, destinationType, MergeOrCreateMappingOptions(mappingOptions));
				}
				catch (ProjectionException e) {
					throw new MappingException(e.InnerException, (sourceType, destinationType));
				}
				catch (MapNotFoundException) {
					return null;
				}
				catch (OperationCanceledException) {
					throw;
				}

				// Convert the expression to accept and return object types
				var param = Expression.Parameter(typeof(object), "source");
				projection = Expression.Lambda(
					Expression.Convert(new LambdaParameterReplacer(Expression.Convert(param, projection.Parameters.Single().Type)).SetupAndVisitBody(projection), typeof(object)),
					param);

				// Compile the expression and wrap it to catch exceptions
				return (Func<object, object>)projection.Compile();
			});
			if (deleg == null)
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultNewMapFactory(sourceType, destinationType, source => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));

				object result;
				try {
					result = deleg.Invoke(source);
				}
				catch (ProjectionException e) {
					throw new MappingException(e.InnerException, (sourceType, destinationType));
				}
				catch (MapNotFoundException e) {
					if (e.From == sourceType && e.To == destinationType)
						throw;
					else
						throw new MappingException(e, (sourceType, destinationType));
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception e) {
					throw new MappingException(e, (sourceType, destinationType));
				}

				// Should not happen
				TypeUtils.CheckObjectType(result, destinationType);

				return result;
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public IMergeMapFactory MapMergeFactory(
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

		#region IMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetNewMaps(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return _projector.GetMaps(mappingOptions);
		}

		public IEnumerable<(Type From, Type To)> GetMergeMaps(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return Enumerable.Empty<(Type, Type)>();
		}
		#endregion
	}
}
