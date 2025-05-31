using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Optional interface to be implemented by projectors, which allows to retrieve a collection of types
	/// which can be projected by the projector. If a projector projects dynamically it should not implement this interface.<br/>
	/// In case of partial implementation, <see cref="System.Linq.Enumerable.Empty{TResult}"/> should be returned,
	/// no exceptions should be thrown.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface IProjectorMaps : IProjector {
		/// <summary>
		/// Retrieves a collection of type pairs which can be projected to create new objects. It does not guarantee
		/// that the actual projections will succeed.
		/// </summary>
		/// <param name="mappingOptions">
		/// Additional options which would be used to match the types, this helps obtaining more accurate results,
		/// since some projectors may depend on specific options to project or not two given types.
		/// </param>
		/// <returns>
		/// A collection of type pairs which can be projected by the projector, may contain duplicate type pairs.
		/// </returns>
		IEnumerable<(Type From, Type To)> GetMaps(MappingOptions? mappingOptions = null);
	}
}
