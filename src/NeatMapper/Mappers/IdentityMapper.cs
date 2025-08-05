using System;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IMapper"/> which returns the provided source element (for both new and merge maps).
	/// Supports only the same source/destination types.
	/// </summary>
	public sealed class IdentityMapper : IMapper, IMapperFactory {
		/// <summary>
		/// Singleton instance of the mapper.
		/// </summary>
		public static readonly IMapper Instance = new IdentityMapper();


		private IdentityMapper() { }


		#region IMapper methods
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return sourceType == destinationType;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapNew(sourceType, destinationType);
		}

		public object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapNew(sourceType, destinationType) || sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			return source;
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapMerge(sourceType, destinationType) || sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			return source;
		}
		#endregion

		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapNew(sourceType, destinationType) || sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultNewMapFactory(sourceType, destinationType, source => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));

				return source;
			});
		}

		public IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapMerge(sourceType, destinationType) || sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultMergeMapFactory(sourceType, destinationType, (source, destination) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));
				TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

				return source;
			});
		}
		#endregion
	}
}
