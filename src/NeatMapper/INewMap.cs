﻿namespace NeatMapper {
	/// <summary>
	/// Map which allows mapping an object to a new one
	/// </summary>
	/// <typeparam name="TSource">Source type</typeparam>
	/// <typeparam name="TDestination">Destination type</typeparam>
	public interface INewMap<TSource, TDestination> {
		/// <summary>
		/// Maps an object to a new one
		/// </summary>
		/// <param name="source">object to map, may be null</param>
		/// <param name="context">mapping context, which allows nested mappings, services retrieval via DI, ...</param>
		/// <returns>the newly created object, may be null</returns>
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
			MappingContext context);
	}
}
