using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper.Async {
	/// <summary>
	/// Interface which allows mapping an object to a new one or an existing one asynchronously
	/// </summary>
	public interface IAsyncMapper {
		/// <summary>
		/// Maps an object to a new one asynchronously.<br/>
		/// Can also map to collections automatically, will create the destination collection and map each element individually
		/// </summary>
		/// <param name="source">object to map, may be null</param>
		/// <param name="sourceType">type of the object to map, used to retrieve the available maps</param>
		/// <param name="destinationType">type of the destination object to create, used to retrieve the available maps</param>
		/// <param name="mappingOptions">additional options for the current map, null to use default ones</param>
		/// <param name="cancellationToken">cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>a task which when completed returns the newly created object of <paramref name="destinationType"/>, which may be null</returns>
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
			AsyncMappingOptions?
#else
			AsyncMappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Maps an object to an existing one and returns the result.<br/>
		/// Can also map to collections automatically, will try to match elements with <see cref="IMatchMap{TSource, TDestination}"/>
		/// (or the passed <see cref="MappingOptions.Matcher"/>), will create the destination collection if it is null and map each element individually
		/// </summary>
		/// <param name="source">object to be mapped, may be null</param>
		/// <param name="sourceType">type of the object to be mapped, used to retrieve the available maps</param>
		/// <param name="destination">object to map to, may be null</param>
		/// <param name="destinationType">type of the destination object, used to retrieve the available maps</param>
		/// <param name="mappingOptions">additional options for the current map, null to use default ones</param>
		/// <param name="cancellationToken">cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>
		/// a task which when completed returns the resulting object of the mapping of <paramref name="destinationType"/> type,
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
			AsyncMappingOptions?
#else
			AsyncMappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default);
	}
}
