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
		public static TDestination? Map<TDestination>(this IMapper mapper, object source) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			return (TDestination)mapper.Map(source, source.GetType(), typeof(TDestination))!;
		}

		/// <summary>
		/// Maps an object to a new one
		/// </summary>
		/// <typeparam name="TSource">type of the object to map, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">object to map, may be null</param>
		/// <returns>the newly created object, may be null</returns>
		public static TDestination? Map<TSource, TDestination>(this IMapper mapper, TSource? source) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return (TDestination)mapper.Map(source, typeof(TSource), typeof(TDestination))!;
		}

		/// <summary>
		/// Maps an object to an existing one and returns the result
		/// </summary>
		/// <typeparam name="TSource">type of the object to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="source">object to be mapped, may be null</param>
		/// <param name="destination">object to map to, may be null</param>
		/// <param name="mappingOptions">additional options for the current map</param>
		/// <returns>
		/// the resulting object of the mapping, can be <paramref name="destination"/> or a new one,
		/// may be null
		/// </returns>
		public static TDestination? Map<TSource, TDestination>(this IMapper mapper, TSource? source, TDestination? destination, MappingOptions? mappingOptions = null) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return (TDestination)mapper.Map(source, typeof(TSource), destination, typeof(TDestination), mappingOptions)!;
		}

		/// <summary>
		/// Maps a collection to an existing one by matching the elements and returns the result
		/// </summary>
		/// <typeparam name="TSourceElement">type of the elements to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestinationElement">type of the destination elements, used to retrieve the available maps</typeparam>
		/// <param name="source">collection to be mapped, may be null</param>
		/// <param name="destination">collection to map to, may be null</param>
		/// <returns>
		/// the resulting collection of the mapping, can be <paramref name="destination"/> or a new one,
		/// may be null
		/// </returns>
		public static ICollection<TDestinationElement>? Map<TSourceElement, TDestinationElement>(this IMapper mapper, IEnumerable<TSourceElement>? source, ICollection<TDestinationElement>? destination, Func<TSourceElement?, TDestinationElement?, MatchingContext, bool> matcher) {
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
						matcher((TSourceElement)s!, (TDestinationElement)d!, c)
				}) as ICollection<TDestinationElement>;
		}
	}
}
