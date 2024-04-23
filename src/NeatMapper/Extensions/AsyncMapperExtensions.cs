using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	public static class AsyncMapperExtensions {
		#region AsyncNewMap
		#region Runtime
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)"/>
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

		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)"/>
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
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)" path="/summary"/>
		/// <typeparam name="TDestination"><inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)" path="/param[@name='destinationType']"/></typeparam>
		/// <param name="source">
		/// Object to map, CANNOT be null as the source type will be retrieved from it,
		/// which will be used to retrieve the available maps.
		/// </param>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)" path="/param[@name='cancellationToken']"/>
		/// <returns>
		/// A task which when completed returns the newly created object of type <typeparamref name="TDestination"/>,
		/// which may be null.
		/// </returns>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)" path="/exception"/>
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

		/// <inheritdoc cref="MapAsync{TDestination}(IAsyncMapper, object, MappingOptions, CancellationToken)"/>
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

		/// <inheritdoc cref="MapAsync{TDestination}(IAsyncMapper, object, MappingOptions, CancellationToken)"/>
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
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)" path="/param[@name='sourceType']"/></typeparam>
		/// <inheritdoc cref="MapAsync{TDestination}(IAsyncMapper, object, MappingOptions, CancellationToken)" path="/typeparam[@name='TDestination']"/>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)" path="/param[@name='source']"/>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)" path="/param[@name='cancellationToken']"/>
		/// <returns>
		/// A task which when completed returns the newly created object of type <typeparamref name="TDestination"/>,
		/// which may be null.
		/// </returns>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)" path="/exception"/>
		public static Task<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
#pragma warning disable CS1712
			> MapAsync<TSource, TDestination>(this IAsyncMapper mapper,
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

		/// <inheritdoc cref="MapAsync{TSource, TDestination}(IAsyncMapper, TSource, MappingOptions, CancellationToken)" />
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

		/// <inheritdoc cref="MapAsync{TSource, TDestination}(IAsyncMapper, TSource, MappingOptions, CancellationToken)" />
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
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)"/>
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

		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)"/>
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
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)" path="/param[@name='source']"/>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)" path="/param[@name='destination']"/>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)" path="/param[@name='cancellationToken']"/>
		/// <returns>
		/// A task which when completed returns the resulting object of the mapping of type
		/// <typeparamref name="TDestination"/>, which can be the same as <paramref name="destination"/>
		/// or a new one, may be null.
		/// </returns>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)" path="/exception"/>
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

		/// <inheritdoc cref="MapAsync{TSource, TDestination}(IAsyncMapper, TSource, TDestination, MappingOptions, CancellationToken)"/>
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

		/// <inheritdoc cref="MapAsync{TSource, TDestination}(IAsyncMapper, TSource, TDestination, MappingOptions, CancellationToken)"/>
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
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)" path="/param[@name='cancellationToken']"/>
		/// <returns>
		/// A task which when completed returns the resulting collection of the mapping of type
		/// <typeparamref name="TDestinationElement"/>, which can be the same as <paramref name="destination"/>
		/// or a new one, may be null.
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
				(s, d, c) => {
					if ((!(s is TSourceElement) && !object.Equals(s, default(TSourceElement))) ||
						(!(d is TDestinationElement) && !object.Equals(d, default(TDestinationElement)))) {

						throw new MapNotFoundException((typeof(TSourceElement), typeof(TDestinationElement)));
					}

					return matcher((TSourceElement)s, (TDestinationElement)d, c);
				}));

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

		/// <inheritdoc cref="MapAsync{TSourceElement, TDestinationElement}(IAsyncMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions, CancellationToken)"/>
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

		/// <inheritdoc cref="MapAsync{TSourceElement, TDestinationElement}(IAsyncMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions, CancellationToken)"/>
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
		/// Checks if the mapper could create a new object from a given one asynchronously, will check if the given mapper
		/// supports <see cref="IAsyncMapperCanMap"/> first or will create a dummy source object (cached) and try to map it.
		/// It does not guarantee that the actual map will succeed.
		/// </summary>
		/// <inheritdoc cref="IAsyncMapperCanMap.CanMapAsyncNew(Type, Type, MappingOptions, CancellationToken)"/>
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

			// Check if the mapper implements IAsyncMapperCanMap, if it throws it means that the map can be checked only when mapping
			if (mapper is IAsyncMapperCanMap mapperCanMap)
				return await mapperCanMap.CanMapAsyncNew(sourceType, destinationType, mappingOptions, cancellationToken);

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
				await mapper.MapAsync(source, sourceType, destinationType, mappingOptions, cancellationToken);
				return true;
			}
			catch (MapNotFoundException) {
				return false;
			}
			catch (Exception e) {
				throw new InvalidOperationException(
					"Cannot verify if the mapper supports the given map because it threw an exception while trying to map a dummy object. " +
					"Check inner exception for details.", e);
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="CanMapAsyncNew(IAsyncMapper, Type, Type, MappingOptions, CancellationToken)"/>
		public static Task<bool> CanMapAsyncNew(this IAsyncMapper mapper, Type sourceType, Type destinationType, CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return mapper.CanMapAsyncNew(sourceType, destinationType, (MappingOptions)null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="CanMapAsyncNew(IAsyncMapper, Type, Type, MappingOptions, CancellationToken)"/>
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
		/// <inheritdoc cref="IAsyncMapperCanMap.CanMapAsyncNew(Type, Type, MappingOptions, CancellationToken)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IAsyncMapperCanMap.CanMapAsyncNew(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IAsyncMapperCanMap.CanMapAsyncNew(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IAsyncMapperCanMap.CanMapAsyncNew(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="IAsyncMapperCanMap.CanMapAsyncNew(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='cancellationToken']"/>
		/// <returns>
		/// A task which when completed returns <see langword="true"/> if an object of type
		/// <typeparamref name="TDestination"/> can be created from a parameter of type
		/// <typeparamref name="TSource"/>.
		/// </returns>
		/// <inheritdoc cref="IAsyncMapperCanMap.CanMapAsyncNew(Type, Type, MappingOptions, CancellationToken)" path="/exception"/>
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

		/// <inheritdoc cref="CanMapAsyncNew{TSource, TDestination}(IAsyncMapper, MappingOptions, CancellationToken)"/>
		public static Task<bool> CanMapAsyncNew<TSource, TDestination>(this IAsyncMapper mapper, CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return mapper.CanMapAsyncNew(typeof(TSource), typeof(TDestination), (MappingOptions)null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="CanMapAsyncNew{TSource, TDestination}(IAsyncMapper, MappingOptions, CancellationToken)"/>
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
		/// Checks if the mapper could merge an object into an existing one asynchronously, will check
		/// if the given mapper supports <see cref="IAsyncMapperCanMap"/> first or will create a dummy source (cached)
		/// and destination (not cached) objects and try to map them. It does not guarantee that the actual map will succeed.
		/// </summary>
		/// <inheritdoc cref="IAsyncMapperCanMap.CanMapAsyncMerge(Type, Type, MappingOptions, CancellationToken)"/>
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

			// Check if the mapper implements IAsyncMapperCanMap, if it throws it means that the map can be checked only when mapping
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
				throw new InvalidOperationException(
					"Cannot verify if the mapper supports the given map because unable to create dummy objects to test it.");
			}

			try {
				await mapper.MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken);
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

		/// <inheritdoc cref="CanMapAsyncMerge(IAsyncMapper, Type, Type, MappingOptions, CancellationToken)"/>
		public static Task<bool> CanMapAsyncMerge(this IAsyncMapper mapper, Type sourceType, Type destinationType, CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return mapper.CanMapAsyncMerge(sourceType, destinationType, (MappingOptions)null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="CanMapAsyncMerge(IAsyncMapper, Type, Type, MappingOptions, CancellationToken)"/>
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
		/// <inheritdoc cref="CanMapAsyncMerge(IAsyncMapper, Type, Type, MappingOptions, CancellationToken)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IAsyncMapperCanMap.CanMapAsyncMerge(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IAsyncMapperCanMap.CanMapAsyncMerge(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IAsyncMapperCanMap.CanMapAsyncMerge(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="IAsyncMapperCanMap.CanMapAsyncMerge(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='cancellationToken']"/>
		/// <returns>
		/// A task which when completed returns <see langword="true"/> if an object of type
		/// <typeparamref name="TSource"/> can be merged into an object of type
		/// <typeparamref name="TDestination"/>.
		/// </returns>
		/// <inheritdoc cref="IAsyncMapperCanMap.CanMapAsyncMerge(Type, Type, MappingOptions, CancellationToken)" path="/exception"/>
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

		/// <inheritdoc cref="CanMapAsyncMerge{TSource, TDestination}(IAsyncMapper, MappingOptions, CancellationToken)"/>
		public static Task<bool> CanMapAsyncMerge<TSource, TDestination>(this IAsyncMapper mapper, CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return mapper.CanMapAsyncMerge(typeof(TSource), typeof(TDestination), (MappingOptions)null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="CanMapAsyncMerge{TSource, TDestination}(IAsyncMapper, MappingOptions, CancellationToken)"/>
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


		#region MapAsyncNewFactory
		#region Runtime
		/// <summary>
		/// Creates a factory which can be used to map objects to new ones asynchronously, will check
		/// if the given mapper supports <see cref="IAsyncMapperFactory"/> first otherwise will return
		/// <see cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)"/> wrapped in a delegate.
		/// </summary>
		/// <remarks>It is NOT guaranteed that the created factory shares the same <see cref="AsyncMappingContext"/>.</remarks>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncNewFactory(Type, Type, MappingOptions, CancellationToken)"/>
		public static IAsyncNewMapFactory MapAsyncNewFactory(this IAsyncMapper mapper,
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

			// Check if the mapper implements IAsyncMapperFactory
			if (mapper is IAsyncMapperFactory mapperFactory)
				return mapperFactory.MapAsyncNewFactory(sourceType, destinationType, mappingOptions, cancellationToken);

			// Check if the mapper can map the types (we don't do it via the extension method above because
			// it may require actually mapping the two types if the interface is not implemented,
			// and as the returned factory may still throw MapNotFoundException we are still compliant)
			if (mapper is IAsyncMapperCanMap mapperCanMap) {
				try {
					if (!mapperCanMap.CanMapAsyncNew(sourceType, destinationType, mappingOptions, cancellationToken).Result)
						throw new MapNotFoundException((sourceType, destinationType));
				}
				catch (MapNotFoundException) {
					throw;
				}
				catch { }
			}

			// Return the map wrapped
			return new AsyncNewMapFactory(sourceType, destinationType, source => mapper.MapAsync(source, sourceType, destinationType, mappingOptions, cancellationToken));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMapper, Type, Type, MappingOptions, CancellationToken)"/>
		public static IAsyncNewMapFactory MapAsyncNewFactory(this IAsyncMapper mapper, Type sourceType, Type destinationType, CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return mapper.MapAsyncNewFactory(sourceType, destinationType, (MappingOptions)null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMapper, Type, Type, MappingOptions, CancellationToken)"/>
		public static IAsyncNewMapFactory MapAsyncNewFactory(this IAsyncMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions,
			CancellationToken cancellationToken = default) {

			return mapper.MapAsyncNewFactory(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMapper, Type, Type, MappingOptions, CancellationToken)" path="/summary"/>
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMapper, Type, Type, MappingOptions, CancellationToken)" path="/remarks"/>
		/// <typeparam name="TSource"><inheritdoc cref="IAsyncMapperFactory.MapAsyncNewFactory(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IAsyncMapperFactory.MapAsyncNewFactory(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncNewFactory(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncNewFactory(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='cancellationToken']"/>
		/// <returns>
		/// A factory which can be used to map objects of type <typeparamref name="TSource"/> into new objects
		/// of type <typeparamref name="TDestination"/> asynchronously.
		/// </returns>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncNewFactory(Type, Type, MappingOptions, CancellationToken)" path="/exception"/>
		public static IAsyncNewMapFactory<TSource, TDestination> MapAsyncNewFactory<TSource, TDestination>(this IAsyncMapper mapper,
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

			var factory = mapper.MapAsyncNewFactory(typeof(TSource), typeof(TDestination), mappingOptions, cancellationToken);
			return new DisposableAsyncNewMapFactory<TSource, TDestination>(async source => (TDestination)await factory.Invoke(source), factory);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsyncNewFactory{TSource, TDestination}(IAsyncMapper, MappingOptions, CancellationToken)"/>
		public static IAsyncNewMapFactory<TSource, TDestination> MapAsyncNewFactory<TSource, TDestination>(this IAsyncMapper mapper, CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return mapper.MapAsyncNewFactory<TSource, TDestination>((MappingOptions)null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsyncNewFactory{TSource, TDestination}(IAsyncMapper, MappingOptions, CancellationToken)"/>
		public static IAsyncNewMapFactory<TSource, TDestination> MapAsyncNewFactory<TSource, TDestination>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions,
			CancellationToken cancellationToken = default) {

			return mapper.MapAsyncNewFactory<TSource, TDestination>(mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken);
		}
		#endregion
		#endregion

		#region MapAsyncMergeFactory
		#region Runtime
		/// <summary>
		/// Creates a factory which can be used to map objects to existing ones asynchronously, will check
		/// if the given mapper supports <see cref="IAsyncMapperFactory"/> first otherwise will return
		/// <see cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)"/> wrapped in a delegate.
		/// </summary>
		/// <remarks>It is NOT guaranteed that the created factory shares the same <see cref="AsyncMappingContext"/>.</remarks>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncMergeFactory(Type, Type, MappingOptions, CancellationToken)"/>
		public static IAsyncMergeMapFactory MapAsyncMergeFactory(this IAsyncMapper mapper,
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

			// Check if the mapper implements IAsyncMapperFactory
			if (mapper is IAsyncMapperFactory mapperFactory)
				return mapperFactory.MapAsyncMergeFactory(sourceType, destinationType, mappingOptions, cancellationToken);

			// Check if the mapper can map the types (we don't do it via the extension method above because
			// it may require actually mapping the two types if the interface is not implemented,
			// and as the returned factory may still throw MapNotFoundException we are still compliant)
			if (mapper is IAsyncMapperCanMap mapperCanMap) {
				try {
					if (!mapperCanMap.CanMapAsyncMerge(sourceType, destinationType, mappingOptions, cancellationToken).Result)
						throw new MapNotFoundException((sourceType, destinationType));
				}
				catch (MapNotFoundException) {
					throw;
				}
				catch { }
			}

			// Return the map wrapped
			return new AsyncMergeMapFactory(sourceType, destinationType, (source, destination) => mapper.MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsyncMergeFactory(IAsyncMapper, Type, Type, MappingOptions, CancellationToken)"/>
		public static IAsyncMergeMapFactory MapAsyncMergeFactory(this IAsyncMapper mapper, Type sourceType, Type destinationType, CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return mapper.MapAsyncMergeFactory(sourceType, destinationType, (MappingOptions)null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsyncMergeFactory(IAsyncMapper, Type, Type, MappingOptions, CancellationToken)"/>
		public static IAsyncMergeMapFactory MapAsyncMergeFactory(this IAsyncMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions,
			CancellationToken cancellationToken = default) {

			return mapper.MapAsyncMergeFactory(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="MapAsyncMergeFactory(IAsyncMapper, Type, Type, MappingOptions, CancellationToken)" path="/summary"/>
		/// <inheritdoc cref="MapAsyncMergeFactory(IAsyncMapper, Type, Type, MappingOptions, CancellationToken)" path="/remarks"/>
		/// <typeparam name="TSource"><inheritdoc cref="IAsyncMapperFactory.MapAsyncMergeFactory(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IAsyncMapperFactory.MapAsyncMergeFactory(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncMergeFactory(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncMergeFactory(Type, Type, MappingOptions, CancellationToken)" path="/param[@name='cancellationToken']"/>
		/// <returns>
		/// A factory which can be used to map objects of type <typeparamref name="TSource"/> into existing objects
		/// of type <typeparamref name="TDestination"/> asynchronously.<br/>
		/// The factory when invoked may throw <see cref="MapNotFoundException"/> or <see cref="MappingException"/> exceptions.
		/// </returns>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncMergeFactory(Type, Type, MappingOptions, CancellationToken)" path="/exception"/>
		public static IAsyncMergeMapFactory<TSource, TDestination> MapAsyncMergeFactory<TSource, TDestination>(this IAsyncMapper mapper,
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

			var factory = mapper.MapAsyncMergeFactory(typeof(TSource), typeof(TDestination), mappingOptions, cancellationToken);
			return new DisposableAsyncMergeMapFactory<TSource, TDestination>(async (source, destination) => (TDestination)await factory.Invoke(source, destination), factory);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsyncMergeFactory{TSource, TDestination}(IAsyncMapper, MappingOptions, CancellationToken)"/>
		public static IAsyncMergeMapFactory<TSource, TDestination> MapAsyncMergeFactory<TSource, TDestination>(this IAsyncMapper mapper, CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return mapper.MapAsyncMergeFactory<TSource, TDestination>((MappingOptions)null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsyncMergeFactory{TSource, TDestination}(IAsyncMapper, MappingOptions, CancellationToken)"/>
		public static IAsyncMergeMapFactory<TSource, TDestination> MapAsyncMergeFactory<TSource, TDestination>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions,
			CancellationToken cancellationToken = default) {

			return mapper.MapAsyncMergeFactory<TSource, TDestination>(mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken);
		}
		#endregion
		#endregion
	}
}
