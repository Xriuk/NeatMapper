﻿namespace NeatMapper {
	/// <summary>
	/// Delegate which allows mapping an object to a new one, used to add custom <see cref="INewMap{TSource, TDestination}"/>
	/// </summary>
	/// <typeparam name="TSource">Source type</typeparam>
	/// <typeparam name="TDestination">Destination type</typeparam>
	/// <param name="source">object to map, may be null</param>
	/// <param name="context">mapping context, which allows nested mappings, services retrieval via DI, ...</param>
	/// <returns>the newly created object, may be null</returns>
	public delegate
#if NET5_0_OR_GREATER
		TDestination?
#else
		TDestination
#endif
		NewMapDelegate<TSource, TDestination>(
#if NET5_0_OR_GREATER
		TSource?
#else
		TSource
#endif
		source,
		MappingContext context);
}
