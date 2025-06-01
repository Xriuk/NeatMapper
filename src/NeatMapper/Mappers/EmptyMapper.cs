using System;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IMapper"/> which cannot map any type.
	/// </summary>
	public sealed class EmptyMapper : IMapper {
		/// <summary>
		/// Singleton instance of the mapper.
		/// </summary>
		public static readonly IMapper Instance = new EmptyMapper();


		private EmptyMapper() { }


		#region IMapper methods
		public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return false;
		}

		public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return false;
		}

		public object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			throw new MapNotFoundException((sourceType, destinationType));
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion
	}
}
