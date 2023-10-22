﻿namespace NeatMapper {
	/// <summary>
	/// Map which allows mapping an object to an existing one, supports open generic types too
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic</typeparam>
	public interface IMergeMap<TSource, TDestination> {
		/// <summary>
		/// Maps an object to an existing one and returns the result
		/// </summary>
		/// <param name="source">Object to be mapped, may be null</param>
		/// <param name="destination">Object to map to, may be null</param>
		/// <param name="context">Mapping context, which allows nested mappings, services retrieval via DI, additional options, ...</param>
		/// <returns>
		/// The resulting object of the mapping, can be <paramref name="destination"/> or a new one,
		/// may be null
		/// </returns>
#if NET5_0_OR_GREATER
		TDestination?
#else
		TDestination
#endif
			Map(
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			destination,
			MappingContext context);
	}
}
