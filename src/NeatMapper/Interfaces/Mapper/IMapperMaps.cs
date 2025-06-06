﻿using System;
using System.Collections.Generic;

namespace NeatMapper {
	/// <summary>
	/// Optional interface to be implemented by mappers, which allows to retrieve a collection of types
	/// which can be mapped by the mapper. If a mapper maps dynamically it should not implement this interface.<br/>
	/// In case of partial implementation, <see cref="System.Linq.Enumerable.Empty{TResult}"/> should be returned,
	/// no exceptions should be thrown.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IMapperMaps : IMapper {
		/// <summary>
		/// Retrieves a collection of type pairs which can be mapped to create new objects. It does not guarantee
		/// that the actual maps will succeed.
		/// </summary>
		/// <param name="mappingOptions">
		/// Additional options which would be used to match the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types.
		/// </param>
		/// <returns>
		/// A collection of type pairs (including generic open types which can be mapped by the mapper,
		/// may contain duplicate type pairs.
		/// </returns>
		IEnumerable<(Type From, Type To)> GetNewMaps(MappingOptions? mappingOptions = null);

		/// <summary>
		/// Retrieves a collection of type pairs which can be mapped to merge objects. It does not guarantee
		/// that the actual maps will succeed.
		/// </summary>
		/// <param name="mappingOptions">
		/// Additional options which would be used to match the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types.
		/// </param>
		/// <returns>
		/// A collection of type pairs (including generic open types which can be mapped by the mapper,
		/// may contain duplicate type pairs.
		/// </returns>
		IEnumerable<(Type From, Type To)> GetMergeMaps(MappingOptions? mappingOptions = null);
	}
}
