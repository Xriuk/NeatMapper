using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	public static class MapperExtensions {
		/// <summary>
		/// Maps an object to a new one
		/// </summary>
		/// <typeparam name="TDestination">Type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">
		/// Object to map, may NOT be null as the source type will be retrieved from it,
		/// which will be used to retrieve the available maps
		/// </param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps</param>
		/// <returns>The newly created object, may be null</returns>
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
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (source == null)
				throw new ArgumentNullException(nameof(source), "Type cannot be inferred from null source, use an overload with an explicit source type");
			return(TDestination)mapper.Map(source, source.GetType(), typeof(TDestination), mappingOptions);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Maps an object to a new one
		/// </summary>
		/// <typeparam name="TDestination">Type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">
		/// Object to map, may NOT be null as the source type will be retrieved from it,
		/// which will be used to retrieve the available maps
		/// </param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps</param>
		/// <returns>The newly created object, may be null</returns>
		public static
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			Map<TDestination>(this IMapper mapper,
			object source,
			params object[] mappingOptions) {

			return mapper.Map<TDestination>(source, mappingOptions.Length > 0 ? (IEnumerable)mappingOptions : null);
		}

		/// <summary>
		/// Maps an object to a new one
		/// </summary>
		/// <typeparam name="TSource">Type of the object to map, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">Object to map, may be null</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps</param>
		/// <returns>The newly created object, may be null</returns>
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
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return(TDestination)mapper.Map(source, typeof(TSource), typeof(TDestination), mappingOptions);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Maps an object to a new one
		/// </summary>
		/// <typeparam name="TSource">Type of the object to map, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">Object to map, may be null</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps</param>
		/// <returns>The newly created object, may be null</returns>
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
			params object[] mappingOptions) {

			return mapper.Map<TSource, TDestination>(source, mappingOptions.Length > 0 ? (IEnumerable)mappingOptions : null);
		}

		/// <summary>
		/// Maps an object to an existing one and returns the result
		/// </summary>
		/// <typeparam name="TSource">Type of the object to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="source">Object to be mapped, may be null</param>
		/// <param name="destination">Object to map to, may be null</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps</param>
		/// <returns>
		/// The resulting object of the mapping, can be <paramref name="destination"/> or a new one,
		/// may be null
		/// </returns>
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

		/// <summary>
		/// Maps an object to an existing one and returns the result
		/// </summary>
		/// <typeparam name="TSource">Type of the object to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="source">Object to be mapped, may be null</param>
		/// <param name="destination">Object to map to, may be null</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps</param>
		/// <returns>
		/// The resulting object of the mapping, can be <paramref name="destination"/> or a new one,
		/// may be null
		/// </returns>
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
			params object[] mappingOptions) {

			return mapper.Map<TSource, TDestination>(source, destination, mappingOptions.Length > 0 ? (IEnumerable)mappingOptions : null);
		}

		/// <summary>
		/// Maps a collection to an existing one by matching the elements and returns the result
		/// </summary>
		/// <typeparam name="TSourceElement">Type of the elements to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestinationElement">Type of the destination elements, used to retrieve the available maps</typeparam>
		/// <param name="source">Collection to be mapped, may be null</param>
		/// <param name="destination">Collection to map to, may be null</param>
		/// <param name="matcher">Matching method to be used to match elements of the <paramref name="source"/> and <paramref name="destination"/> collections</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps</param>
		/// <returns>
		/// The resulting collection of the mapping, can be <paramref name="destination"/> or a new one,
		/// may be null
		/// </returns>
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
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));

			var mergeMappingOptions = mappingOptions?.Cast<object>().OfType<MergeCollectionsMappingOptions>().FirstOrDefault();
			if(mergeMappingOptions == null) {
				mergeMappingOptions = new MergeCollectionsMappingOptions();
				mappingOptions = mappingOptions != null ? mappingOptions.Cast<object>().Concat(new object[] { mergeMappingOptions }) : new object[] { mergeMappingOptions };
			}
			mergeMappingOptions.Matcher = (s, d, c) => (s is TSourceElement || object.Equals(s, default(TSourceElement))) &&
				(d is TDestinationElement || object.Equals(d, default(TDestinationElement))) &&
				matcher((TSourceElement)s, (TDestinationElement)d, c);

			return mapper.Map(
				source,
				typeof(IEnumerable<TSourceElement>),
				destination,
				typeof(ICollection<TDestinationElement>),
				mappingOptions) as ICollection<TDestinationElement>;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Maps a collection to an existing one by matching the elements and returns the result
		/// </summary>
		/// <typeparam name="TSourceElement">Type of the elements to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestinationElement">Type of the destination elements, used to retrieve the available maps</typeparam>
		/// <param name="source">Collection to be mapped, may be null</param>
		/// <param name="destination">Collection to map to, may be null</param>
		/// <param name="matcher">Matching method to be used to match elements of the <paramref name="source"/> and <paramref name="destination"/> collections</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps</param>
		/// <returns>
		/// The resulting collection of the mapping, can be <paramref name="destination"/> or a new one,
		/// may be null
		/// </returns>
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
			params object[] mappingOptions) {

			return mapper.Map<TSourceElement, TDestinationElement>(source, destination, matcher, mappingOptions.Length > 0 ? (IEnumerable)mappingOptions : null);
		}


		/// <summary>
		/// Checks if the mapper can create a new object from a given one, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source object and try to map it
		/// </summary>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object to create, used to retrieve the available maps</param>
		/// <returns>True if an object of <paramref name="destinationType"/> can be created from a parameter of <paramref name="sourceType"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static bool CanMapNew(this IMapper mapper, Type sourceType, Type destinationType) {

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
				return mapperCanMap.CanMapNew(sourceType, destinationType);

			// Try creating a default source object and try mapping it
			object source;
			try {
				source = ObjectFactory.Create(sourceType) ?? throw new Exception(); // Just in case
			}
			catch {
				throw new InvalidOperationException("Cannot verify if the mapper supports the given map because unable to create an object to test it");
			}

			try {
				mapper.Map(source, sourceType, destinationType);
				return true;
			}
			catch (MapNotFoundException) {
				return false;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Checks if the mapper can create a new object from a given one, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source object and try to map it
		/// </summary>
		/// <typeparam name="TSource">Type of the object to map, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <returns>True if an object of <typeparamref name="TDestination"/> can be created from a parameter of <typeparamref name="TSource"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static bool CanMapNew<TSource, TDestination>(this IMapper mapper) {
			return mapper.CanMapNew(typeof(TSource), typeof(TDestination));
		}

		/// <summary>
		/// Checks if the mapper can merge an object into an existing one, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source and destination objects and try to map them
		/// </summary>
		/// <param name="sourceType">Type of the object to be mapped, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <returns>True if an object of <paramref name="sourceType"/> can be merged into an object of <paramref name="destinationType"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static bool CanMapMerge(this IMapper mapper, Type sourceType, Type destinationType) {

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
				return mapperCanMap.CanMapMerge(sourceType, destinationType);

			// Try creating two default source and destination objects and try mapping them
			object source;
			object destination;
			try {
				source = ObjectFactory.Create(sourceType) ?? throw new Exception(); // Just in case
				destination = ObjectFactory.Create(destinationType) ?? throw new Exception(); // Just in case
			}
			catch {
				throw new InvalidOperationException("Cannot verify if the mapper supports the given map because unable to create the objects to test it");
			}

			try {
				mapper.Map(source, sourceType, destination, destinationType);
				return true;
			}
			catch (MapNotFoundException) {
				return false;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Checks if the mapper can merge an object into an existing one, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source and destination objects and try to map them
		/// </summary>
		/// <typeparam name="TSource">Type of the object to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available maps</typeparam>
		/// <returns>True if an object of <typeparamref name="TSource"/> can be merged into an object of <typeparamref name="TDestination"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static bool CanMapMerge<TSource, TDestination>(this IMapper mapper) {
			return mapper.CanMapMerge(typeof(TSource), typeof(TDestination));
		}
	}
}
