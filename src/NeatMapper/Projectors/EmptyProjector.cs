using System;
using System.Linq.Expressions;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IProjector"/> which cannot project any type.
	/// </summary>
	public sealed class EmptyProjector : IProjector {
		/// <summary>
		/// Singleton instance of the projector.
		/// </summary>
		public static readonly IProjector Instance = new EmptyProjector();


		private EmptyProjector() { }


		public bool CanProject(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return false;
		}

		public LambdaExpression Project(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			throw new MapNotFoundException((sourceType, destinationType));
		}
	}
}
