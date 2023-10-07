using NeatMapper.Common;
using System;
using System.Collections.Generic;

namespace NeatMapper {
	public static class MapperExtensions {
		/// <summary>
		/// Maps an object to a new one
		/// </summary>
		/// <typeparam name="TDestination">type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">
		/// object to map, may NOT be null as the source type will be retrieved from it,
		/// which will be used to retrieve the available maps
		/// </param>
		/// <returns>the newly created object, may be null</returns>
		public static
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			Map<TDestination>(this IMapper mapper, object source) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			return(TDestination)mapper.Map(source, source.GetType(), typeof(TDestination));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Maps an object to a new one
		/// </summary>
		/// <typeparam name="TSource">type of the object to map, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">object to map, may be null</param>
		/// <returns>the newly created object, may be null</returns>
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
			source) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return(TDestination)mapper.Map(source, typeof(TSource), typeof(TDestination));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Maps an object to an existing one and returns the result
		/// </summary>
		/// <typeparam name="TSource">type of the object to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="source">object to be mapped, may be null</param>
		/// <param name="destination">object to map to, may be null</param>
		/// <param name="mappingOptions">additional options for the current map, null to use default ones</param>
		/// <returns>
		/// the resulting object of the mapping, can be <paramref name="destination"/> or a new one,
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

		/// <summary>
		/// Maps a collection to an existing one by matching the elements and returns the result
		/// </summary>
		/// <typeparam name="TSourceElement">type of the elements to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestinationElement">type of the destination elements, used to retrieve the available maps</typeparam>
		/// <param name="source">collection to be mapped, may be null</param>
		/// <param name="destination">collection to map to, may be null</param>
		/// <param name="matcher">matching method to be used to match elements of the <paramref name="source"/> and <paramref name="destination"/> collections</param>
		/// <param name="removeNotMatchedDestinationElements">if true will remove all the elements from <paramref name="destination"/> which do not have a corresponding element in <paramref name="source"/>, null to use default setting</param>
		/// <returns>
		/// the resulting collection of the mapping, can be <paramref name="destination"/> or a new one,
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
			bool? removeNotMatchedDestinationElements = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));
			return mapper.Map(
				source,
				typeof(IEnumerable<TSourceElement>),
				destination,
				typeof(ICollection<TDestinationElement>),
				new MappingOptions {
					Matcher = (s, d, c) => (s is TSourceElement || object.Equals(s, default(TSourceElement))) &&
						(d is TDestinationElement || object.Equals(d, default(TDestinationElement))) &&
						matcher((TSourceElement)s, (TDestinationElement)d, c),
					CollectionRemoveNotMatchedDestinationElements = removeNotMatchedDestinationElements
				}) as ICollection<TDestinationElement>;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
