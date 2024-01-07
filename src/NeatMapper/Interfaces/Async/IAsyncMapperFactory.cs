using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Optional interface to be implemented by asynchronous mappers, which allows to create a factory to map objects
	/// of given types, instead of mapping them directly. The created factories should share the same
	/// <see cref="AsyncMappingContext"/> (and thus the same <see cref="CancellationToken"/> too).<br/>
	/// The implementation should be more efficient than calling IAsyncMapper.MapAsync() multiple times,
	/// and thus can be used for collections.
	/// </summary>
	public interface IAsyncMapperFactory : IAsyncMapper {
		/// <summary>
		/// Creates a factory to map an object to a new one asynchronously.
		/// </summary>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object to create, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore.
		/// </param>
		/// <param name="cancellationToken">
		/// Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping.
		/// </param>
		/// <returns>
		/// A factory which can be used to map objects of type <paramref name="sourceType"/> into new objects
		/// of type <paramref name="destinationType"/> asynchronously.<br/>
		/// The factory when invoked may throw <see cref="MapNotFoundException"/> or <see cref="MappingException"/> exceptions.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
		object?, Task<object?>
#else
		object, Task<object>
#endif
			> MapAsyncNewFactory(
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
		/// Creates a factory to map an object to an existing one asynchronously.
		/// </summary>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object to create, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore.
		/// </param>
		/// <param name="cancellationToken">
		/// Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping.
		/// </param>
		/// <returns>
		/// A factory which can be used to map objects of type <paramref name="sourceType"/> into existing objects
		/// of type <paramref name="destinationType"/> asynchronously.<br/>
		/// The factory when invoked may throw <see cref="MapNotFoundException"/> or <see cref="MappingException"/> exceptions.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
		object?, object?, Task<object?>
#else
		object, object, Task<object>
#endif
			> MapAsyncMergeFactory(
			Type sourceType,
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
