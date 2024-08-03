using System;
using System.Collections;
using System.Collections.Generic;

namespace NeatMapper.Transitive {
	public static class MapperExtensions {
		#region MapNewPreview
		#region Runtime
		/// <summary>
		/// Tries to retrieve a chain of type maps which could be used to map the given types. It does not guarantee
		/// that the actual maps will succeed.
		/// </summary>
		/// <param name="sourceType">Type of the source object, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object to create, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to match the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types.
		/// </param>
		/// <returns>
		/// A chain of type maps which could be used to map the given types or <see langword="null"></see>
		/// if the types cannot be mapped.
		/// The chain will always begin with the provided <paramref name="sourceType"/> and will end with
		/// the provided <paramref name="destinationType"/>, so it will always have at least 2 elements.
		/// </returns>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IList<Type>?
#else
			IList<Type>
#endif
			MapNewPreview(this IMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			try {
				using (var factory = mapper.MapNewFactory(sourceType, destinationType, mappingOptions)) {
					if(factory is ITransitiveNewMapFactory transFactory)
						return transFactory.Types;
					else
						return new[] { sourceType, destinationType };
				}
			}
			catch (MapNotFoundException) {
				return null;
			}
		}

		/// <inheritdoc cref="MapNewPreview(IMapper, Type, Type, MappingOptions)"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IList<Type>?
#else
			IList<Type>
#endif
			MapNewPreview(this IMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapNewPreview(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapNewPreview(IMapper, Type, Type, MappingOptions)"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IList<Type>?
#else
			IList<Type>
#endif
			MapNewPreview(this IMapper mapper,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.MapNewPreview(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="MapNewPreview(IMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="MapNewPreview(IMapper, Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="MapNewPreview(IMapper, Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="MapNewPreview(IMapper, Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A chain of type maps which could be used to map the given types or <see langword="null"></see>
		/// if the types cannot be mapped.
		/// The chain will always begin with the provided <typeparamref name="TSource"/> and will end with
		/// the provided <typeparamref name="TDestination"/>, so it will always have at least 2 elements.
		/// </returns>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IList<Type>?
#else
			IList<Type>
#endif
			MapNewPreview<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return mapper.MapNewPreview(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="MapNewPreview{TSource, TDestination}(IMapper, MappingOptions)"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IList<Type>?
#else
			IList<Type>
#endif
			MapNewPreview<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapNewPreview(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapNewPreview(IMapper, Type, Type, MappingOptions)"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IList<Type>?
#else
			IList<Type>
#endif
			MapNewPreview<TSource, TDestination>(this IMapper mapper,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.MapNewPreview(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region MapMergePreview
		#region Runtime
		/// <summary>
		/// Tries to retrieve a chain of type maps which could be used to map the given types. It does not guarantee
		/// that the actual maps will succeed.
		/// </summary>
		/// <param name="sourceType">Type of the source object, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object to create, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to match the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types.
		/// </param>
		/// <returns>
		/// A chain of type maps which could be used to map the given types or <see langword="null"></see>
		/// if the types cannot be mapped.
		/// The chain will always begin with the provided <paramref name="sourceType"/> and will end with
		/// the provided <paramref name="destinationType"/>, so it will always have at least 2 elements.
		/// </returns>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IList<Type>?
#else
			IList<Type>
#endif
			MapMergePreview(this IMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			try {
				using (var factory = mapper.MapMergeFactory(sourceType, destinationType, mappingOptions)) {
					if (factory is ITransitiveMergeMapFactory transFactory)
						return transFactory.Types;
					else
						return new[] { sourceType, destinationType };
				}
			}
			catch (MapNotFoundException) {
				return null;
			}
		}

		/// <inheritdoc cref="MapMergePreview(IMapper, Type, Type, MappingOptions)"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IList<Type>?
#else
			IList<Type>
#endif
			MapMergePreview(this IMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapMergePreview(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapMergePreview(IMapper, Type, Type, MappingOptions)"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IList<Type>?
#else
			IList<Type>
#endif
			MapMergePreview(this IMapper mapper,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.MapMergePreview(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="MapMergePreview(IMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="MapMergePreview(IMapper, Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="MapMergePreview(IMapper, Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="MapMergePreview(IMapper, Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A chain of type maps which could be used to map the given types or <see langword="null"></see>
		/// if the types cannot be mapped.
		/// The chain will always begin with the provided <typeparamref name="TSource"/> and will end with
		/// the provided <typeparamref name="TDestination"/>, so it will always have at least 2 elements.
		/// </returns>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IList<Type>?
#else
			IList<Type>
#endif
			MapMergePreview<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return mapper.MapMergePreview(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="MapMergePreview{TSource, TDestination}(IMapper, MappingOptions)"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IList<Type>?
#else
			IList<Type>
#endif
			MapMergePreview<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapMergePreview(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapMergePreview(IMapper, Type, Type, MappingOptions)"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IList<Type>?
#else
			IList<Type>
#endif
			MapMergePreview<TSource, TDestination>(this IMapper mapper,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.MapMergePreview(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion
	}
}
