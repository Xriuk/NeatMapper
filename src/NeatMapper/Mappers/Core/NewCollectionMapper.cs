using System;
using System.Collections;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which creates a new <see cref="System.Collections.Generic.IEnumerable{T}"/>
	/// (even nested) from a <see cref="System.Collections.Generic.IEnumerable{T}"/>
	/// and maps elements with another <see cref="IMapper"/> by trying new map first, then merge map.<br/>
	/// Collections are NOT mapped lazily, all elements are evaluated during the map.
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
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions"/>.
		/// </param>
		public NewCollectionMapper(
			IMapper elementsMapper) : base(elementsMapper) { }
		

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private Func<object, object> CreateNewFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions, bool isRealFactory) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			// If both types are collections try mapping the element types
			if (types.From.IsEnumerable() && types.To.IsEnumerable() && ObjectFactory.CanCreateCollection(types.To)) {
				var elementTypes = (From: types.From.GetEnumerableElementType(), To: types.To.GetEnumerableElementType());

				mappingOptions = MergeOrCreateMappingOptions(mappingOptions, isRealFactory, out _);

				var elementsMapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

				// Try new map
				Func<object, object> elementsFactory;
				try {
					elementsFactory = elementsMapper.MapNewFactory(elementTypes.From, elementTypes.To, mappingOptions);
				}
				catch (MapNotFoundException) {
					if (!ObjectFactory.CanCreate(elementTypes.To))
						throw;

					// Try merge map
					Func<object, object, object> mergeFactory;
					try { 
						mergeFactory = elementsMapper.MapMergeFactory(elementTypes.From, elementTypes.To, mappingOptions);
					}
					catch (MapNotFoundException) {
						throw new MapNotFoundException(types);
					}
					Func<object> destinationFactory;
					try { 
						destinationFactory = ObjectFactory.CreateFactory(elementTypes.To);
					}
					catch (ObjectCreationException) {
						throw new MapNotFoundException(types);
					}
					elementsFactory = source => mergeFactory.Invoke(source, destinationFactory.Invoke());
				}

				Func<object> collectionFactory;
				Type actualCollectionType;
				try {
					collectionFactory = ObjectFactory.CreateCollectionFactory(types.To, out actualCollectionType);
				}
				catch (ObjectCreationException) {
					throw new MapNotFoundException(types);
				}
				var addMethod = ObjectFactory.GetCollectionAddMethod(actualCollectionType);
				var collectionConversion = ObjectFactory.CreateCollectionConversionFactory(types.To);
				return source => {
					TypeUtils.CheckObjectType(source, types.From, nameof(source));

					if (source is IEnumerable sourceEnumerable) {
						try {
							var destination = collectionFactory.Invoke();

							foreach (var sourceElement in sourceEnumerable) {
								try {
									var destinationElement = elementsFactory.Invoke(sourceElement);
									addMethod.Invoke(destination, new object[] { destinationElement });
								}
								catch (MapNotFoundException) {
									throw new MapNotFoundException(types);
								}
							}

							var result = collectionConversion.Invoke(destination);

							// Should not happen
							TypeUtils.CheckObjectType(result, types.To);

							return result;
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
					}
					else if (source == null) {
						// Check if we can map elements
						try {
							if (elementsMapper.CanMapNew(elementTypes.From, elementTypes.To, mappingOptions))
								return null;
						}
						catch { }

						try {
							if (elementsMapper.CanMapMerge(elementTypes.From, elementTypes.To, mappingOptions))
								return null;
						}
						catch { }

						throw new MapNotFoundException(types);
					}
					else
						throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
				};
			}

			throw new MapNotFoundException(types);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


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

			return CreateNewFactory(sourceType, destinationType, mappingOptions, false).Invoke(source);
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

				mappingOptions = MergeOrCreateMappingOptions(mappingOptions, false, out _);
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
		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?, object?
#else
			object, object
#endif
			> MapNewFactory(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return CreateNewFactory(sourceType, destinationType, mappingOptions, true);
		}

		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?, object?, object?
#else
			object, object, object
#endif
			> MapMergeFactory(
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
