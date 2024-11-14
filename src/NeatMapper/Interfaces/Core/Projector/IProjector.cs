using System;
using System.Linq.Expressions;

namespace NeatMapper {
	/// <summary>
	/// Interface which allows projecting an object to a new one, created expressions can be used in LINQ
	/// (and be translated to external providers like Entity Framework), or can be compiled into delegates.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IProjector {
		/// <summary>
		/// Checks if the projector could project a given object to another. It does not guarantee that the actual map will succeed.
		/// </summary>
		/// <param name="sourceType">Type of the object to project, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object to project to, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to project the types, this helps obtaining more accurate results,
		/// since some projectors may depend on specific options to project or not two given types.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <paramref name="sourceType"/> can be projected
		/// to an object of type <paramref name="destinationType"/>.
		/// </returns>
		bool CanProject(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null);

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
		LambdaExpression Project(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null);
	}
}
