﻿using NeatMapper.Async.Internal;

namespace NeatMapper.Async {
	public static class AsyncMapperExtensions {
		/// <summary>
		/// Maps an object to a new one asynchronously
		/// </summary>
		/// <typeparam name="TDestination">type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">
		/// object to map, may NOT be null as the source type will be retrieved from it,
		/// which will be used to retrieve the available maps
		/// </param>
		/// <param name="cancellationToken">cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>a task which when completed returns the newly created object, which may be null</returns>
		public static Task<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> MapAsync<TDestination>(this IAsyncMapper mapper, object source, CancellationToken cancellationToken = default) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, source.GetType(), typeof(TDestination), cancellationToken));
		}

		/// <summary>
		/// Maps an object to a new one asynchronously
		/// </summary>
		/// <typeparam name="TSource">type of the object to map, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">object to map, may be null</param>
		/// <param name="cancellationToken">cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>a task which when completed returns the newly created object, which may be null</returns>
		public static Task<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> MapAsync<TSource, TDestination>(this IAsyncMapper mapper,
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			source,
			CancellationToken cancellationToken = default) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, typeof(TSource), typeof(TDestination), cancellationToken));
		}

		/// <summary>
		/// Maps an object to an existing one asynchronously and returns the result
		/// </summary>
		/// <typeparam name="TSource">type of the object to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="source">object to be mapped, may be null</param>
		/// <param name="destination">object to map to, may be null</param>
		/// <param name="cancellationToken">cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>
		/// a task which when completed returns the resulting object of the mapping, which can be <paramref name="destination"/>
		/// or a new one, may be null
		/// </returns>
		public static Task<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> MapAsync<TSource, TDestination>(this IAsyncMapper mapper,
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
			CancellationToken cancellationToken = default) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, typeof(TSource), destination, typeof(TDestination), null, cancellationToken));
		}

		/// <summary>
		/// Maps an object to an existing one asynchronously and returns the result
		/// </summary>
		/// <typeparam name="TSource">type of the object to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="source">object to be mapped, may be null</param>
		/// <param name="destination">object to map to, may be null</param>
		/// <param name="mappingOptions">additional options for the current map</param>
		/// <param name="cancellationToken">cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>
		/// a task which when completed returns the resulting object of the mapping, which can be <paramref name="destination"/>
		/// or a new one, may be null
		/// </returns>
		public static Task<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> MapAsync<TSource, TDestination>(this IAsyncMapper mapper,
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
			mappingOptions,
			CancellationToken cancellationToken = default) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, typeof(TSource), destination, typeof(TDestination), mappingOptions, cancellationToken));
		}

		/// <summary>
		/// Maps a collection to an existing one asynchronously by matching the elements and returns the result
		/// </summary>
		/// <typeparam name="TSourceElement">type of the elements to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestinationElement">type of the destination elements, used to retrieve the available maps</typeparam>
		/// <param name="source">collection to be mapped, may be null</param>
		/// <param name="destination">collection to map to, may be null</param>
		/// <param name="matcher">matching method to be used to match elements of the <paramref name="source"/> and <paramref name="destination"/> collections</param>
		/// <param name="cancellationToken">cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>
		/// a task which when completed returns the resulting collection of the mapping, which can be <paramref name="destination"/>
		/// or a new one, may be null
		/// </returns>
		public static
			Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				ICollection<TDestinationElement>?
#else
				ICollection<TDestinationElement>
#endif
			>
			MapAsync<TSourceElement, TDestinationElement>(this IAsyncMapper mapper,
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
#if NET5_0_OR_GREATER
			Func<TSourceElement?, TDestinationElement?, MatchingContext, bool>
#else
			Func<TSourceElement, TDestinationElement, MatchingContext, bool>
#endif
			matcher,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));
			return TaskUtils.AwaitTask<ICollection<TDestinationElement>>(mapper.MapAsync(
				source,
				typeof(IEnumerable<TSourceElement>),
				destination,
				typeof(ICollection<TDestinationElement>),
				new MappingOptions {
					Matcher = (s, d, c) => (s is TSourceElement || object.Equals(s, default(TSourceElement))) &&
						(d is TDestinationElement || object.Equals(d, default(TDestinationElement))) &&
						matcher((TSourceElement)s, (TDestinationElement)d, c)
				}, cancellationToken));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Maps a collection to an existing one asynchronously by matching the elements and returns the result
		/// </summary>
		/// <typeparam name="TSourceElement">type of the elements to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestinationElement">type of the destination elements, used to retrieve the available maps</typeparam>
		/// <param name="source">collection to be mapped, may be null</param>
		/// <param name="destination">collection to map to, may be null</param>
		/// <param name="matcher">matching method to be used to match elements of the <paramref name="source"/> and <paramref name="destination"/> collections</param>
		/// <param name="removeNotMatchedDestinationElements">if true will remove all the elements from <paramref name="destination"/> which do not have a corresponding element in <paramref name="source"/></param>
		/// <param name="cancellationToken">cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>
		/// a task which when completed returns the resulting collection of the mapping, which can be <paramref name="destination"/>
		/// or a new one, may be null
		/// </returns>
		public static
			Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				ICollection<TDestinationElement>?
#else
				ICollection<TDestinationElement>
#endif
			>
			MapAsync<TSourceElement, TDestinationElement>(this IAsyncMapper mapper,
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
#if NET5_0_OR_GREATER
			Func<TSourceElement?, TDestinationElement?, MatchingContext, bool>
#else
			Func<TSourceElement, TDestinationElement, MatchingContext, bool>
#endif
			matcher,
			bool? removeNotMatchedDestinationElements,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));
			return TaskUtils.AwaitTask<ICollection<TDestinationElement>>(mapper.MapAsync(
				source,
				typeof(IEnumerable<TSourceElement>),
				destination,
				typeof(ICollection<TDestinationElement>),
				new MappingOptions {
					Matcher = (s, d, c) => (s is TSourceElement || object.Equals(s, default(TSourceElement))) &&
						(d is TDestinationElement || object.Equals(d, default(TDestinationElement))) &&
						matcher((TSourceElement)s, (TDestinationElement)d, c),
					CollectionRemoveNotMatchedDestinationElements = removeNotMatchedDestinationElements
				}, cancellationToken));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
