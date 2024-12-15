﻿namespace NeatMapper {
	/// <summary>
	/// Optional interface which allows checking if a given mapper can map the given types asynchronously.
	/// Mostly useful for generic types because allows checking the inner generic type arguments.
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic.</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic.</typeparam>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface ICanMapAsyncMerge<in TSource, TDestination> : IAsyncMergeMap<TSource, TDestination> {
		/// <summary>
		/// Checks if the implemented <see cref="IAsyncMergeMap{TSource, TDestination}"/> could merge
		/// an object into an existing one asynchronously.
		/// </summary>
		/// <param name="context">
		/// Mapping context, which allows checking nested mappings, services retrieval via DI, additional options, ....
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be merged
		/// into an object of type <typeparamref name="TDestination"/> asynchronously.
		/// </returns>
		bool CanMapAsyncMerge(AsyncMappingContextOptions context);
	}
}