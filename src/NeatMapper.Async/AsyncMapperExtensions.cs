using NeatMapper.Async.Internal;
using NeatMapper.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
			return mapper.MapAsync<TDestination>(source, null, cancellationToken);
		}

		/// <summary>
		/// Maps an object to a new one asynchronously
		/// </summary>
		/// <typeparam name="TDestination">type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">
		/// object to map, may NOT be null as the source type will be retrieved from it,
		/// which will be used to retrieve the available maps
		/// </param>
		/// <param name="mappingOptions">additional options passed to the context, support depends on the mapper and/or the maps, null to ignore</param>
		/// <param name="cancellationToken">cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>a task which when completed returns the newly created object, which may be null</returns>
		public static Task<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> MapAsync<TDestination>(this IAsyncMapper mapper,
			object source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (source == null)
				throw new ArgumentNullException(nameof(source), "Type cannot be inferred from null source, use an overload with an explicit source type");
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, source.GetType(), typeof(TDestination), mappingOptions, cancellationToken));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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

			return mapper.MapAsync<TSource, TDestination>(source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			(IEnumerable?)
#else
			(IEnumerable)
#endif
				null, cancellationToken);
		}

		/// <summary>
		/// Maps an object to a new one asynchronously
		/// </summary>
		/// <typeparam name="TSource">type of the object to map, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">object to map, may be null</param>
		/// <param name="mappingOptions">additional options passed to the context, support depends on the mapper and/or the maps, null to ignore</param>
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
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, typeof(TSource), typeof(TDestination), mappingOptions, cancellationToken));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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

			return mapper.MapAsync<TSource, TDestination>(source, destination, null, cancellationToken);
		}

		/// <summary>
		/// Maps an object to an existing one asynchronously and returns the result
		/// </summary>
		/// <typeparam name="TSource">type of the object to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="source">object to be mapped, may be null</param>
		/// <param name="destination">object to map to, may be null</param>
		/// <param name="mappingOptions">additional options passed to the context, support depends on the mapper and/or the maps, null to ignore</param>
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
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, typeof(TSource), destination, typeof(TDestination), mappingOptions, cancellationToken));

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
			MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
			CancellationToken cancellationToken = default) {

			return mapper.MapAsync<TSourceElement, TDestinationElement>(source, destination, matcher, null, cancellationToken);
		}

		/// <summary>
		/// Maps a collection to an existing one asynchronously by matching the elements and returns the result
		/// </summary>
		/// <typeparam name="TSourceElement">type of the elements to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestinationElement">type of the destination elements, used to retrieve the available maps</typeparam>
		/// <param name="source">collection to be mapped, may be null</param>
		/// <param name="destination">collection to map to, may be null</param>
		/// <param name="matcher">matching method to be used to match elements of the <paramref name="source"/> and <paramref name="destination"/> collections</param>
		/// <param name="mappingOptions">additional options passed to the context, support depends on the mapper and/or the maps, null to ignore</param>
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
			MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));

			var mergeMappingOptions = mappingOptions?.Cast<object>().OfType<MergeCollectionsMappingOptions>().FirstOrDefault();
			if (mergeMappingOptions == null) {
				mergeMappingOptions = new MergeCollectionsMappingOptions();
				mappingOptions = mappingOptions != null ? mappingOptions.Cast<object>().Concat(new object[] { mergeMappingOptions }) : new object[] { mergeMappingOptions };
			}
			mergeMappingOptions.Matcher = (s, d, c) => (s is TSourceElement || object.Equals(s, default(TSourceElement))) &&
				(d is TDestinationElement || object.Equals(d, default(TDestinationElement))) &&
				matcher((TSourceElement)s, (TDestinationElement)d, c);

			return TaskUtils.AwaitTask<ICollection<TDestinationElement>>(mapper.MapAsync(
				source,
				typeof(IEnumerable<TSourceElement>),
				destination,
				typeof(ICollection<TDestinationElement>),
				mappingOptions,
				cancellationToken));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
