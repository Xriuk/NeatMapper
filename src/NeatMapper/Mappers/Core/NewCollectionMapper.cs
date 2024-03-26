using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which creates a new collection (derived from <see cref="ICollection{T}"/>
	/// plus some special types like below), even nested and readonly, from a <see cref="IEnumerable{T}"/>.<br/>
	/// Elements are then mapped with another <see cref="IMapper"/> by trying new map first, then merge map.<br/>
	/// Special collections which can be created are:
	/// <list type="bullet">
	/// <item><see cref="Stack{T}"/></item>
	/// <item><see cref="Queue{T}"/></item>
	/// <item><see cref="string"/> (considered as a collection of <see cref="char"/>s)</item>
	/// </list>
	/// Collections are NOT mapped lazily, all source elements are evaluated during the map.
	/// </summary>
	public sealed class NewCollectionMapper : CollectionMapper, IMapperCanMap, IMapperFactory {
		/// <inheritdoc cref="NewCollectionMapper(IMapper)"/>
		[Obsolete("serviceProvider parameter is no longer used and will be removed in future versions, use other overloads.")]
		public NewCollectionMapper(
			IMapper elementsMapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider) :
				base(elementsMapper) { }

		/// <summary>
		/// Creates a new instance of <see cref="NewCollectionMapper"/>.
		/// </summary>
		/// <param name="elementsMapper">
		/// <see cref="IMapper"/> to use to map collection elements.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions.Mapper"/>.
		/// </param>
		public NewCollectionMapper(
			IMapper elementsMapper) : base(elementsMapper) { }
		

		#region IMapper methods
		override public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			Map(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			using (var factory = MapNewFactory(sourceType, destinationType, mappingOptions)) {
				return factory.Invoke(source);
			}
		}

		override public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			Map(
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
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			// Not mapping merge
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion

		#region IMapperCanMap methods
		public bool CanMapNew(
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsEnumerable() && destinationType.IsEnumerable() && ObjectFactory.CanCreateCollection(destinationType)) {
				var elementTypes = (From: sourceType.GetEnumerableElementType(), To: destinationType.GetEnumerableElementType());

				mappingOptions = MergeOrCreateMappingOptions(mappingOptions, out _);
				var elementsMapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

				bool cannotVerifyNew = false;
				try {
					if(elementsMapper.CanMapNew(elementTypes.From, elementTypes.To, mappingOptions))
						return true;
				}
				catch(InvalidOperationException) {
					cannotVerifyNew = true;
				}

				if (ObjectFactory.CanCreate(elementTypes.To) && elementsMapper.CanMapMerge(elementTypes.From, elementTypes.To, mappingOptions))
					return true;
				else if(cannotVerifyNew)
					throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
			}

			return false;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public bool CanMapMerge(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return false;
		}
		#endregion

		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			// If both types are collections try mapping the element types
			if (types.From.IsEnumerable() && types.To.IsEnumerable() && ObjectFactory.CanCreateCollection(types.To)) {
				var elementTypes = (From: types.From.GetEnumerableElementType(), To: types.To.GetEnumerableElementType());

				// Retrieve the factory which we will use to create instances of the collection and the actual type
				// which will be used, eg: to create an array we create a List<T> first, which will be later
				// converted to the desired array
				Func<object> collectionFactory;
				Type actualCollectionType;
				try {
					collectionFactory = ObjectFactory.CreateCollectionFactory(types.To, out actualCollectionType);
				}
				catch (ObjectCreationException) {
					throw new MapNotFoundException(types);
				}

				// Since the method above is not 100% accurate in checking if the type is an actual collection
				// we check again here, if we do not get back a method to add elements then it is not a collection
				Action<object, object> addDelegate;
				try { 
					addDelegate = ObjectFactory.GetCollectionCustomAddDelegate(actualCollectionType);
				}
				catch(InvalidOperationException){
					throw new MapNotFoundException(types);
				}

				var collectionConversionDelegate = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType, types.To);

				mappingOptions = MergeOrCreateMappingOptions(mappingOptions, out _);
				var elementsMapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

				// At least one of New or Merge mapper is required to map elements
				// If both are found they will be used in the following order: NewMap then MergeMap
				INewMapFactory newElementsFactory;
				try {
					newElementsFactory = elementsMapper.MapNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
				}
				catch (MapNotFoundException) {
					newElementsFactory = null;
				}

				try { 
					INewMapFactory mergeElementsFactory;
					try {
						IMergeMapFactory mergeFactory;
						try {
							mergeFactory = elementsMapper.MapMergeFactory(elementTypes.From, elementTypes.To, mappingOptions);
						}
						catch (MapNotFoundException) {
							throw new MapNotFoundException(types);
						}

						try { 
							Func<object> destinationFactory;
							try {
								destinationFactory = ObjectFactory.CreateFactory(elementTypes.To);
							}
							catch (ObjectCreationException) {
								throw new MapNotFoundException(types);
							}
							mergeElementsFactory = new DisposableNewMapFactory(elementTypes.From, elementTypes.To, s => mergeFactory.Invoke(s, destinationFactory.Invoke()), mergeFactory);
						}
						catch {
							mergeFactory?.Dispose();
							throw;
						}
					}
					catch (MapNotFoundException) {
						if(newElementsFactory == null)
							throw;
						else
							mergeElementsFactory = null;
					}

					try { 
						return new DisposableNewMapFactory(
							sourceType, destinationType,
							source => {
								TypeUtils.CheckObjectType(source, types.From, nameof(source));

								if (source is IEnumerable sourceEnumerable) {
									var canNew = newElementsFactory != null;

									object result;
									try {
										var destination = collectionFactory.Invoke();

										foreach (var sourceElement in sourceEnumerable) {
											// Try new map
											if (canNew) {
												try {
													addDelegate.Invoke(destination, newElementsFactory.Invoke(sourceElement));
													continue;
												}
												catch (MapNotFoundException) {
													canNew = false;
												}
											}

											// Try merge map
											if(mergeElementsFactory == null)
												throw new MapNotFoundException(types);
											try {
												addDelegate.Invoke(destination, mergeElementsFactory.Invoke(sourceElement));
											}
											catch (MapNotFoundException) {
												throw new MapNotFoundException(types);
											}
										}

										result = collectionConversionDelegate.Invoke(destination);
									}
									catch (MapNotFoundException) {
										throw;
									}
									catch (TaskCanceledException) {
										throw;
									}
									catch (Exception e) {
										throw new MappingException(e, types);
									}

									// Should not happen
									TypeUtils.CheckObjectType(result, types.To);

									return result;
								}
								else if (source == null) { 
									try {
										if (elementsMapper.CanMapNew(elementTypes.From, elementTypes.To, mappingOptions))
											return null;
									}
									catch { }

									try {
										if (elementsMapper.CanMapMerge(elementTypes.From, elementTypes.To, mappingOptions) && ObjectFactory.CanCreate(elementTypes.To))
											return null;
									}
									catch { }

									throw new MapNotFoundException(types);
								}
								else
									throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
							},
							newElementsFactory, mergeElementsFactory);
					}
					catch {
						mergeElementsFactory?.Dispose();
						throw;
					}
				}
				catch {
					newElementsFactory?.Dispose();
					throw;
				}
			}
					
			throw new MapNotFoundException(types);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public IMergeMapFactory MapMergeFactory(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			// Not mapping merge
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion
	}
}
