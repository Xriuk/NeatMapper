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


		internal EmptyProjector() { }


		public bool CanProject(
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

		public LambdaExpression Project(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			throw new MapNotFoundException((sourceType, destinationType));
		}
	}
}
