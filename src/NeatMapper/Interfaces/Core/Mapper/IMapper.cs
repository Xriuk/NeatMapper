﻿using System;

namespace NeatMapper {
	/// <summary>
	/// Interface which allows mapping an object to a new one or an existing one.
	/// </summary>
	/// <remarks>
	/// Note to implementers: if a mapper does not support one of the methods
	/// (<see cref="Map(object, Type, Type, MappingOptions)"/> or
	/// <see cref="Map(object, Type, object, Type, MappingOptions)"/>) it should throw
	/// <see cref="MapNotFoundException"/> inside.<br/>
	/// Implementations of this interface must be thread-safe.
	/// </remarks>
	public interface IMapper {
		/// <summary>
		/// Maps an object to a new one.
		/// </summary>
		/// <param name="source">Object to map, may be null.</param>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object to create, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore.
		/// </param>
		/// <returns>The newly created object of type <paramref name="destinationType"/>, may be null.</returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		/// <exception cref="MappingException">
		/// An exception was thrown while mapping the types, check the inner exception for details.
		/// </exception>
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
		object?
#else
		object
#endif
			Map(
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
			mappingOptions = null);

		/// <summary>
		/// Maps an object to an existing one and returns the result.
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
		/// <returns>
		/// The resulting object of the mapping of type <paramref name="destinationType"/>, can be the same as
		/// <paramref name="destination"/> or a new one, may be null.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		/// <exception cref="MappingException">
		/// An exception was thrown while mapping the types, check the inner exception for details.
		/// </exception>
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
		object?
#else
		object
#endif
		Map(
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
			mappingOptions = null);
	}
}
