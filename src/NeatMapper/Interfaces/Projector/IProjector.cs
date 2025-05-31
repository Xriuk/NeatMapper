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
		/// Checks if the projector could project a given object to another.
		/// </summary>
		/// <param name="sourceType">
		/// Type of the object to project, used to retrieve the available maps. Can be an open generic type.
		/// </param>
		/// <param name="destinationType">
		/// Type of the destination object to project to, used to retrieve the available maps. Can be
		/// an open generic type.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to project the types, this helps obtaining more accurate results,
		/// since some projectors may depend on specific options to project or not two given types.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <paramref name="sourceType"/> can be projected
		/// to an object of type <paramref name="destinationType"/>.
		/// </returns>
		/// <remarks>
		/// When checking for open generic types the method might return true but some concrete generic types
		/// might not be projectable because of various constraints or missing nested maps.
		/// So you should really use open generic types to check if two types are never projectable.
		/// </remarks>
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
		/// which has a parameter of type <paramref name="sourceType"/> and a returned value of
		/// <paramref name="destinationType"/>.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be projected.</exception>
		/// <exception cref="ProjectionException">
		/// An exception was thrown while creating the projection map for the types,
		/// check the inner exception for details.
		/// </exception>
		LambdaExpression Project(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null);
	}
}
