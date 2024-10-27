using System;
using System.Collections;
using System.Collections.Generic;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which creates a new <see cref="IEnumerable{T}"/> (derived from <see cref="ICollection{T}"/>
	/// plus some special types like below), even nested and readonly, from another <see cref="IEnumerable{T}"/>.<br/>
	/// Elements are then mapped with another <see cref="IMapper"/> by trying new map first, then merge map.<br/>
	/// Special collections which can be created are:
	/// <list type="bullet">
	/// <item><see cref="Stack{T}"/></item>
	/// <item><see cref="Queue{T}"/></item>
	/// <item><see cref="string"/> (considered as a collection of <see cref="char"/>s)</item>
	/// </list>
	/// </summary>
	/// <remarks>Collections are NOT mapped lazily, all source elements are evaluated during the map.</remarks>
	public sealed class NewCollectionMapper : CollectionMapper, IMapperFactory {
		/// <summary>
		/// Creates a new instance of <see cref="NewCollectionMapper"/>.
		/// </summary>
		/// <param name="elementsMapper">
		/// <see cref="IMapper"/> to use to map collection elements.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions.Mapper"/>.
		/// </param>
		public NewCollectionMapper(IMapper elementsMapper) : base(elementsMapper) { }


		#region IMapper methods
		override public bool CanMapNew(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return CanMapNewInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
		}

		override public bool CanMapMerge(
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			(Type From, Type To) types = (sourceType, destinationType);

			if(!CanMapNewInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper))
				throw new MapNotFoundException(types);
			
			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			if (source is IEnumerable sourceEnumerable) {
				// At least one of New or Merge mapper is required to map elements
				INewMapFactory elementsFactory;
				try {
					elementsFactory = elementsMapper.MapNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
				}
				catch (MapNotFoundException) {
					try {
						elementsFactory = elementsMapper.MapMergeFactory(elementTypes.From, elementTypes.To, mappingOptions).MapNewFactory();
					}
					catch (MapNotFoundException) {
						throw new MapNotFoundException(types);
					}
				}

				using (elementsFactory) {
					// Create the collection and retrieve the actual type which will be used,
					// eg: to create an array we create a List<T> first, which will be later
					// converted to the desired array
					object destination;
					Type actualCollectionType;
					try {
						destination = ObjectFactory.CreateCollectionFactory(types.To, out actualCollectionType).Invoke();
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
					catch (InvalidOperationException) {
						throw new MapNotFoundException(types);
					}

					try {
						foreach (var sourceElement in sourceEnumerable) {
							addDelegate.Invoke(destination, elementsFactory.Invoke(sourceElement));
						}
					}
					catch (OperationCanceledException) {
						throw;
					}
					catch (Exception e) {
						throw new MappingException(e, types);
					}

					var result = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType, types.To).Invoke(destination);

					// Should not happen
					TypeUtils.CheckObjectType(result, types.To);

					return result;
				}
			}
			else if (source == null) 
				return null;
			else
				throw new InvalidOperationException("Source is not an enumerable"); // Should not happen

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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

			if(!CanMapNewInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var elementsMapper))
				throw new MapNotFoundException(types);

			// At least one of New or Merge mapper is required to map elements
			INewMapFactory elementsFactory;
			try {
				elementsFactory = elementsMapper.MapNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				try {
					elementsFactory = elementsMapper.MapMergeFactory(elementTypes.From, elementTypes.To, mappingOptions).MapNewFactory();
				}
				catch (MapNotFoundException) {
					throw new MapNotFoundException(types);
				}
			}

			try { 
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
				catch (InvalidOperationException) {
					throw new MapNotFoundException(types);
				}

				var collectionConversionDelegate = ObjectFactory.CreateCollectionConversionFactory(actualCollectionType, types.To);

				return new DisposableNewMapFactory(
					sourceType, destinationType,
					source => {
						TypeUtils.CheckObjectType(source, types.From, nameof(source));

						if (source is IEnumerable sourceEnumerable) {
							object result;
							try {
								var destination = collectionFactory.Invoke();

								foreach (var sourceElement in sourceEnumerable) {
									addDelegate.Invoke(destination, elementsFactory.Invoke(sourceElement));
								}

								result = collectionConversionDelegate.Invoke(destination);
							}
							catch (OperationCanceledException) {
								throw;
							}
							catch (Exception e) {
								throw new MappingException(e, types);
							}

							// Should not happen
							TypeUtils.CheckObjectType(result, types.To);

							return result;
						}
						else if (source == null) 
							return null;
						else
							throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
					},
					elementsFactory);
			}
			catch {
				elementsFactory?.Dispose();
				throw;
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


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private bool CanMapNewInternal(
			Type sourceType,
			Type destinationType,
			ref MappingOptions mappingOptions,
			out (Type From, Type To) elementTypes,
			out IMapper elementsMapper) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsEnumerable() && destinationType.IsEnumerable() &&
				ObjectFactory.CanCreateCollection(destinationType)) {

				elementTypes = (sourceType.GetEnumerableElementType(), destinationType.GetEnumerableElementType());
				mappingOptions = _optionsCache.GetOrCreate(mappingOptions);
				elementsMapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

				return elementsMapper.CanMapNew(elementTypes.From, elementTypes.To, mappingOptions) ||
					(ObjectFactory.CanCreate(elementTypes.To) && elementsMapper.CanMapMerge(elementTypes.From, elementTypes.To, mappingOptions));
			}
			else {
				elementTypes = default;
				elementsMapper = null;

				return false;
			}
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
	}
}
