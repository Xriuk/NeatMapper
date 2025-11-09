using System;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper.Transitive {
	/// <summary>
	/// <see cref="IProjector"/> which projects types transitively by retrieving all the available maps
	/// from another <see cref="IProjector"/> (via <see cref="IProjectorMaps.GetMaps(MappingOptions)"/>),
	/// the retrieved maps are combined into a graph and the shortest path is solved by using
	/// the Dijkstra algorithm, the resulting expressions are then merged each into the next one by replacing them.
	/// </summary>
	public sealed class TransitiveProjector : IProjector {
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
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output <see cref="MappingOptions"/> (with 
		///	<see cref="MappingOptions.Cached"/> also set to <see langword="true"/>).
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


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
		public TransitiveProjector(IProjector projector, TransitiveOptions? transitiveOptions = null) {
			_projector = projector ?? throw new ArgumentNullException(nameof(projector));
			_graphCreator = new GraphCreator(mappingOptions =>
				(mappingOptions.GetOptions<ProjectorOverrideMappingOptions>()?.Projector
					?? _projector).GetMaps(mappingOptions).Distinct(), transitiveOptions);
			var nestedProjectionContext = new NestedProjectionContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options =>
				options.ReplaceOrAdd<NestedProjectionContext>(
					n => n != null ? new NestedProjectionContext(nestedProjectionContext.ParentProjector, n) : nestedProjectionContext, options.Cached));
		}


		public bool CanProject(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// We cannot project identical types
			if (sourceType == destinationType)
				throw new MapNotFoundException((sourceType, destinationType));

			return _graphCreator.GetOrCreateTypesPath(sourceType, destinationType, _optionsCache.GetOrCreate(mappingOptions)) != null;
		}

		public LambdaExpression Project(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// We cannot project identical types
			if (sourceType == destinationType)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

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
	}
}
