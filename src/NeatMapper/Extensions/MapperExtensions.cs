using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

		// No params overload because it may become problematic if people try to use it to merge types
		// like: mapper.Map<TDestination>(source, destination), in this case destination is actually passed
		// to mappingOptions
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

		// No params overload because it overlaps with merge map with explicit source and destination:
		// mapper.Map<TSource, TDestination>(source, destination, option1, ...), in this case destination
		// is actually passed to mappingOptions
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
		#region MatchMapDelegate
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

			return mapper.Map<IEnumerable<TSourceElement>, ICollection<TDestinationElement>>(source, destination,
				(mappingOptions ?? MappingOptions.Empty).AddMergeCollectionMatchers(DelegateMatcher.Create(matcher)));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		#region IEqualityComparer
		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/summary"/>
		/// <typeparam name="TElement">
		/// Type of the source and destination elements, used to retrieve the available maps.
		/// </typeparam>
		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/param[@name='source']"/>
		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/param[@name='destination']"/>
		/// <param name="comparer">
		/// Comparer to be used to match elements of the <paramref name="source"/>
		/// and <paramref name="destination"/> collections.
		/// </param>
		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/returns"/>
		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/exception"/>
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TElement>?
#else
			ICollection<TElement>
#endif
			Map<TElement>(this IMapper mapper,
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
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (comparer == null)
				throw new ArgumentNullException(nameof(comparer));

			return mapper.Map<IEnumerable<TElement>, ICollection<TElement>>(source, destination,
				(mappingOptions ?? MappingOptions.Empty).AddMergeCollectionMatchers(EqualityComparerMatcher.Create(comparer)));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Map{TElement}(IMapper, IEnumerable{TElement}, ICollection{TElement}, IEqualityComparer{TElement}, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TElement>?
#else
			ICollection<TElement>
#endif
			Map<TElement>(this IMapper mapper,
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
			mappingOptions) {

			return mapper.Map<TElement>(source, destination, comparer, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Map{TElement}(IMapper, IEnumerable{TElement}, ICollection{TElement}, IEqualityComparer{TElement}, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			ICollection<TElement>?
#else
			ICollection<TElement>
#endif
			Map<TElement>(this IMapper mapper,
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
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			return mapper.Map<TElement>(source, destination, comparer, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion
		#endregion


		#region CanMapNew
		#region Runtime
		/// <inheritdoc cref="IMapper.CanMapNew(Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		/// <inheritdoc cref="IMapper.CanMapNew(Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		/// <inheritdoc cref="IMapper.CanMapNew(Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapper.CanMapNew(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapper.CanMapNew(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapper.CanMapNew(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be created
		/// from a parameter of type <typeparamref name="TSource"/>.
		/// </returns>
		/// <inheritdoc cref="IMapper.CanMapNew(Type, Type, MappingOptions)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapNew<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return mapper.CanMapNew(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="CanMapNew{TSource, TDestination}(IMapper, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapNew<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.CanMapNew(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="CanMapNew{TSource, TDestination}(IMapper, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be merged into an object
		/// of type <typeparamref name="TDestination"/>.
		/// </returns>
		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapMerge<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return mapper.CanMapMerge(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be merged into an object
		/// of type <typeparamref name="TDestination"/>.
		/// </returns>
		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapMerge<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.CanMapMerge(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be merged into an object
		/// of type <typeparamref name="TDestination"/>.
		/// </returns>
		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		/// <remarks>
		/// If the mapper does not implement <see cref="IMapperFactory"/> it is NOT guaranteed
		/// that the created factory shares the same <see cref="MappingContext"/>.
		/// </remarks>
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
			try {
				if(!mapper.CanMapNew(sourceType, destinationType, mappingOptions))
					throw new MapNotFoundException((sourceType, destinationType));
			}
			catch (MapNotFoundException) {
				throw;
			}
			catch { }

			// Return the map wrapped
			return new DefaultNewMapFactory(
				sourceType, destinationType,
				source => mapper.Map(source, sourceType, destinationType, mappingOptions));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapNewFactory(IMapper, Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
			try { 
				return new DisposableNewMapFactory<TSource, TDestination>(source => (TDestination)factory.Invoke(source), factory);
			}
			catch {
				factory.Dispose();
				throw;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapNewFactory{TSource, TDestination}(IMapper, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NewMapFactory<TSource, TDestination> MapNewFactory<TSource, TDestination>(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return mapper.MapNewFactory<TSource, TDestination>(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapNewFactory{TSource, TDestination}(IMapper, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		/// <remarks>
		/// If the mapper does not implement <see cref="IMapperFactory"/> it is NOT guaranteed
		/// that the created factory shares the same <see cref="MappingContext"/>.
		/// </remarks>
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
			try {
				if (!mapper.CanMapMerge(sourceType, destinationType, mappingOptions))
					throw new MapNotFoundException((sourceType, destinationType));
			}
			catch (MapNotFoundException) {
				throw;
			}
			catch { }

			// Return the map wrapped
			return new DefaultMergeMapFactory(
				sourceType, destinationType,
				(source, destination) => mapper.Map(source, sourceType, destination, destinationType, mappingOptions));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapMergeFactory(IMapper, Type, Type, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
			try { 
				return new DisposableMergeMapFactory<TSource, TDestination>((source, destination) => (TDestination)factory.Invoke(source, destination), factory);
			}
			catch {
				factory.Dispose();
				throw;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="MapMergeFactory{TSource, TDestination}(IMapper, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		#region MatchMapDelegate
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

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));

			return mapper.MapMergeFactory<IEnumerable<TSourceElement>, ICollection<TDestinationElement>>(
				(mappingOptions ?? MappingOptions.Empty).AddMergeCollectionMatchers(DelegateMatcher.Create(matcher)));
		}


		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		#region IEqualityComparer
		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/summary"/>
		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/remarks"/>
		/// <typeparam name="TElement">
		/// Type of the source and destination elements, used to retrieve the available maps.
		/// </typeparam>
		/// <param name="comparer">
		/// Comparer to be used to match elements of the source and destination collections.
		/// </param>
		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/returns"/>
		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions)" path="/exception"/>
		public static MergeMapFactory<
			IEnumerable<TElement>,
			ICollection<TElement>>
				MapMergeFactory<TElement>(this IMapper mapper,
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

			return mapper.MapMergeFactory<IEnumerable<TElement>, ICollection<TElement>>(
				(mappingOptions ?? MappingOptions.Empty).AddMergeCollectionMatchers(EqualityComparerMatcher.Create(comparer)));
		}

		/// <inheritdoc cref="MapMergeFactory{TElement}(IMapper, IEqualityComparer{TElement}, MappingOptions)"/>
		public static MergeMapFactory<
			IEnumerable<TElement>,
			ICollection<TElement>>
				MapMergeFactory<TElement>(this IMapper mapper,
			IEqualityComparer<TElement> comparer,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			if (comparer == null)
				throw new ArgumentNullException(nameof(comparer));

			return mapper.MapMergeFactory<TElement>(comparer, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapMergeFactory{TElement}(IMapper, IEqualityComparer{TElement}, MappingOptions)"/>
		public static MergeMapFactory<
			IEnumerable<TElement>,
			ICollection<TElement>>
				MapMergeFactory<TElement>(this IMapper mapper,
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

			return mapper.MapMergeFactory<TElement>(comparer, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion
		#endregion


		#region GetNewMaps
		/// <summary>
		/// Retrieves a collection of type pairs which can be mapped to create new objects, will check
		/// if the given mapper supports <see cref="IMapperMaps"/> otherwise will return an empty result.
		/// It does not guarantee that the actual maps will succeed.
		/// </summary>
		/// <inheritdoc cref="IMapperMaps.GetNewMaps(MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetNewMaps(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (mapper is IMapperMaps maps)
				return maps.GetNewMaps(mappingOptions);
			else
				return Enumerable.Empty<(Type, Type)>();
		}

		/// <inheritdoc cref="GetNewMaps(IMapper, MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetNewMaps(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			if (mapper is IMapperMaps maps)
				return maps.GetNewMaps(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
			else
				return Enumerable.Empty<(Type, Type)>();
		}

		/// <inheritdoc cref="GetNewMaps(IMapper, MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetNewMaps(this IMapper mapper,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			if (mapper is IMapperMaps maps)
				return maps.GetNewMaps(mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
			else
				return Enumerable.Empty<(Type, Type)>();
		}
		#endregion

		#region GetMergeMaps
		/// <summary>
		/// Retrieves a collection of type pairs which can be mapped to merge objects, will check
		/// if the given mapper supports <see cref="IMapperMaps"/> otherwise will return an empty result.
		/// It does not guarantee that the actual maps will succeed.
		/// </summary>
		/// <inheritdoc cref="IMapperMaps.GetMergeMaps(MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetMergeMaps(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (mapper is IMapperMaps maps)
				return maps.GetMergeMaps(mappingOptions);
			else
				return Enumerable.Empty<(Type, Type)>();
		}

		/// <inheritdoc cref="GetMergeMaps(IMapper, MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetMergeMaps(this IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			if (mapper is IMapperMaps maps)
				return maps.GetMergeMaps(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
			else
				return Enumerable.Empty<(Type, Type)>();
		}

		/// <inheritdoc cref="GetMergeMaps(IMapper, MappingOptions)"/>
		public static IEnumerable<(Type From, Type To)> GetMergeMaps(this IMapper mapper,
			params
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?[]?
#else
			object[]
#endif
			mappingOptions) {

			if (mapper is IMapperMaps maps)
				return maps.GetMergeMaps(mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
			else
				return Enumerable.Empty<(Type, Type)>();
		}
		#endregion
	}
}
