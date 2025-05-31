using System;
using System.ComponentModel;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IMapper"/> which maps objects by using the corresponding <see cref="TypeConverter"/>
	/// (via <see cref="TypeDescriptor.GetConverter(Type)"/>).
	/// Supports only new maps.
	/// </summary>
	public sealed class TypeConverterMapper : IMapper, IMapperFactory {
		/// <summary>
		/// Singleton instance of the mapper.
		/// </summary>
		public static readonly IMapper Instance = new TypeConverterMapper();


		private TypeConverterMapper() { }


		#region IMapper methods
		public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if(sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				return false;

			return TypeDescriptor.GetConverter(sourceType).CanConvertTo(destinationType);
		}

		public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return false;
		}

		public object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			var converter = TypeDescriptor.GetConverter(sourceType);

			if(!converter.CanConvertTo(destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			object? result;
			try { 
				result = converter.ConvertTo(source, destinationType);
			}
			catch (Exception e) {
				throw new MappingException(e, (sourceType, destinationType));
			}

			// Should not happen
			TypeUtils.CheckObjectType(result, destinationType);

			return result;
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion

		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition)
				throw new MapNotFoundException((sourceType, destinationType));

			var converter = TypeDescriptor.GetConverter(sourceType);

			if (!converter.CanConvertTo(destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultNewMapFactory(
				sourceType, destinationType,
				source => {
					TypeUtils.CheckObjectType(source, sourceType, nameof(source));

					object? result;
					try {
						result = converter.ConvertTo(source, destinationType);
					}
					catch (Exception e) {
						throw new MappingException(e, (sourceType, destinationType));
					}

					// Should not happen
					TypeUtils.CheckObjectType(result, destinationType);

					return result;
				});
		}

		public IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion
	}
}
