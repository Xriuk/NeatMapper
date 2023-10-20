using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Mappers {
	/// <summary>
	/// <see cref="IMapper"/> which delegates mapping to other <see cref="IMapper"/>s, this allows to combine different mapping capabilities.<br/>
	/// Each mapper is invoked in order and the first one to succeed in mapping is returned
	/// </summary>
	public sealed class CompositeMapper : IMapper {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		readonly IList<IMapper> _mappers;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif

		/// <summary>
		/// Creates the mapper by using the provided mappers list
		/// </summary>
		/// <param name="mappers">mappers to delegate the mapping to</param>
		public CompositeMapper(params IMapper[] mappers) : this((IList<IMapper>) mappers) { }

		/// <summary>
		/// Creates the mapper by using the provided mappers list
		/// </summary>
		/// <param name="mappers">mappers to delegate the mapping to</param>
		public CompositeMapper(IList<IMapper> mappers) {
			_mappers = mappers.ToList();
		}


		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
            object
#endif
			Map(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
            object
#endif
			source,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
            IEnumerable
#endif
			mappingOptions = null) {

			foreach (var mapper in _mappers) {
				try {
					return mapper.Map(source, sourceType, destinationType, mappingOptions);
				}
				catch (MapNotFoundException) { }
			}

			throw new MapNotFoundException((sourceType, destinationType));
		}

		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
            object
#endif
			Map(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
            object
#endif
			source,
			Type sourceType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
            object
#endif
			destination,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
            IEnumerable
#endif
			mappingOptions = null) {

			foreach (var mapper in _mappers) {
				try {
					return mapper.Map(source, sourceType, destination, destinationType, mappingOptions);
				}
				catch (MapNotFoundException) { }
			}

			throw new MapNotFoundException((sourceType, destinationType));
		}
	}
}
