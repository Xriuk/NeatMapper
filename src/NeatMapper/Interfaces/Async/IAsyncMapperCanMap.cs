using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Optional interface to be implemented by asynchronous mappers, which allows to discover if two given types can be mapped or not
	/// </summary>
	public interface IAsyncMapperCanMap : IAsyncMapper {
		/// <summary>
		/// Checks if the mapper can create a new object from a given one asynchronously
		/// </summary>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object to create, used to retrieve the available maps</param>
		/// <param name="cancellationToken">Cancellation token used to interrupt asynchronous operations</param>
		/// <returns>True if an object of <paramref name="destinationType"/> can be created from a parameter of <paramref name="sourceType"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		Task<bool> CanMapAsyncNew(Type sourceType, Type destinationType, CancellationToken cancellationToken = default);

		/// <summary>
		/// Checks if the mapper can merge an object into an existing one asynchronously
		/// </summary>
		/// <param name="sourceType">Type of the object to be mapped, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <param name="cancellationToken">Cancellation token used to interrupt asynchronous operations</param>
		/// <returns>True if an object of <paramref name="sourceType"/> can be merged into an object of <paramref name="destinationType"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		Task<bool> CanMapAsyncMerge(Type sourceType, Type destinationType, CancellationToken cancellationToken = default);
	}
}
