using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	public static class AsyncMapperExtensions {
		#region CanMapAsyncNew
		#region Runtime
		/// <inheritdoc cref="IAsyncMapper.CanMapAsyncNew(Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapAsyncNew(this IAsyncMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapAsyncNew(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="IAsyncMapper.CanMapAsyncNew(Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapAsyncNew(this IAsyncMapper mapper,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapAsyncNew(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IAsyncMapper.CanMapAsyncNew(Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IAsyncMapper.CanMapAsyncNew(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IAsyncMapper.CanMapAsyncNew(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IAsyncMapper.CanMapAsyncNew(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A task which when completed returns <see langword="true"/> if an object of type
		/// <typeparamref name="TDestination"/> can be created from a parameter of type
		/// <typeparamref name="TSource"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapAsyncNew<TSource, TDestination>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapAsyncNew(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="CanMapAsyncNew{TSource, TDestination}(IAsyncMapper, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapAsyncNew<TSource, TDestination>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapAsyncNew(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="CanMapAsyncNew{TSource, TDestination}(IAsyncMapper, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapAsyncNew<TSource, TDestination>(this IAsyncMapper mapper,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapAsyncNew(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region CanMapAsyncMerge
		#region Runtime
		/// <inheritdoc cref="IAsyncMapper.CanMapAsyncMerge(Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapAsyncMerge(this IAsyncMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="IAsyncMapper.CanMapAsyncMerge(Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapAsyncMerge(this IAsyncMapper mapper,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IAsyncMapper.CanMapAsyncMerge(Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IAsyncMapper.CanMapAsyncMerge(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IAsyncMapper.CanMapAsyncMerge(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IAsyncMapper.CanMapAsyncMerge(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A task which when completed returns <see langword="true"/> if an object of type
		/// <typeparamref name="TSource"/> can be merged into an object of type
		/// <typeparamref name="TDestination"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapAsyncMerge<TSource, TDestination>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapAsyncMerge(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="CanMapAsyncMerge{TSource, TDestination}(IAsyncMapper, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapAsyncMerge<TSource, TDestination>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapAsyncMerge(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="CanMapAsyncMerge{TSource, TDestination}(IAsyncMapper, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapAsyncMerge<TSource, TDestination>(this IAsyncMapper mapper,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapAsyncMerge(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region AsyncNewMap
		#region Runtime
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		#region MatchMapDelegate
		/// <summary>
		/// Maps a collection to an existing one asynchronously by matching the elements and returns the result.
		/// </summary>
		/// <typeparam name="TSourceElement">
		/// Type of the elements to be mapped, used to retrieve the available maps.
		/// </typeparam>
		/// <typeparam name="TDestinationElement">
		/// Type of the destination elements, used to retrieve the available maps.
		/// </typeparam>
		/// <param name="source">Collection to be mapped, may be null.</param>
		/// <param name="destination">Collection to map to, may be null.</param>
		/// <param name="matcher">
		/// Matching method to be used to match elements of the <paramref name="source"/>
		/// and <paramref name="destination"/> collections.
		/// </param>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)" path="/param[@name='cancellationToken']"/>
		/// <returns>
		/// A task which when completed returns the resulting collection of the mapping of type
		/// <typeparamref name="TDestinationElement"/>, which can be the same as <paramref name="destination"/>
		/// or a new one, may be null.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

			return mapper.MapAsync<IEnumerable<TSourceElement>, ICollection<TDestinationElement>>(source, destination,
				(mappingOptions ?? MappingOptions.Empty).AddMergeCollectionMatchers(DelegateMatcher.Create(matcher)),
				cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsync{TSourceElement, TDestinationElement}(IAsyncMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions, CancellationToken)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
			CancellationToken cancellationToken = default) {

			return mapper.MapAsync<TSourceElement, TDestinationElement>(source, destination, matcher, mappingOptions != null ? new MappingOptions(mappingOptions) : null, cancellationToken);
		}
		#endregion

		#region IEqualityComparer
		/// <inheritdoc cref="MapAsync{TSourceElement, TDestinationElement}(IAsyncMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions, CancellationToken)" path="/summary"/>
		/// <typeparam name="TElement">
		/// Type of the source and destination elements, used to retrieve the available maps.
		/// </typeparam>
		/// <inheritdoc cref="MapAsync{TSourceElement, TDestinationElement}(IAsyncMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions, CancellationToken)" path="/param[@name='source']"/>
		/// <inheritdoc cref="MapAsync{TSourceElement, TDestinationElement}(IAsyncMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions, CancellationToken)" path="/param[@name='destination']"/>
		/// <param name="comparer">
		/// Comparer to be used to match elements of the <paramref name="source"/>
		/// and <paramref name="destination"/> collections.
		/// </param>
		/// <inheritdoc cref="MapAsync{TSourceElement, TDestinationElement}(IAsyncMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions, CancellationToken)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="MapAsync{TSourceElement, TDestinationElement}(IAsyncMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions, CancellationToken)" path="/param[@name='cancellationToken']"/>
		/// <inheritdoc cref="MapAsync{TSourceElement, TDestinationElement}(IAsyncMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions, CancellationToken)" path="/returns"/>
		/// <inheritdoc cref="MapAsync{TSourceElement, TDestinationElement}(IAsyncMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions, CancellationToken)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TElement>?
#else
			ICollection<TElement>
#endif
			> MapAsync<TElement>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable<TElement>?
#else
			IEnumerable<TElement>
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TElement>?
#else
			ICollection<TElement>
#endif
			destination,
			IEqualityComparer<TElement> comparer,
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
			if (comparer == null)
				throw new ArgumentNullException(nameof(comparer));

			return mapper.MapAsync<IEnumerable<TElement>, ICollection<TElement>>(source, destination,
				(mappingOptions ?? MappingOptions.Empty).AddMergeCollectionMatchers(EqualityComparerMatcher.Create(comparer)),
				cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsync{TElement}(IAsyncMapper, IEnumerable{TElement}, ICollection{TElement}, IEqualityComparer{TElement}, MappingOptions, CancellationToken)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TElement>?
#else
			ICollection<TElement>
#endif
			> MapAsync<TElement>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable<TElement>?
#else
			IEnumerable<TElement>
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TElement>?
#else
			ICollection<TElement>
#endif
			destination,
			IEqualityComparer<TElement> comparer,
			CancellationToken cancellationToken) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return mapper.MapAsync<TElement>(source, destination, comparer, (MappingOptions)null, cancellationToken);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsync{TElement}(IAsyncMapper, IEnumerable{TElement}, ICollection{TElement}, IEqualityComparer{TElement}, MappingOptions, CancellationToken)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TElement>?
#else
			ICollection<TElement>
#endif
			> MapAsync<TElement>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable<TElement>?
#else
			IEnumerable<TElement>
#endif
			source,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TElement>?
#else
			ICollection<TElement>
#endif
			destination,
			IEqualityComparer<TElement> comparer,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions,
			CancellationToken cancellationToken = default) {

			return mapper.MapAsync<TElement>(source, destination, comparer, mappingOptions, cancellationToken);
		}
		#endregion
		#endregion
		#endregion


		#region MapAsyncNewFactory
		#region Runtime
		/// <summary>
		/// Creates a factory which can be used to map objects to new ones asynchronously, will check
		/// if the given mapper supports <see cref="IAsyncMapperFactory"/> first otherwise will return
		/// <see cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)"/> wrapped in a delegate.
		/// </summary>
		/// <remarks>
		/// If the mapper does not implement <see cref="IAsyncMapperFactory"/> it is NOT guaranteed
		/// that the created factory shares the same <see cref="AsyncMappingContextOptions"/>.
		/// </remarks>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncNewFactory(Type, Type, MappingOptions)"/>
		public static IAsyncNewMapFactory MapAsyncNewFactory(this IAsyncMapper mapper,
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

			// Check if the mapper implements IAsyncMapperFactory
			if (mapper is IAsyncMapperFactory mapperFactory)
				return mapperFactory.MapAsyncNewFactory(sourceType, destinationType, mappingOptions);

			if (!mapper.CanMapAsyncNew(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			// Return the map wrapped
			return new DefaultAsyncNewMapFactory(
				sourceType, destinationType,
				(source, cancellationToken) => mapper.MapAsync(source, sourceType, destinationType, mappingOptions, cancellationToken));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMapper, Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IAsyncNewMapFactory MapAsyncNewFactory(this IAsyncMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapAsyncNewFactory(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMapper, Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IAsyncNewMapFactory MapAsyncNewFactory(this IAsyncMapper mapper,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.MapAsyncNewFactory(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <inheritdoc cref="MapAsyncNewFactory(IAsyncMapper, Type, Type, MappingOptions)" path="/remarks"/>
		/// <typeparam name="TSource"><inheritdoc cref="IAsyncMapperFactory.MapAsyncNewFactory(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IAsyncMapperFactory.MapAsyncNewFactory(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncNewFactory(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A factory which can be used to map objects of type <typeparamref name="TSource"/> into new objects
		/// of type <typeparamref name="TDestination"/> asynchronously.
		/// </returns>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncNewFactory(Type, Type, MappingOptions)" path="/exception"/>
		public static AsyncNewMapFactory<TSource, TDestination> MapAsyncNewFactory<TSource, TDestination>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			var factory = mapper.MapAsyncNewFactory(typeof(TSource), typeof(TDestination), mappingOptions);
			try {
				return new DisposableAsyncNewMapFactory<TSource, TDestination>((source, cancellationToken) => TaskUtils.AwaitTask<TDestination>(factory.Invoke(source, cancellationToken)), factory);
			}
			catch {
				factory.Dispose();
				throw;
			}
			
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsyncNewFactory{TSource, TDestination}(IAsyncMapper, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AsyncNewMapFactory<TSource, TDestination> MapAsyncNewFactory<TSource, TDestination>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapAsyncNewFactory<TSource, TDestination>(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapAsyncNewFactory{TSource, TDestination}(IAsyncMapper, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AsyncNewMapFactory<TSource, TDestination> MapAsyncNewFactory<TSource, TDestination>(this IAsyncMapper mapper,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.MapAsyncNewFactory<TSource, TDestination>(mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
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
		/// <remarks>
		/// If the mapper does not implement <see cref="IAsyncMapperFactory"/> it is NOT guaranteed
		/// that the created factory shares the same <see cref="AsyncMappingContextOptions"/>.
		/// </remarks>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncMergeFactory(Type, Type, MappingOptions)"/>
		public static IAsyncMergeMapFactory MapAsyncMergeFactory(this IAsyncMapper mapper,
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

			// Check if the mapper implements IAsyncMapperFactory
			if (mapper is IAsyncMapperFactory mapperFactory)
				return mapperFactory.MapAsyncMergeFactory(sourceType, destinationType, mappingOptions);

			if (!mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			// Return the map wrapped
			return new DefaultAsyncMergeMapFactory(
				sourceType, destinationType,
				(source, destination, cancellationToken) => mapper.MapAsync(source, sourceType, destination, destinationType, mappingOptions, cancellationToken));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsyncMergeFactory(IAsyncMapper, Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IAsyncMergeMapFactory MapAsyncMergeFactory(this IAsyncMapper mapper,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapAsyncMergeFactory(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapAsyncMergeFactory(IAsyncMapper, Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IAsyncMergeMapFactory MapAsyncMergeFactory(this IAsyncMapper mapper,
			Type sourceType,
			Type destinationType,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.MapAsyncMergeFactory(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="MapAsyncMergeFactory(IAsyncMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <inheritdoc cref="MapAsyncMergeFactory(IAsyncMapper, Type, Type, MappingOptions)" path="/remarks"/>
		/// <typeparam name="TSource"><inheritdoc cref="IAsyncMapperFactory.MapAsyncMergeFactory(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IAsyncMapperFactory.MapAsyncMergeFactory(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncMergeFactory(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A factory which can be used to map objects of type <typeparamref name="TSource"/> into existing objects
		/// of type <typeparamref name="TDestination"/> asynchronously.
		/// </returns>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncMergeFactory(Type, Type, MappingOptions)" path="/exception"/>
		public static AsyncMergeMapFactory<TSource, TDestination> MapAsyncMergeFactory<TSource, TDestination>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			var factory = mapper.MapAsyncMergeFactory(typeof(TSource), typeof(TDestination), mappingOptions);
			try {
				return new DisposableAsyncMergeMapFactory<TSource, TDestination>((source, destination, cancellationToken) => TaskUtils.AwaitTask<TDestination>(factory.Invoke(source, destination, cancellationToken)), factory);
			}
			catch {
				factory.Dispose();
				throw;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsyncMergeFactory{TSource, TDestination}(IAsyncMapper, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AsyncMergeMapFactory<TSource, TDestination> MapAsyncMergeFactory<TSource, TDestination>(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapAsyncMergeFactory<TSource, TDestination>(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapAsyncMergeFactory{TSource, TDestination}(IAsyncMapper, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AsyncMergeMapFactory<TSource, TDestination> MapAsyncMergeFactory<TSource, TDestination>(this IAsyncMapper mapper,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.MapAsyncMergeFactory<TSource, TDestination>(mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Collection
		#region MatchMapDelegate
		/// <summary>
		/// Creates a factory which can be used to map collections to existing ones asynchronously by matching the elements,
		/// will check if the given mapper supports <see cref="IAsyncMapperFactory"/> first otherwise will return
		/// <see cref="IAsyncMapper.MapAsync(object, Type, object, Type, MappingOptions, CancellationToken)"/> wrapped in a delegate.
		/// </summary>
		/// <inheritdoc cref="MapAsyncMergeFactory(IAsyncMapper, Type, Type, MappingOptions)" path="/remarks"/>
		/// <typeparam name="TSourceElement">
		/// Type of the elements to be mapped, used to retrieve the available maps.
		/// </typeparam>
		/// <typeparam name="TDestinationElement">
		/// Type of the destination elements, used to retrieve the available maps.
		/// </typeparam>
		/// <param name="matcher">
		/// Matching method to be used to match elements of the source and destination collections.
		/// </param>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncMergeFactory(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A factory which can be used to map collections of type <typeparamref name="TSourceElement"/> into existing
		/// collections of type <typeparamref name="TDestinationElement"/> asynchronously.
		/// </returns>
		/// <inheritdoc cref="IAsyncMapperFactory.MapAsyncMergeFactory(Type, Type, MappingOptions)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AsyncMergeMapFactory<
			IEnumerable<TSourceElement>,
			ICollection<TDestinationElement>>
				MapAsyncMergeFactory<TSourceElement, TDestinationElement>(this IAsyncMapper mapper,
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

			return mapper.MapAsyncMergeFactory<IEnumerable<TSourceElement>, ICollection<TDestinationElement>>(
				(mappingOptions ?? MappingOptions.Empty).AddMergeCollectionMatchers(DelegateMatcher.Create(matcher)));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapAsyncMergeFactory{TSourceElement, TDestinationElement}(IAsyncMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AsyncMergeMapFactory<
			IEnumerable<TSourceElement>,
			ICollection<TDestinationElement>>
				MapAsyncMergeFactory<TSourceElement, TDestinationElement>(this IAsyncMapper mapper,
			MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapAsyncMergeFactory<TSourceElement, TDestinationElement>(matcher, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapAsyncMergeFactory{TSourceElement, TDestinationElement}(IAsyncMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AsyncMergeMapFactory<
			IEnumerable<TSourceElement>,
			ICollection<TDestinationElement>>
				MapAsyncMergeFactory<TSourceElement, TDestinationElement>(this IAsyncMapper mapper,
			MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.MapAsyncMergeFactory<TSourceElement, TDestinationElement>(matcher, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region IEqualityComparer
		/// <inheritdoc cref="MapAsyncMergeFactory{TSourceElement, TDestinationElement}(IAsyncMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/summary"/>
		/// <inheritdoc cref="MapAsyncMergeFactory{TSourceElement, TDestinationElement}(IAsyncMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/remarks"/>
		/// <typeparam name="TElement">
		/// Type of the source and destination elements, used to retrieve the available maps.
		/// </typeparam>
		/// <param name="comparer">
		/// Comparer to be used to match elements of the source and destination collections.
		/// </param>
		/// <inheritdoc cref="MapAsyncMergeFactory{TSourceElement, TDestinationElement}(IAsyncMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="MapAsyncMergeFactory{TSourceElement, TDestinationElement}(IAsyncMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/returns"/>
		/// <inheritdoc cref="MapAsyncMergeFactory{TSourceElement, TDestinationElement}(IAsyncMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AsyncMergeMapFactory<
			IEnumerable<TElement>,
			ICollection<TElement>>
				MapAsyncMergeFactory<TElement>(this IAsyncMapper mapper,
			IEqualityComparer<TElement> comparer,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (comparer == null)
				throw new ArgumentNullException(nameof(comparer));

			return mapper.MapAsyncMergeFactory<IEnumerable<TElement>, ICollection<TElement>>(
				(mappingOptions ?? MappingOptions.Empty).AddMergeCollectionMatchers(EqualityComparerMatcher.Create(comparer)));
		}

		/// <inheritdoc cref="MapAsyncMergeFactory{TElement}(IAsyncMapper, IEqualityComparer{TElement}, MappingOptions)"/>
		public static AsyncMergeMapFactory<
			IEnumerable<TElement>,
			ICollection<TElement>>
				MapAsyncMergeFactory<TElement>(this IAsyncMapper mapper,
			IEqualityComparer<TElement> comparer,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			if (comparer == null)
				throw new ArgumentNullException(nameof(comparer));

			return mapper.MapAsyncMergeFactory<TElement>(comparer, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapAsyncMergeFactory{TElement}(IAsyncMapper, IEqualityComparer{TElement}, MappingOptions)"/>
		public static AsyncMergeMapFactory<
			IEnumerable<TElement>,
			ICollection<TElement>>
				MapAsyncMergeFactory<TElement>(this IAsyncMapper mapper,
			IEqualityComparer<TElement> comparer,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			if (comparer == null)
				throw new ArgumentNullException(nameof(comparer));

			return mapper.MapAsyncMergeFactory<TElement>(comparer, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion
		#endregion


		#region GetAsyncNewMaps
		/// <summary>
		/// Retrieves a collection of type pairs which can be mapped to create new objects asynchronously, will check
		/// if the given mapper supports <see cref="IAsyncMapperMaps"/> otherwise will return an empty result.
		/// It does not guarantee that the actual maps will succeed.
		/// </summary>
		/// <inheritdoc cref="IAsyncMapperMaps.GetAsyncNewMaps(MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetAsyncNewMaps(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (mapper is IAsyncMapperMaps maps)
				return maps.GetAsyncNewMaps(mappingOptions);
			else
				return Enumerable.Empty<(Type, Type)>();
		}

		/// <inheritdoc cref="GetAsyncNewMaps(IAsyncMapper, MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetAsyncNewMaps(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			if (mapper is IAsyncMapperMaps maps)
				return maps.GetAsyncNewMaps(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
			else
				return Enumerable.Empty<(Type, Type)>();
		}

		/// <inheritdoc cref="GetAsyncNewMaps(IAsyncMapper, MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetAsyncNewMaps(this IAsyncMapper mapper,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			if (mapper is IAsyncMapperMaps maps)
				return maps.GetAsyncNewMaps(mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
			else
				return Enumerable.Empty<(Type, Type)>();
		}
		#endregion

		#region GetAsyncMergeMaps
		/// <summary>
		/// Retrieves a collection of type pairs which can be mapped to merge objects asynchronously, will check
		/// if the given mapper supports <see cref="IAsyncMapperMaps"/> otherwise will return an empty result.
		/// It does not guarantee that the actual maps will succeed.
		/// </summary>
		/// <inheritdoc cref="IAsyncMapperMaps.GetAsyncMergeMaps(MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetAsyncMergeMaps(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (mapper is IAsyncMapperMaps maps)
				return maps.GetAsyncMergeMaps(mappingOptions);
			else
				return Enumerable.Empty<(Type, Type)>();
		}

		/// <inheritdoc cref="GetAsyncMergeMaps(IAsyncMapper, MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetAsyncMergeMaps(this IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			if (mapper is IAsyncMapperMaps maps)
				return maps.GetAsyncMergeMaps(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
			else
				return Enumerable.Empty<(Type, Type)>();
		}

		/// <inheritdoc cref="GetAsyncMergeMaps(IAsyncMapper, MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetAsyncMergeMaps(this IAsyncMapper mapper,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			if (mapper is IAsyncMapperMaps maps)
				return maps.GetAsyncMergeMaps(mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
			else
				return Enumerable.Empty<(Type, Type)>();
		}
		#endregion
	}
}
