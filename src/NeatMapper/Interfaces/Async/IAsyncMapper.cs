using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Interface which allows mapping an object to a new one or an existing one asynchronously
	/// </summary>
	public interface IAsyncMapper {
		/// <summary>
		/// Maps an object to a new one asynchronously.<br/>
		/// Can also map to collections automatically, will create the destination collection and map each element individually
		/// </summary>
		/// <param name="source">Object to map, may be null</param>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object to create, used to retrieve the available maps</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>T task which when completed returns the newly created object of <paramref name="destinationType"/>, which may be null</returns>
		Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> MapAsync(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Maps an object to an existing one and returns the result.<br/>
		/// Can also map to collections automatically, will try to match elements with <see cref="IMatchMap{TSource, TDestination}"/>
		/// (or the passed <see cref="MergeCollectionsMappingOptions.Matcher"/>), will create the destination collection if it is null and map each element individually
		/// </summary>
		/// <param name="source">Object to be mapped, may be null</param>
		/// <param name="sourceType">Type of the object to be mapped, used to retrieve the available maps</param>
		/// <param name="destination">Object to map to, may be null</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>
		/// A task which when completed returns the resulting object of the mapping of <paramref name="destinationType"/> type,
		/// which can be the same as <paramref name="destination"/> or a new one, may be null
		/// </returns>
		Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> MapAsync(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			destination,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default);
	}
}
