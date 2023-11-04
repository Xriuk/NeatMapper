﻿using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	public static class AsyncMapperExtensions {
		#region AsyncNewMap
		#region Runtime
		/// <summary>
		/// Maps an object to a new one asynchronously.<br/>
		/// Can also map to collections automatically, will create the destination collection and map each element individually
		/// </summary>
		/// <param name="source">Object to map, may be null</param>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object to create, used to retrieve the available maps</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>A task which when completed returns the newly created object of type <paramref name="destinationType"/> type, which may be null</returns>
		public static Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> MapAsync(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
			Type destinationType,
			CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return mapper.MapAsync(source, sourceType, destinationType, null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Maps an object to a new one asynchronously.<br/>
		/// Can also map to collections automatically, will create the destination collection and map each element individually
		/// </summary>
		/// <param name="source">Object to map, may be null</param>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object to create, used to retrieve the available maps</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>A task which when completed returns the newly created object of type <paramref name="destinationType"/> type, which may be null</returns>
		public static Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> MapAsync(this IAsyncMapper mapper,
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
			mappingOptions,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return mapper.MapAsync(source, sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region Explicit destination, inferred source
		/// <summary>
		/// Maps an object to a new one asynchronously
		/// </summary>
		/// <typeparam name="TDestination">Type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">
		/// Object to map, may NOT be null as the source type will be retrieved from it,
		/// which will be used to retrieve the available maps
		/// </param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>A task which when completed returns the newly created object, which may be null</returns>
		public static Task<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> MapAsync<TDestination>(this IAsyncMapper mapper,
			object source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
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
		/// <typeparam name="TDestination">Type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">
		/// Object to map, may NOT be null as the source type will be retrieved from it,
		/// which will be used to retrieve the available maps
		/// </param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>A task which when completed returns the newly created object, which may be null</returns>
		public static Task<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> MapAsync<TDestination>(this IAsyncMapper mapper,
			object source,
			CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (source == null)
				throw new ArgumentNullException(nameof(source), "Type cannot be inferred from null source, use an overload with an explicit source type");
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, source.GetType(), typeof(TDestination), null, cancellationToken));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Maps an object to a new one asynchronously
		/// </summary>
		/// <typeparam name="TDestination">Type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">
		/// Object to map, may NOT be null as the source type will be retrieved from it,
		/// which will be used to retrieve the available maps
		/// </param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>A task which when completed returns the newly created object, which may be null</returns>
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
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, source.GetType(), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region Explicit source and destination
		/// <summary>
		/// Maps an object to a new one asynchronously
		/// </summary>
		/// <typeparam name="TSource">Type of the object to map, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">Object to map, may be null</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>A task which when completed returns the newly created object, which may be null</returns>
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
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
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
		/// Maps an object to a new one asynchronously
		/// </summary>
		/// <typeparam name="TSource">Type of the object to map, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">Object to map, may be null</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>A task which when completed returns the newly created object, which may be null</returns>
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
			CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, typeof(TSource), typeof(TDestination), null, cancellationToken));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Maps an object to a new one asynchronously
		/// </summary>
		/// <typeparam name="TSource">Type of the object to map, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="source">Object to map, may be null</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>A task which when completed returns the newly created object, which may be null</returns>
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
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion
		#endregion

		#region AsyncMergeMap
		#region Runtime
		/// <summary>
		/// Maps an object to an existing one and returns the result.<br/>
		/// Can also map to collections automatically, will try to match elements with <see cref="IMatchMap{TSource, TDestination}"/>
		/// (or the passed <see cref="MergeCollectionsMappingOptions.Matcher"/>), will create the destination collection if it is null and map each element individually
		/// </summary>
		/// <param name="source">Object to be mapped, may be null</param>
		/// <param name="sourceType">Type of the object to be mapped, used to retrieve the available maps</param>
		/// <param name="destination">Object to map to, may be null</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>
		/// A task which when completed returns the resulting object of the mapping of type <paramref name="destinationType"/> type,
		/// which can be the same as <paramref name="destination"/> or a new one, may be null
		/// </returns>
		public static Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> MapAsync(this IAsyncMapper mapper,
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
			CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return mapper.MapAsync(source, sourceType, destination, destinationType, null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Maps an object to an existing one and returns the result.<br/>
		/// Can also map to collections automatically, will try to match elements with <see cref="IMatchMap{TSource, TDestination}"/>
		/// (or the passed <see cref="MergeCollectionsMappingOptions.Matcher"/>), will create the destination collection if it is null and map each element individually
		/// </summary>
		/// <param name="source">Object to be mapped, may be null</param>
		/// <param name="sourceType">Type of the object to be mapped, used to retrieve the available maps</param>
		/// <param name="destination">Object to map to, may be null</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps, null to ignore</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>
		/// A task which when completed returns the resulting object of the mapping of type <paramref name="destinationType"/> type,
		/// which can be the same as <paramref name="destination"/> or a new one, may be null
		/// </returns>
		public static Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> MapAsync(this IAsyncMapper mapper,
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
			mappingOptions,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return mapper.MapAsync(source, sourceType, destination, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region Explicit source and destination
		/// <summary>
		/// Maps an object to an existing one and returns the result
		/// </summary>
		/// <typeparam name="TSource">Type of the object to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="source">Object to be mapped, may be null</param>
		/// <param name="destination">Object to map to, may be null</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>
		/// A task which when completed returns the resulting object of the mapping,
		/// which can be the same as <paramref name="destination"/> or a new one, may be null
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
			mappingOptions = null,
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
		/// Maps an object to an existing one and returns the result
		/// </summary>
		/// <typeparam name="TSource">Type of the object to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="source">Object to be mapped, may be null</param>
		/// <param name="destination">Object to map to, may be null</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>
		/// A task which when completed returns the resulting object of the mapping,
		/// which can be the same as <paramref name="destination"/> or a new one, may be null
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
			CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, typeof(TSource), destination, typeof(TDestination), null, cancellationToken));

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
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>
		/// A task which when completed returns the resulting object of the mapping,
		/// which can be the same as <paramref name="destination"/> or a new one, may be null
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
			return TaskUtils.AwaitTask<TDestination>(mapper.MapAsync(source, typeof(TSource), destination, typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
		#endregion

		#region Collection
		/// <summary>
		/// Maps a collection to an existing one by matching the elements and returns the result
		/// </summary>
		/// <typeparam name="TSourceElement">Type of the elements to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestinationElement">Type of the destination elements, used to retrieve the available maps</typeparam>
		/// <param name="source">Collection to be mapped, may be null</param>
		/// <param name="destination">Collection to map to, may be null</param>
		/// <param name="matcher">Matching method to be used to match elements of the <paramref name="source"/> and <paramref name="destination"/> collections</param>
		/// <param name="mappingOptions">Additional options passed to the context, support depends on the mapper and/or the maps</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>
		/// A task which when completed returns the resulting collection of the mapping,
		/// which can be the same as <paramref name="destination"/> or a new one, may be null
		/// </returns>
		public static Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TDestinationElement>?
#else
			ICollection<TDestinationElement>
#endif
			> MapAsync<TSourceElement, TDestinationElement>(this IAsyncMapper mapper,
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
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));

			mappingOptions = (mappingOptions ?? MappingOptions.Empty).ReplaceOrAdd<MergeCollectionsMappingOptions>(m => new MergeCollectionsMappingOptions(
				m?.RemoveNotMatchedDestinationElements,
				(s, d, c) => (s is TSourceElement || object.Equals(s, default(TSourceElement))) &&
					(d is TDestinationElement || object.Equals(d, default(TDestinationElement))) &&
					matcher((TSourceElement)s, (TDestinationElement)d, c)));

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

		/// <summary>
		/// Maps a collection to an existing one by matching the elements and returns the result
		/// </summary>
		/// <typeparam name="TSourceElement">Type of the elements to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestinationElement">Type of the destination elements, used to retrieve the available maps</typeparam>
		/// <param name="source">Collection to be mapped, may be null</param>
		/// <param name="destination">Collection to map to, may be null</param>
		/// <param name="matcher">Matching method to be used to match elements of the <paramref name="source"/> and <paramref name="destination"/> collections</param>
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>
		/// A task which when completed returns the resulting collection of the mapping,
		/// which can be the same as <paramref name="destination"/> or a new one, may be null
		/// </returns>
		public static Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TDestinationElement>?
#else
			ICollection<TDestinationElement>
#endif
			> MapAsync<TSourceElement, TDestinationElement>(this IAsyncMapper mapper,
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
			CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return mapper.MapAsync<TSourceElement, TDestinationElement>(source, destination, matcher, (MappingOptions)null, cancellationToken);

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
		/// <param name="cancellationToken">Cancellation token used to cancel async operations, will be forwarded to all the contexts in the mapping</param>
		/// <returns>
		/// A task which when completed returns the resulting collection of the mapping,
		/// which can be the same as <paramref name="destination"/> or a new one, may be null
		/// </returns>
		public static Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TDestinationElement>?
#else
			ICollection<TDestinationElement>
#endif
			> MapAsync<TSourceElement, TDestinationElement>(this IAsyncMapper mapper,
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
			CancellationToken cancellationToken) {

			return mapper.MapAsync<TSourceElement, TDestinationElement>(source, destination, matcher, mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken);
		}
		#endregion
		#endregion

		#region CanMapAsyncNew
		#region Runtime
		/// <summary>
		/// Checks if the mapper can create a new object from a given one asynchronously, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source object (cached) and try to map it
		/// </summary>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object to create, used to retrieve the available maps</param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to map the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types
		/// </param>
		/// <param name="cancellationToken">Cancellation token used to interrupt asynchronous operations</param>
		/// <returns>A task which when completed returns <see langword="true"/> if an object of type <paramref name="destinationType"/> can be created from a parameter of type <paramref name="sourceType"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static async Task<bool> CanMapAsyncNew(this IAsyncMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

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
			if (mapper is IAsyncMapperCanMap mapperCanMap)
				return await mapperCanMap.CanMapAsyncNew(sourceType, destinationType, mappingOptions, cancellationToken);

			// Try creating a default source object and try mapping it
			object source;
			try {
				source = ObjectFactory.GetOrCreateCached(sourceType) ?? throw new Exception(); // Just in case
			}
			catch {
				throw new InvalidOperationException("Cannot verify if the mapper supports the given map because unable to create an object to test it");
			}

			try {
				await mapper.MapAsync(source, sourceType, destinationType, mappingOptions, cancellationToken);
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
		/// Checks if the mapper can create a new object from a given one asynchronously, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source object (cached) and try to map it
		/// </summary>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object to create, used to retrieve the available maps</param>
		/// <param name="cancellationToken">Cancellation token used to interrupt asynchronous operations</param>
		/// <returns>A task which when completed returns <see langword="true"/> if an object of type <paramref name="destinationType"/> can be created from a parameter of type <paramref name="sourceType"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static Task<bool> CanMapAsyncNew(this IAsyncMapper mapper, Type sourceType, Type destinationType, CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return mapper.CanMapAsyncNew(sourceType, destinationType, (MappingOptions)null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Checks if the mapper can create a new object from a given one asynchronously, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source object (cached) and try to map it
		/// </summary>
		/// <param name="sourceType">Type of the object to map, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object to create, used to retrieve the available maps</param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to map the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types
		/// </param>
		/// <param name="cancellationToken">Cancellation token used to interrupt asynchronous operations</param>
		/// <returns>A task which when completed returns <see langword="true"/> if an object of type <paramref name="destinationType"/> can be created from a parameter of type <paramref name="sourceType"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static Task<bool> CanMapAsyncNew(this IAsyncMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions,
			CancellationToken cancellationToken = default) {

			return mapper.CanMapAsyncNew(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken);
		}
		#endregion

		#region Explicit source and destination
		/// <summary>
		/// Checks if the mapper can create a new object from a given one asynchronously, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source object (cached) and try to map it
		/// </summary>
		/// <typeparam name="TSource">Type of the object to map, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="mappingOptions">
		/// Additional options which would be used to map the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types
		/// </param>
		/// <param name="cancellationToken">Cancellation token used to interrupt asynchronous operations</param>
		/// <returns>A task which when completed returns <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be created from a parameter of type <typeparamref name="TSource"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static Task<bool> CanMapAsyncNew<TSource, TDestination>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return mapper.CanMapAsyncNew(typeof(TSource), typeof(TDestination), mappingOptions, cancellationToken);
		}

		/// <summary>
		/// Checks if the mapper can create a new object from a given one asynchronously, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source object (cached) and try to map it
		/// </summary>
		/// <typeparam name="TSource">Type of the object to map, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="cancellationToken">Cancellation token used to interrupt asynchronous operations</param>
		/// <returns>A task which when completed returns <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be created from a parameter of type <typeparamref name="TSource"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static Task<bool> CanMapAsyncNew<TSource, TDestination>(this IAsyncMapper mapper, CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return mapper.CanMapAsyncNew(typeof(TSource), typeof(TDestination), (MappingOptions)null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Checks if the mapper can create a new object from a given one asynchronously, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source object (cached) and try to map it
		/// </summary>
		/// <typeparam name="TSource">Type of the object to map, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object to create, used to retrieve the available maps</typeparam>
		/// <param name="mappingOptions">
		/// Additional options which would be used to map the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types
		/// </param>
		/// <param name="cancellationToken">Cancellation token used to interrupt asynchronous operations</param>
		/// <returns>A task which when completed returns <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be created from a parameter of type <typeparamref name="TSource"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static Task<bool> CanMapAsyncNew<TSource, TDestination>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions,
			CancellationToken cancellationToken = default) {

			return mapper.CanMapAsyncNew(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken);
		}
		#endregion
		#endregion

		#region CanMapAsyncMerge
		#region Runtime
		/// <summary>
		/// Checks if the mapper can merge an object into an existing one asynchronously, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source (cached) and destination (not cached) objects and try to map them
		/// </summary>
		/// <param name="sourceType">Type of the object to be mapped, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to map the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types
		/// </param>
		/// <param name="cancellationToken">Cancellation token used to interrupt asynchronous operations</param>
		/// <returns>A task which when completed returns <see langword="true"/> if an object of type <paramref name="sourceType"/> can be merged into an object of type <paramref name="destinationType"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static async Task<bool> CanMapAsyncMerge(this IAsyncMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

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
			if (mapper is IAsyncMapperCanMap mapperCanMap)
				return await mapperCanMap.CanMapAsyncMerge(sourceType, destinationType, mappingOptions, cancellationToken);

			// Try creating two default source and destination objects and try mapping them,
			// cannot create a cached destination because it could be modified by the map so we could not reuse it
			object source;
			object destination;
			try {
				source = ObjectFactory.GetOrCreateCached(sourceType) ?? throw new Exception(); // Just in case
				destination = ObjectFactory.Create(destinationType) ?? throw new Exception(); // Just in case
			}
			catch {
				throw new InvalidOperationException("Cannot verify if the mapper supports the given map because unable to create the objects to test it");
			}

			try {
				await mapper.MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken);
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
		/// Checks if the mapper can merge an object into an existing one asynchronously, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source (cached) and destination (not cached) objects and try to map them
		/// </summary>
		/// <param name="sourceType">Type of the object to be mapped, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <param name="cancellationToken">Cancellation token used to interrupt asynchronous operations</param>
		/// <returns>A task which when completed returns <see langword="true"/> if an object of type <paramref name="sourceType"/> can be merged into an object of type <paramref name="destinationType"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static Task<bool> CanMapAsyncMerge(this IAsyncMapper mapper, Type sourceType, Type destinationType, CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return mapper.CanMapAsyncMerge(sourceType, destinationType, (MappingOptions)null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Checks if the mapper can merge an object into an existing one asynchronously, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source (cached) and destination (not cached) objects and try to map them
		/// </summary>
		/// <param name="sourceType">Type of the object to be mapped, used to retrieve the available maps</param>
		/// <param name="destinationType">Type of the destination object, used to retrieve the available maps</param>
		/// <param name="mappingOptions">
		/// Additional options which would be used to map the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types
		/// </param>
		/// <param name="cancellationToken">Cancellation token used to interrupt asynchronous operations</param>
		/// <returns>A task which when completed returns <see langword="true"/> if an object of type <paramref name="sourceType"/> can be merged into an object of type <paramref name="destinationType"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static Task<bool> CanMapAsyncMerge(this IAsyncMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions,
			CancellationToken cancellationToken = default) {

			return mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken);
		}
		#endregion

		#region Explicit source and destination
		/// <summary>
		/// Checks if the mapper can merge an object into an existing one asynchronously, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source (cached) and destination (not cached) objects and try to map them
		/// </summary>
		/// <typeparam name="TSource">Type of the object to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="mappingOptions">
		/// Additional options which would be used to map the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types
		/// </param>
		/// <param name="cancellationToken">Cancellation token used to interrupt asynchronous operations</param>
		/// <returns>A task which when completed returns <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be merged into an object of type <typeparamref name="TDestination"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static Task<bool> CanMapAsyncMerge<TSource, TDestination>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return mapper.CanMapAsyncMerge(typeof(TSource), typeof(TDestination), mappingOptions, cancellationToken);
		}

		/// <summary>
		/// Checks if the mapper can merge an object into an existing one asynchronously, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source (cached) and destination (not cached) objects and try to map them
		/// </summary>
		/// <typeparam name="TSource">Type of the object to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="cancellationToken">Cancellation token used to interrupt asynchronous operations</param>
		/// <returns>A task which when completed returns <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be merged into an object of type <typeparamref name="TDestination"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static Task<bool> CanMapAsyncMerge<TSource, TDestination>(this IAsyncMapper mapper, CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return mapper.CanMapAsyncMerge(typeof(TSource), typeof(TDestination), (MappingOptions)null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Checks if the mapper can merge an object into an existing one asynchronously, will check if the given mapper supports <see cref="IMapperCanMap"/> first
		/// or will create a dummy source (cached) and destination (not cached) objects and try to map them
		/// </summary>
		/// <typeparam name="TSource">Type of the object to be mapped, used to retrieve the available maps</typeparam>
		/// <typeparam name="TDestination">Type of the destination object, used to retrieve the available maps</typeparam>
		/// <param name="mappingOptions">
		/// Additional options which would be used to map the types, this helps obtaining more accurate results,
		/// since some mappers may depend on specific options to map or not two given types
		/// </param>
		/// <param name="cancellationToken">Cancellation token used to interrupt asynchronous operations</param>
		/// <returns>A task which when completed returns <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be merged into an object of type <typeparamref name="TDestination"/></returns>
		/// <exception cref="InvalidOperationException">Could not verify if the mapper supports the given types</exception>
		public static Task<bool> CanMapAsyncMerge<TSource, TDestination>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions,
			CancellationToken cancellationToken = default) {

			return mapper.CanMapAsyncMerge(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken);
		} 
		#endregion
		#endregion
	}
}