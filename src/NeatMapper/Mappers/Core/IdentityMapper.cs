using System;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IMapper"/> which returns the provided source element (for both new and merge maps).
	/// Supports only the same source/destination types. Can be used to merge collections of elements of the same type.
	/// </summary>
	public sealed class IdentityMapper : IMapper, IMapperFactory {
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
		public static readonly IMapper Instance = new IdentityMapper();


		private IdentityMapper() { }


		#region IMapper methods
		public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMap(sourceType, destinationType);
		}

		public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMap(sourceType, destinationType);
		}

		public object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMap(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			return source;
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMap(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			return source;
		}
		#endregion

		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMap(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultNewMapFactory(sourceType, destinationType, source => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));

				return source;
			});
		}

		public IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMap(sourceType, destinationType))
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
