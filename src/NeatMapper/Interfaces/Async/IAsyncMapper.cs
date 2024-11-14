using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Interface which allows mapping an object to a new one or an existing one asynchronously.
	/// </summary>
	/// <remarks>
	/// Note to implementers: if a mapper does not support one of the methods
	/// (<see cref="MapAsync(object?, Type, Type, MappingOptions, CancellationToken)"/> or
	/// <see cref="MapAsync(object?, Type, object?, Type, MappingOptions, CancellationToken)"/>) it should throw
	/// <see cref="MapNotFoundException"/> inside.<br/>
	/// Implementations of this interface must be thread-safe.
	/// </remarks>
	public interface IAsyncMapper {
		/// <summary>
		/// Checks if the mapper can create a new object from a given one asynchronously.
		/// </summary>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object to create, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to map the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <paramref name="destinationType"/> can be created
		/// from a parameter of type <paramref name="sourceType"/>.
		/// </returns>
		bool CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null);

		/// <summary>
		/// Checks if the mapper can merge an object into an existing one asynchronously.
		/// </summary>
		/// <param name="sourceType">Type of the object to be mapped, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to map the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <paramref name="sourceType"/> can be merged
		/// into an object of type <paramref name="destinationType"/>.
		/// </returns>
		bool CanMapAsyncMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null);

		/// <summary>
		/// Maps an object to a new one asynchronously.
		/// </summary>
		/// <param name="source">Object to map, may be null.</param>
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
		/// A task which when completed returns the newly created object of type <paramref name="destinationType"/>,
		/// which may be null.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		/// <exception cref="MappingException">
		/// An exception was thrown while mapping the types, check the inner exception for details.
		/// </exception>
		Task<object?> MapAsync(
			object? source,
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Maps an object to an existing one and returns the result asynchronously.
		/// </summary>
		/// <param name="source">Object to be mapped, may be null.</param>
		/// <param name="sourceType">Type of the object to be mapped, used to retrieve the available maps.</param>
		/// <param name="destination">Object to map to, may be null.</param>
		/// <param name="destinationType">
		/// Type of the destination object, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore.
		/// </param>
		/// <param name="cancellationToken">
		/// Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping.
		/// </param>
		/// <returns>
		/// A task which when completed returns the resulting object of the mapping of type
		/// <paramref name="destinationType"/>, which can be the same as <paramref name="destination"/> or a new one,
		/// may be null.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		/// <exception cref="MappingException">
		/// An exception was thrown while mapping the types, check the inner exception for details.
		/// </exception>
		Task<object?> MapAsync(
			object? source,
			Type sourceType,
			object? destination,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default);
	}
}
