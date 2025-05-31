using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IAsyncMapper"/> which returns the provided source element (for both new and merge maps).
	/// Supports only the same source/destination types. Can be used to merge collections of elements of the same type.
	/// </summary>
	[Obsolete("AsyncIdentityMapper will be made static in future versions, make sure you're using AsyncIdentityMapper.Instance")]
	public sealed class AsyncIdentityMapper : IAsyncMapper, IAsyncMapperFactory {
		/// <summary>
		/// Singleton instance of the mapper.
		/// </summary>
		public static readonly IAsyncMapper Instance = new AsyncIMapperWrapperMapper(IdentityMapper.Instance);


		private AsyncIdentityMapper() { }


		#region IAsyncMapper methods
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return sourceType == destinationType;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanMapAsyncMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapAsyncNew(sourceType, destinationType);
		}

		public Task<object?> MapAsync(
			object? source,
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (!CanMapAsyncNew(sourceType, destinationType) || sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
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

			if (!CanMapAsyncMerge(sourceType, destinationType) || sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			return Task.FromResult(source);
		}
		#endregion

		#region IAsyncMapperFactory methods
		public IAsyncNewMapFactory MapAsyncNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapAsyncNew(sourceType, destinationType) || sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultAsyncNewMapFactory(sourceType, destinationType, (source, cancellationToken) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));

				return Task.FromResult(source);
			});
		}

		public IAsyncMergeMapFactory MapAsyncMergeFactory(Type sourceType, Type destinationType, MappingOptions?mappingOptions = null) {
			if (!CanMapAsyncMerge(sourceType, destinationType) || sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
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
