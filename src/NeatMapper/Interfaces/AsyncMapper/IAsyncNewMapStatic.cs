﻿#if NET7_0_OR_GREATER
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Map which allows mapping an object to a new one asynchronously, supports open generic types too
	/// </summary>
	/// <typeparam name="TSource">Source type, can be an open generic</typeparam>
	/// <typeparam name="TDestination">Destination type, can be an open generic</typeparam>
	/// <remarks>
	/// This interface is the same as <see cref="IAsyncNewMap{TSource, TDestination}"/>, but allows greater flexibility:
	/// for example it can be used in classes which cannot be instantiated (which do not have parameterless constructors).<br/>
	/// Implementations of this interface must be thread-safe.
	/// </remarks>
	public interface IAsyncNewMapStatic<in TSource, TDestination> {
		/// <summary>
		/// Maps an object to a new one asynchronously
		/// </summary>
		/// <param name="source">Object to map, may be null</param>
		/// <param name="context">Mapping context, which allows nested mappings, services retrieval via DI, ...</param>
		/// <returns>A task which when completed returns the newly created object, which may be null</returns>
		public static abstract Task<TDestination?> MapAsync(TSource? source, AsyncMappingContext context);
	}
}
#endif