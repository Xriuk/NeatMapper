using System;
using System.Linq.Expressions;

namespace NeatMapper {
	/// <summary>
	/// Interface which allows projecting an object to a new one, can be used in LINQ
	/// (and be translated to external providers like Entity Framework), or can be compiled into delegates.
	/// </summary>
	public interface IProjector {
		/// <summary>
		/// Projects an object to a new one.
		/// </summary>
		/// <param name="sourceType">Type of the object to project, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object to project to, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the projector and/or the maps, null to ignore.
		/// </param>
		/// <returns>
		/// An <see cref="Expression{TDelegate}"/> with a delegate of <see cref="Func{T, TResult}"/> type
		/// which has a parameter of type <paramref name="sourceType"/> (which may be null) and a returned value of
		/// <paramref name="destinationType"/> (which may be null).
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be projected.</exception>
		/// <exception cref="ProjectionException">
		/// An exception was thrown while creating the projection map for the types,
		/// check the inner exception for details.
		/// </exception>
		LambdaExpression Project(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null);
	}
}
