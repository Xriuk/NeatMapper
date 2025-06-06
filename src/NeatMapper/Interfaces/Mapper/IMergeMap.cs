﻿namespace NeatMapper {
	/// <summary>
	/// Map which allows mapping an object to an existing one, supports open generic types too.
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic.</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic.</typeparam>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IMergeMap<in TSource, TDestination> {
		/// <summary>
		/// Maps an object to an existing one and returns the result.
		/// </summary>
		/// <param name="source">Object to be mapped, may be null.</param>
		/// <param name="destination">Object to map to, may be null.</param>
		/// <param name="context">
		/// Mapping context, which allows nested mappings, services retrieval via DI, additional options, ....
		/// </param>
		/// <returns>
		/// The resulting object of the mapping, can be <paramref name="destination"/> or a new one,
		/// may be null.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		/// <exception cref="MappingException">
		/// An exception was thrown while mapping the types, check the inner exception for details.
		/// </exception>
		TDestination? Map(TSource? source, TDestination? destination, MappingContext context);
	}
}
