using System;
using System.Collections.Concurrent;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IMapper"/> which maps objects implementing <see cref="IConvertible"/>
	/// (via <see cref="Convert.ChangeType(object, Type)"/>).
	/// Supports only new maps.
	/// </summary>
	public sealed class ConvertibleMapper : IMapper, IMapperFactory {
		private static readonly ConcurrentDictionary<(Type From, Type To), bool> _canConvertTypesCache =
			new ConcurrentDictionary<(Type From, Type To), bool>();

		/// <summary>
		/// Singleton instance of the mapper.
		/// </summary>
		public static readonly IMapper Instance = new ConvertibleMapper();


		private ConvertibleMapper() { }


		#region IMapper methods
		public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if(sourceType != typeof(IConvertible) && !sourceType.GetInterfaces().Contains(typeof(IConvertible)))
				return false;

			// We throw an exception if we cannot verify the mapping (in case of MappingExceptions),
			// so that we don't cache the result but still return false
			try { 
				return _canConvertTypesCache.GetOrAdd((sourceType, destinationType), types => {
					if(!ObjectFactory.CanCreate(types.From))
						return false;

					object source;
					try {
						source = ObjectFactory.Create(types.From);
					}
					catch (ObjectCreationException) {
						return false;
					}

					try {
						Map(source, sourceType, destinationType);
						return true;
					}
					catch (MapNotFoundException) {
						return false;
					}
				});
			}
			catch {
				return false;
			}
		}

		public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return false;
		}

		public object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if(sourceType != typeof(IConvertible) && !sourceType.GetInterfaces().Contains(typeof(IConvertible)))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			if (source == null) {
				if (destinationType.IsValueType) 
					throw new MappingException(new InvalidCastException("Cannot cast null to value type"), (sourceType, destinationType));
				else
					return null;
			}

			object? result;
			try {
				result = Convert.ChangeType(source, destinationType);
			}
			catch (InvalidCastException) {
				throw new MapNotFoundException((sourceType, destinationType));
			}
			catch(Exception e) {
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
			if(!CanMapNew(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultNewMapFactory(
				sourceType, destinationType,
				source => {
					TypeUtils.CheckObjectType(source, sourceType, nameof(source));

					object? result;
					try {
						result = Convert.ChangeType(source, destinationType);
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
