﻿using System.Threading.Tasks;

namespace NeatMapper.Async {
	/// <summary>
	/// Map which allows mapping an object to an existing one asynchronously
	/// </summary>
	/// <typeparam name="TSource">Source type</typeparam>
	/// <typeparam name="TDestination">Destination type</typeparam>
	public interface IAsyncMergeMap<TSource, TDestination> {
		/// <summary>
		/// Maps an object to an existing one asynchronously and returns the result
		/// </summary>
		/// <param name="source">object to be mapped, may be null</param>
		/// <param name="destination">object to map to, may be null</param>
		/// <param name="context">mapping context, which allows nested mappings, services retrieval via DI, ...</param>
		/// <returns>
		/// a task which when completed returns the resulting object of the mapping,
		/// can be <paramref name="destination"/> or a new one, may be null
		/// </returns>
		Task<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> MapAsync(
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
			AsyncMappingContext context);
	}
}
