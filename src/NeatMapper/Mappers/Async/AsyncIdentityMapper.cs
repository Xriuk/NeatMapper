using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IAsyncMapper"/> which returns the provided source element (for both new and merge maps).
	/// Supports only the same source/destination types. Can be used to merge collections of elements of the same type.
	/// </summary>
	public sealed class AsyncIdentityMapper : IAsyncMapper, IAsyncMapperFactory {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool CanMap(Type sourceType, Type destinationType) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return sourceType == destinationType;
		}


		/// <summary>
		/// Singleton instance of the mapper.
		/// </summary>
		public static readonly IAsyncMapper Instance = new AsyncIdentityMapper();


		private AsyncIdentityMapper() { }


		#region IAsyncMapper methods
		public bool CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMap(sourceType, destinationType);
		}

		public bool CanMapAsyncMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMap(sourceType, destinationType);
		}

		public Task<object?> MapAsync(
			object? source,
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (!CanMap(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			return Task.FromResult(source);
		}

		public Task<object?> MapAsync(
			object? source,
			Type sourceType,
			object? destination,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (!CanMap(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			return Task.FromResult(source);
		}
		#endregion

		#region IAsyncMapperFactory methods
		public IAsyncNewMapFactory MapAsyncNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMap(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultAsyncNewMapFactory(sourceType, destinationType, (source, cancellationToken) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));

				return Task.FromResult(source);
			});
		}

		public IAsyncMergeMapFactory MapAsyncMergeFactory(Type sourceType, Type destinationType, MappingOptions?mappingOptions = null) {
			if (!CanMap(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultAsyncMergeMapFactory(sourceType, destinationType, (source, destination, cancellationToken) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));
				TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

				return Task.FromResult(source);
			});
		}
		#endregion
	}
}
