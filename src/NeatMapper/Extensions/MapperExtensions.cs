using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	public static class MapperExtensions {
		#region CanMapNew
		#region Runtime
		/// <inheritdoc cref="IMapper.CanMapNew(Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapNew(this IMapper mapper,Type sourceType, Type destinationType, IEnumerable? mappingOptions) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapNew(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="IMapper.CanMapNew(Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapNew(this IMapper mapper, Type sourceType, Type destinationType, params object?[]? mappingOptions) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapNew(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IMapper.CanMapNew(Type, Type, MappingOptions?)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapper.CanMapNew(Type, Type, MappingOptions?)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapper.CanMapNew(Type, Type, MappingOptions?)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapper.CanMapNew(Type, Type, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be created
		/// from a parameter of type <typeparamref name="TSource"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapNew<TSource, TDestination>(this IMapper mapper, MappingOptions? mappingOptions = null) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapNew(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="CanMapNew{TSource, TDestination}(IMapper, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapNew<TSource, TDestination>(this IMapper mapper, IEnumerable? mappingOptions) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapNew(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="CanMapNew{TSource, TDestination}(IMapper, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapNew<TSource, TDestination>(this IMapper mapper, params object?[]? mappingOptions) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapNew(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region CanMapMerge
		#region Runtime
		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapMerge(this IMapper mapper, Type sourceType, Type destinationType, IEnumerable? mappingOptions) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapMerge(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapMerge(this IMapper mapper, Type sourceType, Type destinationType, params object?[]? mappingOptions) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapMerge(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions?)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions?)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions?)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapper.CanMapMerge(Type, Type, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be merged into an object
		/// of type <typeparamref name="TDestination"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapMerge<TSource, TDestination>(this IMapper mapper, MappingOptions? mappingOptions = null) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapMerge(typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="CanMapMerge{TSource, TDestination}(IMapper, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapMerge<TSource, TDestination>(this IMapper mapper, IEnumerable? mappingOptions) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapMerge(typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="CanMapMerge{TSource, TDestination}(IMapper, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanMapMerge<TSource, TDestination>(this IMapper mapper, params object?[]? mappingOptions) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.CanMapMerge(typeof(TSource), typeof(TDestination), mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region NewMap
		#region Runtime
		/// <inheritdoc cref="IMapper.Map(object?, Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object? Map(this IMapper mapper, object? source, Type sourceType, Type destinationType, IEnumerable? mappingOptions) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.Map(source, sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="IMapper.Map(object?, Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object? Map(this IMapper mapper, object? source, Type sourceType, Type destinationType, params object?[]? mappingOptions) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.Map(source, sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit destination, inferred source
		/// <inheritdoc cref="IMapper.Map(object?, Type, Type, MappingOptions?)" path="/summary"/>
		/// <typeparam name="TDestination">
		/// <inheritdoc cref="IMapper.Map(object?, Type, Type, MappingOptions?)" path="/param[@name='destinationType']"/>
		/// </typeparam>
		/// <param name="source">
		/// Object to map, CANNOT be null as the source type will be retrieved from it at runtime,
		/// which will be used to retrieve the available maps.
		/// </param>
		/// <inheritdoc cref="IMapper.Map(object?, Type, Type, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <returns>The newly created object, may be null.</returns>
		/// <inheritdoc cref="IMapper.Map(object?, Type, Type, MappingOptions?)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TDestination? Map<TDestination>(this IMapper mapper, object source, MappingOptions? mappingOptions = null) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (source == null)
				throw new ArgumentNullException(nameof(source), "Type cannot be inferred from null source, use an overload with an explicit source type");

			return (TDestination?)mapper.Map(source, source.GetType(), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="Map{TDestination}(IMapper, object?, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TDestination? Map<TDestination>(this IMapper mapper, object source, IEnumerable? mappingOptions) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (source == null)
				throw new ArgumentNullException(nameof(source), "Type cannot be inferred from null source, use an overload with an explicit source type");

			return (TDestination?)mapper.Map(source, source.GetType(), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		// No params overload because it may become problematic if people try to use it to merge types
		// like: mapper.Map<TDestination>(source, destination), in this case destination is actually passed
		// to mappingOptions
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IMapper.Map(object?, Type, Type, MappingOptions?)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapper.Map(object?, Type, Type, MappingOptions?)" path="/param[@name='sourceType']"/></typeparam>
		/// <inheritdoc cref="Map{TDestination}(IMapper, object?, MappingOptions?)" path="/typeparam[@name='TDestination']"/>
		/// <inheritdoc cref="IMapper.Map(object?, Type, Type, MappingOptions?)" path="/param[@name='source']"/>
		/// <inheritdoc cref="IMapper.Map(object?, Type, Type, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <returns>The newly created object, may be null.</returns>
		/// <inheritdoc cref="IMapper.Map(object?, Type, Type, MappingOptions?)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS1712
		public static TDestination? Map<TSource, TDestination>(this IMapper mapper, TSource? source, MappingOptions? mappingOptions = null) {
#pragma warning restore CS1712

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return (TDestination?)mapper.Map(source, typeof(TSource), typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="Map{TSource, TDestination}(IMapper, TSource, MappingOptions?)" />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TDestination? Map<TSource, TDestination>(this IMapper mapper, TSource? source, IEnumerable? mappingOptions) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return (TDestination?)mapper.Map(source, typeof(TSource), typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		// No params overload because it overlaps with merge map with explicit source and destination:
		// mapper.Map<TSource, TDestination>(source, destination, option1, ...), in this case destination
		// is actually passed to mappingOptions
		#endregion
		#endregion

		#region MergeMap
		#region Runtime
		/// <inheritdoc cref="IMapper.Map(object?, Type, object?, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object? Map(this IMapper mapper,
			object? source,
			Type sourceType,
			object? destination,
			Type destinationType,
			IEnumerable? mappingOptions) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.Map(source, sourceType, destination, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="IMapper.Map(object?, Type, object?, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object? Map(this IMapper mapper,
			object? source,
			Type sourceType,
			object? destination,
			Type destinationType,
			params object?[]? mappingOptions) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return mapper.Map(source, sourceType, destination, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="IMapper.Map(object?, Type, object?, Type, MappingOptions?)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapper.Map(object?, Type, object?, Type, MappingOptions?)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapper.Map(object?, Type, object?, Type, MappingOptions?)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapper.Map(object?, Type, object?, Type, MappingOptions?)" path="/param[@name='source']"/>
		/// <inheritdoc cref="IMapper.Map(object?, Type, object?, Type, MappingOptions?)" path="/param[@name='destination']"/>
		/// <inheritdoc cref="IMapper.Map(object?, Type, object?, Type, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// The resulting object of the mapping, can be <paramref name="destination"/> or a new one,
		/// may be null.
		/// </returns>
		/// <inheritdoc cref="IMapper.Map(object?, Type, object?, Type, MappingOptions?)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TDestination? Map<TSource, TDestination>(this IMapper mapper,
			TSource? source,
			TDestination? destination,
			MappingOptions? mappingOptions = null) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return (TDestination?)mapper.Map(source, typeof(TSource), destination, typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="Map{TSource, TDestination}(IMapper, TSource, TDestination, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TDestination? Map<TSource, TDestination>(this IMapper mapper,
			TSource? source,
			TDestination? destination,
			IEnumerable? mappingOptions) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return (TDestination?)mapper.Map(source, typeof(TSource), destination, typeof(TDestination), mappingOptions != null ? new MappingOptions(mappingOptions) : null);
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
		/// <inheritdoc cref="IMapper.Map(object?, Type, object?, Type, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// The resulting collection of the mapping, can be <paramref name="destination"/> or a new one,
		/// may be null.
		/// </returns>
		/// <inheritdoc cref="IMapper.Map(object?, Type, object?, Type, MappingOptions?)" path="/exception"/>
		/// <exception cref="InvalidOperationException"><paramref name="destination"/> is a readonly collection.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ICollection<TDestinationElement>? Map<TSourceElement, TDestinationElement>(this IMapper mapper,
			IEnumerable<TSourceElement>? source,
			ICollection<TDestinationElement>? destination,
			MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
			MappingOptions? mappingOptions = null) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));

			return mapper.Map<IEnumerable<TSourceElement>, ICollection<TDestinationElement>>(source, destination,
				(mappingOptions ?? MappingOptions.Empty).AddMergeCollectionMatchers(DelegateMatcher.Create(matcher)));
		}

		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ICollection<TDestinationElement>? Map<TSourceElement, TDestinationElement>(this IMapper mapper,
			IEnumerable<TSourceElement>? source,
			ICollection<TDestinationElement>? destination,
			MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
			IEnumerable? mappingOptions) {

			return mapper.Map<TSourceElement, TDestinationElement>(source, destination, matcher, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ICollection<TDestinationElement>? Map<TSourceElement, TDestinationElement>(this IMapper mapper,
			IEnumerable<TSourceElement>? source,
			ICollection<TDestinationElement>? destination,
			MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
			params object?[]? mappingOptions) {

			return mapper.Map<TSourceElement, TDestinationElement>(source, destination, matcher, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region IEqualityComparer
		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)" path="/summary"/>
		/// <typeparam name="TElement">
		/// Type of the source and destination elements, used to retrieve the available maps.
		/// </typeparam>
		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)" path="/param[@name='source']"/>
		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)" path="/param[@name='destination']"/>
		/// <param name="comparer">
		/// Comparer to be used to match elements of the <paramref name="source"/>
		/// and <paramref name="destination"/> collections.
		/// </param>
		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)" path="/returns"/>
		/// <inheritdoc cref="Map{TSourceElement, TDestinationElement}(IMapper, IEnumerable{TSourceElement}, ICollection{TDestinationElement}, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ICollection<TElement>? Map<TElement>(this IMapper mapper,
			IEnumerable<TElement>? source,
			ICollection<TElement>? destination,
			IEqualityComparer<TElement> comparer,
			MappingOptions? mappingOptions = null) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (comparer == null)
				throw new ArgumentNullException(nameof(comparer));

			return mapper.Map<IEnumerable<TElement>, ICollection<TElement>>(source, destination,
				(mappingOptions ?? MappingOptions.Empty).AddMergeCollectionMatchers(EqualityComparerMatcher.Create(comparer)));
		}

		/// <inheritdoc cref="Map{TElement}(IMapper, IEnumerable{TElement}, ICollection{TElement}, IEqualityComparer{TElement}, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ICollection<TElement>? Map<TElement>(this IMapper mapper,
			IEnumerable<TElement>? source,
			ICollection<TElement>? destination,
			IEqualityComparer<TElement> comparer,
			IEnumerable? mappingOptions) {

			return mapper.Map<TElement>(source, destination, comparer, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Map{TElement}(IMapper, IEnumerable{TElement}, ICollection{TElement}, IEqualityComparer{TElement}, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ICollection<TElement>? Map<TElement>(this IMapper mapper,
			IEnumerable<TElement>? source,
			ICollection<TElement>? destination,
			IEqualityComparer<TElement> comparer,
			params object?[]? mappingOptions) {

			return mapper.Map<TElement>(source, destination, comparer, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion
		#endregion


		#region MapNewFactory
		#region Runtime
		/// <summary>
		/// Creates a factory which can be used to map objects to new ones, will check if the given mapper supports
		/// <see cref="IMapperFactory"/> first otherwise will return
		/// <see cref="IMapper.Map(object?, Type, Type, MappingOptions?)"/> wrapped in a delegate.
		/// </summary>
		/// <remarks>
		/// If the mapper does not implement <see cref="IMapperFactory"/> it is NOT guaranteed
		/// that the created factory shares the same <see cref="MappingContext"/>.
		/// </remarks>
		/// <inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions?)"/>
		public static INewMapFactory MapNewFactory(this IMapper mapper, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Check if the mapper implements IMapperFactory
			if (mapper is IMapperFactory mapperFactory)
				return mapperFactory.MapNewFactory(sourceType, destinationType, mappingOptions);

			if (!mapper.CanMapNew(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			// Return the map wrapped
			return new DefaultNewMapFactory(
				sourceType, destinationType,
				source => mapper.Map(source, sourceType, destinationType, mappingOptions));
		}

		/// <inheritdoc cref="MapNewFactory(IMapper, Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static INewMapFactory MapNewFactory(this IMapper mapper, Type sourceType, Type destinationType, IEnumerable? mappingOptions) {
			return mapper.MapNewFactory(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapNewFactory(IMapper, Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static INewMapFactory MapNewFactory(this IMapper mapper, Type sourceType, Type destinationType, params object?[]? mappingOptions) {
			return mapper.MapNewFactory(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="MapNewFactory(IMapper, Type, Type, MappingOptions?)" path="/summary"/>
		/// <inheritdoc cref="MapNewFactory(IMapper, Type, Type, MappingOptions?)" path="/remarks"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions?)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions?)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A factory which can be used to map objects of type <typeparamref name="TSource"/> into new objects
		/// of type <typeparamref name="TDestination"/>.
		/// </returns>
		/// <inheritdoc cref="IMapperFactory.MapNewFactory(Type, Type, MappingOptions?)" path="/exception"/>
		public static NewMapFactory<TSource, TDestination> MapNewFactory<TSource, TDestination>(this IMapper mapper,
			MappingOptions? mappingOptions = null) {
			
			var factory = mapper.MapNewFactory(typeof(TSource), typeof(TDestination), mappingOptions);
			try { 
				return new DisposableNewMapFactory<TSource, TDestination>(source => (TDestination?)factory.Invoke(source), factory);
			}
			catch {
				factory.Dispose();
				throw;
			}
		}

		/// <inheritdoc cref="MapNewFactory{TSource, TDestination}(IMapper, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NewMapFactory<TSource, TDestination> MapNewFactory<TSource, TDestination>(this IMapper mapper,
			IEnumerable? mappingOptions) {

			return mapper.MapNewFactory<TSource, TDestination>(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapNewFactory{TSource, TDestination}(IMapper, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NewMapFactory<TSource, TDestination> MapNewFactory<TSource, TDestination>(this IMapper mapper,
			params object?[]? mappingOptions) {

			return mapper.MapNewFactory<TSource, TDestination>(mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion

		#region MapMergeFactory
		#region Runtime
		/// <summary>
		/// Creates a factory which can be used to map objects to existing ones, will check if the given mapper supports
		/// <see cref="IMapperFactory"/> first otherwise will return
		/// <see cref="IMapper.Map(object?, Type, object?, Type, MappingOptions?)"/> wrapped in a delegate.
		/// </summary>
		/// <remarks>
		/// If the mapper does not implement <see cref="IMapperFactory"/> it is NOT guaranteed
		/// that the created factory shares the same <see cref="MappingContext"/>.
		/// </remarks>
		/// <inheritdoc cref="IMapperFactory.MapMergeFactory(Type, Type, MappingOptions?)"/>
		public static IMergeMapFactory MapMergeFactory(this IMapper mapper, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Check if the mapper implements IMapperFactory
			if (mapper is IMapperFactory mapperFactory)
				return mapperFactory.MapMergeFactory(sourceType, destinationType, mappingOptions);

			if (!mapper.CanMapMerge(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			// Return the map wrapped
			return new DefaultMergeMapFactory(
				sourceType, destinationType,
				(source, destination) => mapper.Map(source, sourceType, destination, destinationType, mappingOptions));
		}

		/// <inheritdoc cref="MapMergeFactory(IMapper, Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IMergeMapFactory MapMergeFactory(this IMapper mapper, Type sourceType, Type destinationType, IEnumerable? mappingOptions) {
			return mapper.MapMergeFactory(sourceType, destinationType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapMergeFactory(IMapper, Type, Type, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IMergeMapFactory MapMergeFactory(this IMapper mapper, Type sourceType, Type destinationType, params object?[]? mappingOptions) {
			return mapper.MapMergeFactory(sourceType, destinationType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="MapMergeFactory(IMapper, Type, Type, MappingOptions?)" path="/summary"/>
		/// <inheritdoc cref="MapMergeFactory(IMapper, Type, Type, MappingOptions?)" path="/remarks"/>
		/// <typeparam name="TSource"><inheritdoc cref="IMapperFactory.MapMergeFactory(Type, Type, MappingOptions?)" path="/param[@name='sourceType']"/></typeparam>
		/// <typeparam name="TDestination"><inheritdoc cref="IMapperFactory.MapMergeFactory(Type, Type, MappingOptions?)" path="/param[@name='destinationType']"/></typeparam>
		/// <inheritdoc cref="IMapperFactory.MapMergeFactory(Type, Type, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A factory which can be used to map objects of type <typeparamref name="TSource"/> into existing objects
		/// of type <typeparamref name="TDestination"/>.
		/// </returns>
		/// <inheritdoc cref="IMapperFactory.MapMergeFactory(Type, Type, MappingOptions?)" path="/exception"/>
		public static MergeMapFactory<TSource, TDestination> MapMergeFactory<TSource, TDestination>(this IMapper mapper,
			MappingOptions? mappingOptions = null) {

			var factory = mapper.MapMergeFactory(typeof(TSource), typeof(TDestination), mappingOptions);
			try { 
				return new DisposableMergeMapFactory<TSource, TDestination>((source, destination) => (TDestination?)factory.Invoke(source, destination), factory);
			}
			catch {
				factory.Dispose();
				throw;
			}
		}

		/// <inheritdoc cref="MapMergeFactory{TSource, TDestination}(IMapper, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MergeMapFactory<TSource, TDestination> MapMergeFactory<TSource, TDestination>(this IMapper mapper,
			IEnumerable? mappingOptions) {

			return mapper.MapMergeFactory<TSource, TDestination>(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapMergeFactory{TSource, TDestination}(IMapper, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MergeMapFactory<TSource, TDestination> MapMergeFactory<TSource, TDestination>(this IMapper mapper,
			params object?[]? mappingOptions) {

			return mapper.MapMergeFactory<TSource, TDestination>(mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Collection
		#region MatchMapDelegate
		/// <summary>
		/// Creates a factory which can be used to map collections to existing ones by matching the elements,
		/// will check if the given mapper supports <see cref="IMapperFactory"/> first otherwise will return
		/// <see cref="IMapper.Map(object?, Type, object?, Type, MappingOptions?)"/> wrapped in a delegate.
		/// </summary>
		/// <inheritdoc cref="MapMergeFactory(IMapper, Type, Type, MappingOptions?)" path="/remarks"/>
		/// <typeparam name="TSourceElement">
		/// Type of the elements to be mapped, used to retrieve the available maps.
		/// </typeparam>
		/// <typeparam name="TDestinationElement">
		/// Type of the destination elements, used to retrieve the available maps.
		/// </typeparam>
		/// <param name="matcher">
		/// Matching method to be used to match elements of the source and destination collections.
		/// </param>
		/// <inheritdoc cref="IMapperFactory.MapMergeFactory(Type, Type, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// A factory which can be used to map collections of type <typeparamref name="TSourceElement"/> into existing
		/// collections of type <typeparamref name="TDestinationElement"/>.
		/// </returns>
		/// <inheritdoc cref="IMapperFactory.MapMergeFactory(Type, Type, MappingOptions?)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MergeMapFactory<IEnumerable<TSourceElement>, ICollection<TDestinationElement>>
			MapMergeFactory<TSourceElement, TDestinationElement>(this IMapper mapper,
				MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
				MappingOptions? mappingOptions = null) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (matcher == null)
				throw new ArgumentNullException(nameof(matcher));

			return mapper.MapMergeFactory<IEnumerable<TSourceElement>, ICollection<TDestinationElement>>(
				(mappingOptions ?? MappingOptions.Empty).AddMergeCollectionMatchers(DelegateMatcher.Create(matcher)));
		}


		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MergeMapFactory<IEnumerable<TSourceElement>, ICollection<TDestinationElement>>
			MapMergeFactory<TSourceElement, TDestinationElement>(this IMapper mapper,
				MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
				IEnumerable? mappingOptions) {

			return mapper.MapMergeFactory<TSourceElement, TDestinationElement>(matcher, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MergeMapFactory<IEnumerable<TSourceElement>, ICollection<TDestinationElement>>
			MapMergeFactory<TSourceElement, TDestinationElement>(this IMapper mapper,
				MatchMapDelegate<TSourceElement, TDestinationElement> matcher,
				params object?[]? mappingOptions) {

			return mapper.MapMergeFactory<TSourceElement, TDestinationElement>(matcher, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region IEqualityComparer
		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)" path="/summary"/>
		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)" path="/remarks"/>
		/// <typeparam name="TElement">
		/// Type of the source and destination elements, used to retrieve the available maps.
		/// </typeparam>
		/// <param name="comparer">
		/// Comparer to be used to match elements of the source and destination collections.
		/// </param>
		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)" path="/returns"/>
		/// <inheritdoc cref="MapMergeFactory{TSourceElement, TDestinationElement}(IMapper, MatchMapDelegate{TSourceElement, TDestinationElement}, MappingOptions?)" path="/exception"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MergeMapFactory<IEnumerable<TElement>, ICollection<TElement>>
			MapMergeFactory<TElement>(this IMapper mapper,
				IEqualityComparer<TElement> comparer,
				MappingOptions? mappingOptions = null) {

			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (comparer == null)
				throw new ArgumentNullException(nameof(comparer));

			return mapper.MapMergeFactory<IEnumerable<TElement>, ICollection<TElement>>(
				(mappingOptions ?? MappingOptions.Empty).AddMergeCollectionMatchers(EqualityComparerMatcher.Create(comparer)));
		}

		/// <inheritdoc cref="MapMergeFactory{TElement}(IMapper, IEqualityComparer{TElement}, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MergeMapFactory<IEnumerable<TElement>, ICollection<TElement>>
			MapMergeFactory<TElement>(this IMapper mapper,
				IEqualityComparer<TElement> comparer,
				IEnumerable? mappingOptions) {

			return mapper.MapMergeFactory<TElement>(comparer, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="MapMergeFactory{TElement}(IMapper, IEqualityComparer{TElement}, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MergeMapFactory<IEnumerable<TElement>, ICollection<TElement>>
			MapMergeFactory<TElement>(this IMapper mapper,
				IEqualityComparer<TElement> comparer,
				params object?[]? mappingOptions) {

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
		/// <inheritdoc cref="IMapperMaps.GetNewMaps(MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<(Type From, Type To)> GetNewMaps(this IMapper mapper, MappingOptions? mappingOptions = null) {
			if (mapper is IMapperMaps maps)
				return maps.GetNewMaps(mappingOptions);
			else
				return Enumerable.Empty<(Type, Type)>();
		}

		/// <inheritdoc cref="GetNewMaps(IMapper, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<(Type From, Type To)> GetNewMaps(this IMapper mapper, IEnumerable? mappingOptions) {
			if (mapper is IMapperMaps maps)
				return maps.GetNewMaps(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
			else
				return Enumerable.Empty<(Type, Type)>();
		}

		/// <inheritdoc cref="GetNewMaps(IMapper, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<(Type From, Type To)> GetNewMaps(this IMapper mapper, params object?[]? mappingOptions) {
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
		/// <inheritdoc cref="IMapperMaps.GetMergeMaps(MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<(Type From, Type To)> GetMergeMaps(this IMapper mapper, MappingOptions? mappingOptions = null) {
			if (mapper is IMapperMaps maps)
				return maps.GetMergeMaps(mappingOptions);
			else
				return Enumerable.Empty<(Type, Type)>();
		}

		/// <inheritdoc cref="GetMergeMaps(IMapper, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<(Type From, Type To)> GetMergeMaps(this IMapper mapper, IEnumerable? mappingOptions) {
			if (mapper is IMapperMaps maps)
				return maps.GetMergeMaps(mappingOptions != null ? new MappingOptions(mappingOptions) : null);
			else
				return Enumerable.Empty<(Type, Type)>();
		}

		/// <inheritdoc cref="GetMergeMaps(IMapper, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<(Type From, Type To)> GetMergeMaps(this IMapper mapper, params object?[]? mappingOptions) {
			if (mapper is IMapperMaps maps)
				return maps.GetMergeMaps(mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
			else
				return Enumerable.Empty<(Type, Type)>();
		}
		#endregion
	}
}
