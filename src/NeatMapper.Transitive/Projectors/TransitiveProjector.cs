using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace NeatMapper.Transitive {
	/// <summary>
	/// <see cref="IProjector"/> which projects types transitively by retrieving all the available maps
	/// from another <see cref="IProjector"/> (via <see cref="IProjectorMaps.GetMaps(MappingOptions)"/>),
	/// the retrieved maps are combined into a graph and the shortest path is solved by using
	/// the Dijkstra algorithm, the resulting expressions are then merged each into the next one by replacing them.
	/// </summary>
	public sealed class TransitiveProjector : IProjector {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private class ParameterReplacer : ExpressionVisitor {
			private readonly ParameterExpression _search;
			private readonly Expression _replace;

			public ParameterReplacer(ParameterExpression search, Expression replace) {
				_search = search;
				_replace = replace;
			}


			protected override Expression VisitParameter(ParameterExpression node) {
				if(node != _search)
					return base.VisitParameter(node);
				else
					return _replace;
			}
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


		/// <summary>
		/// <see cref="IProjector"/> which is used to project the types, will be also provided as a nested projector
		/// in <see cref="ProjectorOverrideMappingOptions"/> (if not already present).
		/// </summary>
		private readonly IProjector _projector;

		/// <summary>
		/// Instance used to create and retrieve type chains.
		/// </summary>
		private readonly GraphCreator _graphCreator;

		/// <summary>
		/// Cached nested context with no parents.
		/// </summary>
		private readonly NestedProjectionContext _nestedProjectionContext;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output <see cref="MappingOptions"/> (with 
		///	<see cref="MappingOptions.Cached"/> also set to <see langword="true"/>).
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MappingOptions> _optionsCache =
			new ConcurrentDictionary<MappingOptions, MappingOptions>();

		/// <summary>
		/// Cached output <see cref="MappingOptions"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have null keys) and <see cref="MappingOptions.Empty"/> inputs,
		/// also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		private readonly MappingOptions _optionsCacheNull;


		/// <summary>
		/// Creates a new instance of <see cref="TransitiveProjector"/>.
		/// </summary>
		/// <param name="projector">
		/// <see cref="IProjector"/> to use to project types.<br/>
		/// Can be overridden during mapping with <see cref="ProjectorOverrideMappingOptions.Projector"/>.
		/// </param>
		/// <param name="transitiveOptions">
		/// Options to apply when mapping types.<br/>
		/// Can be overridden during mapping with <see cref="TransitiveMappingOptions"/>.
		/// </param>
		public TransitiveProjector(IProjector projector,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			TransitiveOptions?
#else
			TransitiveOptions
#endif
			transitiveOptions = null) {

			_projector = projector ?? throw new ArgumentNullException(nameof(projector));
			_graphCreator = new GraphCreator(mappingOptions =>
				(mappingOptions.GetOptions<ProjectorOverrideMappingOptions>()?.Projector
					?? _projector).GetMaps(mappingOptions).Distinct(), transitiveOptions);

			_nestedProjectionContext = new NestedProjectionContext(this);
			_optionsCacheNull = MergeMappingOptions(MappingOptions.Empty);
		}


		public bool CanProject(Type sourceType, Type destinationType,
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

			// We cannot project identical types
			if (sourceType == destinationType)
				throw new MapNotFoundException((sourceType, destinationType));

			return _graphCreator.GetOrCreateTypesPath(sourceType, destinationType, MergeOrCreateMappingOptions(mappingOptions)) != null;
		}

		public LambdaExpression Project(Type sourceType, Type destinationType,
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

			// We cannot project identical types
			if (sourceType == destinationType)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			var typesPath = _graphCreator.GetOrCreateTypesPath(sourceType, destinationType, mappingOptions)
				?? throw new MapNotFoundException((sourceType, destinationType));

			var projector = mappingOptions.GetOptions<ProjectorOverrideMappingOptions>()?.Projector
				?? _projector;

			try {
				return typesPath.Zip(typesPath.Skip(1), (t1, t2) => (From: t1, To: t2))
					.Select(types => projector.Project(types.From, types.To, mappingOptions))
					.Aggregate((result, expression) => Expression.Lambda(new ParameterReplacer(expression.Parameters.Single(), result.Body).Visit(expression.Body), result.Parameters.Single()));
			}
			catch (MapNotFoundException) {
				throw new MapNotFoundException((sourceType, destinationType));
			}
			catch (OperationCanceledException) {
				throw;
			}
			catch (Exception e) {
				throw new ProjectionException(e, (sourceType, destinationType));
			}
		}


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private MappingOptions MergeOrCreateMappingOptions(MappingOptions options) {
			if (options == null || options == MappingOptions.Empty)
				return _optionsCacheNull;
			else if (options.Cached)
				return _optionsCache.GetOrAdd(options, MergeMappingOptions);
			else
				return MergeMappingOptions(options);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MappingOptions MergeMappingOptions(MappingOptions options) {
			return options.ReplaceOrAdd<NestedProjectionContext>(
				n => n != null ? new NestedProjectionContext(_nestedProjectionContext.ParentProjector, n) : _nestedProjectionContext, options.Cached);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
