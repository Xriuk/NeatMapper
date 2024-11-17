using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper{
	/// <summary>
	/// Singleton <see cref="IAsyncMapper"/> which cannot map any type.
	/// </summary>
	public sealed class AsyncEmptyMapper : IAsyncMapper, IAsyncMapperFactory {
		/// <summary>
		/// Singleton instance of the mapper
		/// </summary>
		public static readonly IAsyncMapper Instance = new AsyncEmptyMapper();


		private AsyncEmptyMapper() { }


		#region IAsyncMapper methods
		public bool CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return false;
		}

		public bool CanMapAsyncMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return false;
		}

		public Task<object?> MapAsync(
			object? source,
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			throw new MapNotFoundException((sourceType, destinationType));
		}

		public Task<object?> MapAsync(
			object? source,
			Type sourceType,
			object? destination,
			Type destinationType,
			MappingOptions?mappingOptions = null,
			CancellationToken cancellationToken = default) {

			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion


		#region IAsyncMapperFactory methods (deprecated)
		[Obsolete("IAsyncMapperFactory implementation will be removed in future versions, use the extension method by casting to IAsyncMapper instead: ((IAsyncMapper)mapper).MapAsyncNewFactory()")]
		public IAsyncNewMapFactory MapAsyncNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			throw new MapNotFoundException((sourceType, destinationType));
		}

		[Obsolete("IAsyncMapperFactory implementation will be removed in future versions, use the extension method by casting to IAsyncMapper instead: ((IAsyncMapper)mapper).MapAsyncMergeFactory()")]
		public IAsyncMergeMapFactory MapAsyncMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion
	}
}
