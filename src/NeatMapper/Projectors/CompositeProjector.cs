﻿using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IProjector"/> which delegates projection to other <see cref="IProjector"/>s,
	/// this allows to combine different projection capabilities.<br/>
	/// Each projector is invoked in order and the first one to succeed in projection is returned.
	/// </summary>
	public sealed class CompositeProjector : IProjector, IProjectorMaps {
		/// <summary>
		/// List of <see cref="IProjector"/>s to be tried in order when projecting types.
		/// </summary>
		private readonly IReadOnlyList<IProjector> _projectors;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		/// <summary>
		/// Creates a new instance of <see cref="CompositeProjector"/>.
		/// </summary>
		/// <param name="projectors">Projectors to delegate the projection to.</param>
		public CompositeProjector(params IProjector[] projectors) : this((IList<IProjector>)projectors ?? throw new ArgumentNullException(nameof(projectors))) { }
		/// <summary>
		/// Creates a new instance of <see cref="CompositeProjector"/>.
		/// </summary>
		/// <param name="projectors">Projectors to delegate the projection to.</param>
		public CompositeProjector(IList<IProjector> projectors) {
			_projectors = new List<IProjector>(projectors ?? throw new ArgumentNullException(nameof(projectors)));
			var nestedProjectionContext = new NestedProjectionContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<ProjectorOverrideMappingOptions, NestedProjectionContext>(
				p => p?.Projector != null ? p : new ProjectorOverrideMappingOptions(this, p?.ServiceProvider),
				n => n != null ? new NestedProjectionContext(this, n) : nestedProjectionContext));
		}


		public bool CanProject(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			return _projectors.Any(p => p.CanProject(sourceType, destinationType, mappingOptions));
		}

		public LambdaExpression Project(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var projector = _projectors.FirstOrDefault(p => p.CanProject(sourceType, destinationType, mappingOptions));
			if (projector != null)
				return projector.Project(sourceType, destinationType, mappingOptions);
			else
				throw new MapNotFoundException((sourceType, destinationType));
		}

		public IEnumerable<(Type From, Type To)> GetMaps(MappingOptions? mappingOptions = null) {
			return _projectors.SelectMany(m => m.GetMaps(mappingOptions));
		}
	}
}
