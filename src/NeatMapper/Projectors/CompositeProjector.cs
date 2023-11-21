using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IProjector"/> which delegates projection to other <see cref="IProjector"/>s,
	/// this allows to combine different projection capabilities.<br/>
	/// Each projector is invoked in order and the first one to succeed in projection is returned.
	/// </summary>
	public sealed class CompositeProjector : IProjector, IProjectorCanProject {
		private readonly IList<IProjector> _projectors;
		private readonly NestedProjectionContext _nestedProjectionContext;

		/// <summary>
		/// Creates a new instance of <see cref="CompositeProjector"/>.
		/// </summary>
		/// <param name="projectors">Projectors to delegate the projection to.</param>
		public CompositeProjector(params IProjector[] projectors) : this((IList<IProjector>)projectors) { }

		/// <summary>
		/// Creates a new instance of <see cref="CompositeProjector"/>.
		/// </summary>
		/// <param name="projectors">Projectors to delegate the projection to.</param>
		public CompositeProjector(IList<IProjector> projectors) {
			if (projectors == null)
				throw new ArgumentNullException(nameof(projectors));

			_projectors = new List<IProjector>(projectors);
			_nestedProjectionContext = new NestedProjectionContext(this);
		}


		public LambdaExpression Project(
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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			foreach (var projector in _projectors) {
				try {
					return projector.Project(sourceType, destinationType, mappingOptions);
				}
				catch (MapNotFoundException) { }
			}

			throw new MapNotFoundException((sourceType, destinationType));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public bool CanProject(
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

			mappingOptions = MergeOrCreateMappingOptions(mappingOptions);

			// Check if any projector implements IProjectorCanProject, if one of them throws it means that the map can be checked only when projecting
			var undeterminateProjectors = new List<IProjector>();
			foreach (var projector in _projectors.OfType<IProjectorCanProject>()) {
				try {
					if (projector.CanProject(sourceType, destinationType, mappingOptions))
						return true;
				}
				catch (InvalidOperationException) {
					undeterminateProjectors.Add(projector);
				}
			}

			// Try projecting the types
			var projectorsLeft = _projectors.Where(p => !(p is IProjectorCanProject) || undeterminateProjectors.IndexOf(p) != -1);
			if (projectorsLeft.Any()) {
				foreach (var projector in projectorsLeft) {
					try {
						projector.Project(sourceType, destinationType, mappingOptions);
						return true;
					}
					catch (MapNotFoundException) { }
				}
			}

			if (undeterminateProjectors.Count > 0)
				throw new InvalidOperationException("Cannot verify if the projector supports the given map");
			else
				return false;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		// Will override a projector if not already overridden
		MappingOptions MergeOrCreateMappingOptions(MappingOptions options) {
			return (options ?? MappingOptions.Empty).ReplaceOrAdd<ProjectorOverrideMappingOptions, NestedProjectionContext>(
				p => p?.Projector != null ? p : new ProjectorOverrideMappingOptions(this, p?.ServiceProvider),
				n => n != null ? new NestedProjectionContext(this, n) : _nestedProjectionContext);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
