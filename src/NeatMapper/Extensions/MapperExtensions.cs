﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace NeatMapper {
	public static class MapperExtensions {
		#region NewMap
		#region Runtime
		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			Map(this IMapper mapper,
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
			mappingOptions) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return mapper.Map(source, sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			Map(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return mapper.Map(source, sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region Explicit destination, inferred source
		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <param name="source">
		/// Object to map, CANNOT be null as the source type will be retrieved from it at runtime,
		/// which will be used to retrieve the available maps.
		/// </param>
		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>The newly created object, may be null.</returns>
		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/exception"/>
		public static
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			Map<TDestination>(this IMapper mapper,
			object source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (source == null)
				throw new ArgumentNullException(nameof(source), "Type cannot be inferred from null source, use an overload with an explicit source type");

			return (TDestination)mapper.Map(source, source.GetType(), typeof(TDestination), mappingOptions);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Map{TDestination}(IMapper, object, MappingOptions)"/>
		public static
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			Map<TDestination>(this IMapper mapper,
			object source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (source == null)
				throw new ArgumentNullException(nameof(source), "Type cannot be inferred from null source, use an overload with an explicit source type");
			return (TDestination)mapper.Map(source, source.GetType(), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Map{TDestination}(IMapper, object, MappingOptions)"/>
		public static
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			Map<TDestination>(this IMapper mapper,
			object source,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (source == null)
				throw new ArgumentNullException(nameof(source), "Type cannot be inferred from null source, use an overload with an explicit source type");
			return (TDestination)mapper.Map(source, source.GetType(), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <inheritdoc cref="Map{TDestination}(IMapper, object, MappingOptions)" path="/typeparam[@name='TDestination']"/>
		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/param[@name='source']"/>
		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>The newly created object, may be null.</returns>
		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/exception"/>
		public static
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
#pragma warning disable CS1712
			Map<TSource, TDestination>(this IMapper mapper,
#pragma warning restore CS1712
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return (TDestination)mapper.Map(source, typeof(TSource), typeof(TDestination), mappingOptions);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Map{TSource, TDestination}(IMapper, TSource, MappingOptions)" />
		public static
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			Map<TSource, TDestination>(this IMapper mapper,
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return (TDestination)mapper.Map(source, typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Map{TSource, TDestination}(IMapper, TSource, MappingOptions)" />
		public static
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			Map<TSource, TDestination>(this IMapper mapper,
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return (TDestination)mapper.Map(source, typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion
		#endregion

		#region MergeMap
		#region Runtime
		/// <inheritdoc cref="IMapper.Map(object, Type, object, Type, MappingOptions)"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			Map(this IMapper mapper,
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
			mappingOptions) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return mapper.Map(source, sourceType, destination, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="IMapper.Map(object, Type, object, Type, MappingOptions)"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			Map(this IMapper mapper,
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
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return mapper.Map(source, sourceType, destination, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IMapper.Map(object, Type, object, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapper.Map(object, Type, object, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapper.Map(object, Type, object, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapper.Map(object, Type, object, Type, MappingOptions)" path="/param[@name='source']"/>
		/// <inheritdoc cref="IMapper.Map(object, Type, object, Type, MappingOptions)" path="/param[@name='destination']"/>
		/// <inheritdoc cref="IMapper.Map(object, Type, object, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// The resulting object of the mapping, can be <paramref name="destination"/> or a new one,
		/// may be null.
		/// </returns>
		/// <inheritdoc cref="IMapper.Map(object, Type, object, Type, MappingOptions)" path="/exception"/>
		public static
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			Map<TSource, TDestination>(this IMapper mapper,
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			destination,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return (TDestination)mapper.Map(source, typeof(TSource), destination, typeof(TDestination), mappingOptions);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Map{TSource, TDestination}(IMapper, TSource, TDestination, MappingOptions)"/>
		public static
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			Map<TSource, TDestination>(this IMapper mapper,
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			destination,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return (TDestination)mapper.Map(source, typeof(TSource), destination, typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		// DEV: cannot have a "params object[] mappingOptions" overload because causes ambiguity with Runtime overloads
		// (both NewMap and MergeMap with "IEnumerable mappingOptions") when types are not specified (which is a farly-widely used case)
		#endregion

		#region Collection
		/// <summary>
		/// Maps a collection to an existing one by matching the elements and returns the result.
		/// </summary>
		/// <typeparam name="TSourceElement">
		/// Type of the elements to be mapped, used to retrieve the available maps.
		/// </typeparam>
		/// <typeparam name="TDestinationElement">
		/// Type of the destination elements, used to retrieve the available maps.
		/// </typeparam>
		/// <param name="source">Collection to be mapped, may be null.</param>
		/// <param name="destination">Collection to map to, may be null, CANNOT be readonly.</param>
		/// <param name="matcher">
		/// Matching method to be used to match elements of the <paramref name="source"/>
		/// and <paramref name="destination"/> collections.
		/// </param>
		/// <inheritdoc cref="IMapper.Map(object, Type, object, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// The resulting collection of the mapping, can be <paramref name="destination"/> or a new one,
		/// may be null.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be mapped.</exception>
		/// <exception cref="MappingException">
		/// An exception was thrown while mapping the types, check the inner exception for details.
		/// </exception>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TDestinationElement>?
#else
			ICollection<TDestinationElement>
#endif
			Map<TSourceElement, TDestinationElement>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable<TSourceElement>?
#else
			IEnumerable<TSourceElement>
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TDestinationElement>?
#else
			ICollection<TDestinationElement>
#endif
			destination,
			MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));

			mappingOptions = (mappingOptions ?? MappingOptions.Empty).ReplaceOrAdd<MergeCollectionsMappingOptions>(m => new MergeCollectionsMappingOptions(
				m?.RemoveNotMatchedDestinationElements,
				(s, d, c) => {
					if((!(s is TSourceElement) && !object.Equals(s, default(TSourceElement))) ||
						(!(d is TDestinationElement) && !object.Equals(d, default(TDestinationElement)))) {

						throw new MapNotFoundException((typeof(TSourceElement), typeof(TDestinationElement)));
					}

					return matcher((TSourceElement)s, (TDestinationElement)d, c);
			}));

			return mapper.Map<IEnumerable<TSourceElement>, ICollection<TDestinationElement>>(source, destination, mappingOptions);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TDestinationElement>?
#else
			ICollection<TDestinationElement>
#endif
			Map<TSourceElement, TDestinationElement>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable<TSourceElement>?
#else
			IEnumerable<TSourceElement>
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TDestinationElement>?
#else
			ICollection<TDestinationElement>
#endif
			destination,
			MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.Map<TSourceElement, TDestinationElement>(source, destination, matcher, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TDestinationElement>?
#else
			ICollection<TDestinationElement>
#endif
			Map<TSourceElement, TDestinationElement>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable<TSourceElement>?
#else
			IEnumerable<TSourceElement>
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TDestinationElement>?
#else
			ICollection<TDestinationElement>
#endif
			destination,
			MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.Map<TSourceElement, TDestinationElement>(source, destination, matcher, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion


		#region CanMapNew
		#region Runtime
		/// <summary>
		/// Checks if the mapper could create a new object from a given one, will check if the given mapper supports
		/// <see cref="IMapperCanMap"/> first otherwise will create a dummy source object (cached) and try to map it.
		/// It does not guarantee that the actual map will succeed.
		/// </summary>
		/// <inheritdoc cref="IMapperCanMap.CanMapNew(Type, Type, MappingOptions)"/>
		public static bool CanMapNew(this IMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Check if the mapper implements IMapperCanMap, if it throws it means that the map can be checked only when mapping
			if (mapper is IMapperCanMap mapperCanMap)
				return mapperCanMap.CanMapNew(sourceType, destinationType, mappingOptions);

			// Try creating a default source object and try mapping it
			object source;
			try {
				source = ObjectFactory.GetOrCreateCached(sourceType) ?? throw new Exception(); // Just in case
			}
			catch {
				throw new InvalidOperationException(
					"Cannot verify if the mapper supports the given map because unable to create a dummy object to test it.");
			}

			try {
				mapper.Map(source, sourceType, destinationType, mappingOptions);
				return true;
			}
			catch (MapNotFoundException) {
				return false;
			}
			catch (Exception e){
				throw new InvalidOperationException(
					"Cannot verify if the mapper supports the given map because it threw an exception while trying to map a dummy object. " +
					"Check inner exception for details.", e);
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="CanMapNew(IMapper, Type, Type, MappingOptions)"/>
		public static bool CanMapNew(this IMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.CanMapNew(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="CanMapNew(IMapper, Type, Type, MappingOptions)"/>
		public static bool CanMapNew(this IMapper mapper,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.CanMapNew(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="CanMapNew(IMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapperCanMap.CanMapNew(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapperCanMap.CanMapNew(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapperCanMap.CanMapNew(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be created
		/// from a parameter of type <typeparamref name="TSource"/>.
		/// </returns>
		/// <inheritdoc cref="IMapperCanMap.CanMapNew(Type, Type, MappingOptions)" path="/exception"/>
		public static bool CanMapNew<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return mapper.CanMapNew(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="CanMapNew(IMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapperCanMap.CanMapNew(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapperCanMap.CanMapNew(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapperCanMap.CanMapNew(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be created
		/// from a parameter of type <typeparamref name="TSource"/>.
		/// </returns>
		/// <inheritdoc cref="IMapperCanMap.CanMapNew(Type, Type, MappingOptions)" path="/exception"/>
		public static bool CanMapNew<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.CanMapNew(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="CanMapNew(IMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapperCanMap.CanMapNew(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapperCanMap.CanMapNew(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapperCanMap.CanMapNew(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be created
		/// from a parameter of type <typeparamref name="TSource"/>.
		/// </returns>
		/// <inheritdoc cref="IMapperCanMap.CanMapNew(Type, Type, MappingOptions)" path="/exception"/>
		public static bool CanMapNew<TSource, TDestination>(this IMapper mapper,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.CanMapNew(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region CanMapMerge
		#region Runtime
		/// <summary>
		/// Checks if the mapper could merge an object into an existing one, will check if the given mapper supports
		/// <see cref="IMapperCanMap"/> first otherwise will create a dummy source (cached)
		/// and destination (not cached) objects and try to map them. It does not guarantee that the actual map will succeed.
		/// </summary>
		/// <inheritdoc cref="IMapperCanMap.CanMapMerge(Type, Type, MappingOptions)"/>
		public static bool CanMapMerge(this IMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Check if the mapper implements IMapperCanMap, if it throws it means that the map can be checked only when mapping
			if (mapper is IMapperCanMap mapperCanMap)
				return mapperCanMap.CanMapMerge(sourceType, destinationType, mappingOptions);

			// Try creating two default source and destination objects and try mapping them,
			// cannot create a cached destination because it could be modified by the map so we could not reuse it
			object source;
			object destination;
			try {
				source = ObjectFactory.GetOrCreateCached(sourceType) ?? throw new Exception(); // Just in case
				destination = ObjectFactory.Create(destinationType) ?? throw new Exception(); // Just in case
			}
			catch {
				throw new InvalidOperationException(
					"Cannot verify if the mapper supports the given map because unable to create dummy objects to test it.");
			}

			try {
				mapper.Map(source, sourceType, destination, destinationType, mappingOptions);
				return true;
			}
			catch (MapNotFoundException) {
				return false;
			}
			catch (Exception e) {
				throw new InvalidOperationException(
					"Cannot verify if the mapper supports the given map because it threw an exception while trying to map dummy objects. " +
					"Check inner exception for details.", e);
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="CanMapMerge(IMapper, Type, Type, MappingOptions)"/>
		public static bool CanMapMerge(this IMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.CanMapMerge(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="CanMapMerge(IMapper, Type, Type, MappingOptions)"/>
		public static bool CanMapMerge(this IMapper mapper,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.CanMapMerge(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="CanMapMerge(IMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapperCanMap.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapperCanMap.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapperCanMap.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be merged into an object
		/// of type <typeparamref name="TDestination"/>.
		/// </returns>
		/// <inheritdoc cref="IMapperCanMap.CanMapMerge(Type, Type, MappingOptions)" path="/exception"/>
		public static bool CanMapMerge<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return mapper.CanMapMerge(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="CanMapMerge(IMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapperCanMap.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapperCanMap.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapperCanMap.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be merged into an object
		/// of type <typeparamref name="TDestination"/>.
		/// </returns>
		/// <inheritdoc cref="IMapperCanMap.CanMapMerge(Type, Type, MappingOptions)" path="/exception"/>
		public static bool CanMapMerge<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.CanMapMerge(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="CanMapMerge(IMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapperCanMap.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapperCanMap.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapperCanMap.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be merged into an object
		/// of type <typeparamref name="TDestination"/>.
		/// </returns>
		/// <inheritdoc cref="IMapperCanMap.CanMapMerge(Type, Type, MappingOptions)" path="/exception"/>
		public static bool CanMapMerge<TSource, TDestination>(this IMapper mapper,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.CanMapMerge(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion


		#region MapNewFactory
		#region Runtime
		/// <summary>
		/// Creates a factory which can be used to map objects to new ones, will check if the given mapper supports
		/// <see cref="IMapperFactory"/> first otherwise will return
		/// <see cref="IMapper.Map(object, Type, Type, MappingOptions)"/> wrapped in a delegate.
		/// </summary>
		/// <remarks>It is NOT guaranteed that the created factory shares the same <see cref="MappingContext"/>.</remarks>
		/// <inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions)"/>
		public static INewMapFactory MapNewFactory(this IMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Check if the mapper implements IMapperFactory
			if (mapper is IMapperFactory mapperFactory)
				return mapperFactory.MapNewFactory(sourceType, destinationType, mappingOptions);

			// Check if the mapper can map the types (we don't do it via the extension method above because
			// it may require actually mapping the two types if the interface is not implemented,
			// and as the returned factory may still throw MapNotFoundException we are still compliant)
			if (mapper is IMapperCanMap mapperCanMap) {
				try {
					if(!mapperCanMap.CanMapNew(sourceType, destinationType, mappingOptions))
						throw new MapNotFoundException((sourceType, destinationType));
				}
				catch (MapNotFoundException) {
					throw;
				}
				catch { }
			}

			// Return the map wrapped
			return new DefaultNewMapFactory(
				sourceType, destinationType,
				source => mapper.Map(source, sourceType, destinationType, mappingOptions));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapNewFactory(IMapper, Type, Type, MappingOptions)"/>
		public static INewMapFactory MapNewFactory(this IMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapNewFactory(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapNewFactory(IMapper, Type, Type, MappingOptions)"/>
		public static INewMapFactory MapNewFactory(this IMapper mapper,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.MapNewFactory(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="MapNewFactory(IMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <inheritdoc cref="MapNewFactory(IMapper, Type, Type, MappingOptions)" path="/remarks"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A factory which can be used to map objects of type <typeparamref name="TSource"/> into new objects
		/// of type <typeparamref name="TDestination"/>.<br/>
		/// The factory when invoked may throw <see cref="MapNotFoundException"/> or <see cref="MappingException"/> exceptions.
		/// </returns>
		/// <inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions)" path="/exception"/>
		public static NewMapFactory<TSource, TDestination> MapNewFactory<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			var factory = mapper.MapNewFactory(typeof(TSource), typeof(TDestination), mappingOptions);
			return new DisposableNewMapFactory<TSource, TDestination>(source => (TDestination)factory.Invoke(source), factory);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapNewFactory(IMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <inheritdoc cref="MapNewFactory(IMapper, Type, Type, MappingOptions)" path="/remarks"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A factory which can be used to map objects of type <typeparamref name="TSource"/> into new objects
		/// of type <typeparamref name="TDestination"/>.<br/>
		/// The factory when invoked may throw <see cref="MapNotFoundException"/> or <see cref="MappingException"/> exceptions.
		/// </returns>
		/// <inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions)" path="/exception"/>
		public static NewMapFactory<TSource, TDestination> MapNewFactory<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapNewFactory<TSource, TDestination>(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapNewFactory(IMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <inheritdoc cref="MapNewFactory(IMapper, Type, Type, MappingOptions)" path="/remarks"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A factory which can be used to map objects of type <typeparamref name="TSource"/> into new objects
		/// of type <typeparamref name="TDestination"/>.<br/>
		/// The factory when invoked may throw <see cref="MapNotFoundException"/> or <see cref="MappingException"/> exceptions.
		/// </returns>
		/// <inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions)" path="/exception"/>
		public static NewMapFactory<TSource, TDestination> MapNewFactory<TSource, TDestination>(this IMapper mapper,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.MapNewFactory<TSource, TDestination>(mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region MapMergeFactory
		#region Runtime
		/// <summary>
		/// Creates a factory which can be used to map objects to existing ones, will check if the given mapper supports
		/// <see cref="IMapperFactory"/> first otherwise will return
		/// <see cref="IMapper.Map(object, Type, object, Type, MappingOptions)"/> wrapped in a delegate.
		/// </summary>
		/// <remarks>It is NOT guaranteed that the created factory shares the same <see cref="MappingContext"/>.</remarks>
		/// <inheritdoc cref="IMapperFactory.MapMergeFactory(Type, Type, MappingOptions)"/>
		public static IMergeMapFactory MapMergeFactory(this IMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Check if the mapper implements IMapperFactory
			if (mapper is IMapperFactory mapperFactory)
				return mapperFactory.MapMergeFactory(sourceType, destinationType, mappingOptions);

			// Check if the mapper can map the types (we don't do it via the extension method above because
			// it may require actually mapping the two types if the interface is not implemented,
			// and as the returned factory may still throw MapNotFoundException we are still compliant)
			if (mapper is IMapperCanMap mapperCanMap) {
				try {
					if (!mapperCanMap.CanMapMerge(sourceType, destinationType, mappingOptions))
						throw new MapNotFoundException((sourceType, destinationType));
				}
				catch (MapNotFoundException) {
					throw;
				}
				catch { }
			}

			// Return the map wrapped
			return new DefaultMergeMapFactory(
				sourceType, destinationType,
				(source, destination) => mapper.Map(source, sourceType, destination, destinationType, mappingOptions));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapMergeFactory(IMapper, Type, Type, MappingOptions)"/>
		public static IMergeMapFactory MapMergeFactory(this IMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapMergeFactory(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapMergeFactory(IMapper, Type, Type, MappingOptions)"/>
		public static IMergeMapFactory MapMergeFactory(this IMapper mapper,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.MapMergeFactory(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="MapMergeFactory(IMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <inheritdoc cref="MapMergeFactory(IMapper, Type, Type, MappingOptions)" path="/remarks"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapperFactory.MapMergeFactory(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapperFactory.MapMergeFactory(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapperFactory.MapMergeFactory(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A factory which can be used to map objects of type <typeparamref name="TSource"/> into existing objects
		/// of type <typeparamref name="TDestination"/>.<br/>
		/// The factory when invoked may throw <see cref="MapNotFoundException"/> or <see cref="MappingException"/> exceptions.
		/// </returns>
		/// <inheritdoc cref="IMapperFactory.MapMergeFactory(Type, Type, MappingOptions)" path="/exception"/>
		public static MergeMapFactory<TSource, TDestination> MapMergeFactory<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			var factory = mapper.MapMergeFactory(typeof(TSource), typeof(TDestination), mappingOptions);
			return new DisposableMergeMapFactory<TSource, TDestination>((source, destination) => (TDestination)factory.Invoke(source, destination), factory);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapMergeFactory{TSource, TDestination}(IMapper, MappingOptions)"/>
		public static MergeMapFactory<TSource, TDestination> MapMergeFactory<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapMergeFactory<TSource, TDestination>(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapMergeFactory{TSource, TDestination}(IMapper, MappingOptions)"/>
		public static MergeMapFactory<TSource, TDestination> MapMergeFactory<TSource, TDestination>(this IMapper mapper,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.MapMergeFactory<TSource, TDestination>(mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Collection
		/// <summary>
		/// Creates a factory which can be used to map collections to existing ones by matching the elements,
		/// will check if the given mapper supports <see cref="IMapperFactory"/> first otherwise will return
		/// <see cref="IMapper.Map(object, Type, object, Type, MappingOptions)"/> wrapped in a delegate.
		/// </summary>
		/// <inheritdoc cref="MapMergeFactory(IMapper, Type, Type, MappingOptions)" path="/remarks"/>
		/// <typeparam name="TSourceElement">
		/// Type of the elements to be mapped, used to retrieve the available maps.
		/// </typeparam>
		/// <typeparam name="TDestinationElement">
		/// Type of the destination elements, used to retrieve the available maps.
		/// </typeparam>
		/// <param name="matcher">
		/// Matching method to be used to match elements of the source and destination collections.
		/// </param>
		/// <inheritdoc cref="IMapperFactory.MapMergeFactory(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A factory which can be used to map collections of type <typeparamref name="TSourceElement"/> into existing
		/// collections of type <typeparamref name="TDestinationElement"/>.<br/>
		/// The factory when invoked may throw <see cref="MapNotFoundException"/> or <see cref="MappingException"/> exceptions.
		/// </returns>
		/// <inheritdoc cref="IMapperFactory.MapMergeFactory(Type, Type, MappingOptions)" path="/exception"/>
		public static MergeMapFactory<
			IEnumerable<TSourceElement>,
			ICollection<TDestinationElement>>
				MapMergeFactory<TSourceElement, TDestinationElement>(this IMapper mapper,
			MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));

			mappingOptions = (mappingOptions ?? MappingOptions.Empty).ReplaceOrAdd<MergeCollectionsMappingOptions>(m => new MergeCollectionsMappingOptions(
				m?.RemoveNotMatchedDestinationElements,
				(s, d, c) => {
					if ((!(s is TSourceElement) && !object.Equals(s, default(TSourceElement))) ||
						(!(d is TDestinationElement) && !object.Equals(d, default(TDestinationElement)))) {

						throw new MapNotFoundException((typeof(TSourceElement), typeof(TDestinationElement)));
					}

					return matcher((TSourceElement)s, (TDestinationElement)d, c);
				}));

			return mapper.MapMergeFactory<IEnumerable<TSourceElement>, ICollection<TDestinationElement>>(mappingOptions);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}


		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)"/>
		public static MergeMapFactory<
			IEnumerable<TSourceElement>,
			ICollection<TDestinationElement>>
				MapMergeFactory<TSourceElement, TDestinationElement>(this IMapper mapper,
			MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapMergeFactory<TSourceElement, TDestinationElement>(matcher, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)"/>
		public static MergeMapFactory<
			IEnumerable<TSourceElement>,
			ICollection<TDestinationElement>>
				MapMergeFactory<TSourceElement, TDestinationElement>(this IMapper mapper,
			MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.MapMergeFactory<TSourceElement, TDestinationElement>(matcher, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion
	}
}
