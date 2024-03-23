using System;

namespace NeatMapper {
	/// <summary>
	/// A factory which can be used to map objects of a given type into new objects of another type.<br/>
	/// Created by <see cref="IMapperFactory"/>.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe.</remarks>
	public interface INewMapFactory : IDisposable {
		/// <summary>
		/// Type of the object to map.
		/// </summary>
		Type SourceType { get; }

		/// <summary>
		/// Type of the destination object to create.
		/// </summary>
		Type DestinationType { get; }

		/// <summary>
		/// Maps an object into a new one.
		/// </summary>
		/// <param name="source">Object to map, may be null.</param>
		/// <returns>The newly created object, may be null.</returns>
		/// <exception cref="MapNotFoundException">The provided objects could not be mapped.</exception>
		/// <exception cref="MappingException">An exception was thrown inside the map.</exception>
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
		object?
#else
		object
#endif
			Invoke(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source);
	}
}
