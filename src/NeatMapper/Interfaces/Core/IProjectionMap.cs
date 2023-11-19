using System;
using System.Linq.Expressions;

namespace NeatMapper {
	/// <summary>
	/// Map which allows projecting an object to a new one, supports open generic types too.
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic.</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic.</typeparam>
	public interface IProjectionMap<TSource, TDestination> {
		/// <summary>
		/// Projects an object to a new one.
		/// </summary>
		/// <param name="context">
		/// Projection context, which allows nested projections, services retrieval via DI, additional options, ....
		/// </param>
		/// <returns>
		/// An expression which can be used to project an instance of <typeparamref name="TSource"/> type (which may be null)
		/// to an instance <typeparamref name="TDestination"/> type (which may be null).
		/// </returns>
		Expression<Func<
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			,
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			>> Project(ProjectionContext context);
	}
}
