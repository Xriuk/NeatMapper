
namespace NeatMapper.Expressions {
	/// <summary>
	/// <para>
	/// Projector to be used for nested projections.
	/// </para>
	/// <para>
	/// This class is only used inside expressions where it is expanded into the actual nested maps,
	/// so this class should not be used outside of expressions.
	/// </para>
	/// </summary>
	public sealed class NestedProjector {
		/// <summary>
		/// Actual projector to be used to retrieve maps
		/// </summary>
		public IProjector Projector { get; }


		public NestedProjector(IProjector projector) {
			Projector = projector ?? throw new ArgumentNullException(nameof(projector));
		}


		public TDestination Project<TDestination>(object source) {
			throw new InvalidOperationException("NestedProjector cannot be used outside expressions");
		}

		public TDestination Project<TSource, TDestination>(TSource source) {
			throw new InvalidOperationException("NestedProjector cannot be used outside expressions");
		}
	}
}
