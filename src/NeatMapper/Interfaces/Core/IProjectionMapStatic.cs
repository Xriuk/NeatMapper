#if NET7_0_OR_GREATER
using System;
using System.Linq.Expressions;

namespace NeatMapper {
	/// <summary>
	/// Map which allows projecting an object to a new one, supports open generic types too.
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic.</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic.</typeparam>
	/// <remarks>
	/// This interface is the same as <see cref="IProjectionMap{TSource, TDestination}"/>, but allows greater flexibility:
	/// for example it can be used in classes which cannot be instantiated (which do not have parameterless constructors).
	/// </remarks>
	public interface IProjectionMapStatic<TSource, TDestination> {
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
		public static abstract Expression<Func<TSource?, TDestination?>> Project(ProjectionContext context);
	}
}
#endif