using System;
using System.Collections;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which creates a new <see cref="System.Collections.Generic.IEnumerable{T}"/>
	/// (even nested) from a <see cref="System.Collections.Generic.IEnumerable{T}"/>
	/// and maps elements with another <see cref="IMapper"/> by trying new map first, then merge map
	/// </summary>
	public sealed class NewCollectionMapper : CollectionMapper, IMapperCanMap {
		public NewCollectionMapper(
			IMapper elementsMapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) :
			base(elementsMapper, serviceProvider) { }


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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source != null && !sourceType.IsAssignableFrom(source.GetType()))
				throw new ArgumentException($"Object of type {source.GetType().FullName ?? source.GetType().Name} is not assignable to type {sourceType.FullName ?? sourceType.Name}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			(Type From, Type To) types = (sourceType, destinationType);

			// If both types are collections try mapping the element types
			if (types.From.IsEnumerable() && types.To.IsEnumerable()) {
				var elementTypes = (From: types.From.GetEnumerableElementType(), To: types.To.GetEnumerableElementType());

				// Check if collection can be created
				if (ObjectFactory.CanCreateCollection(types.To)) {
					try {
						if (source is IEnumerable sourceEnumerable) {
							var destination = ObjectFactory.CreateCollection(types.To);
							var addMethod = ObjectFactory.GetCollectionAddMethod(destination);

							mappingOptions = MergeOrCreateMappingOptions(mappingOptions, out _);

							var elementsMapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

							var canCreateNew = true;

							foreach (var element in sourceEnumerable) {
								object destinationElement;

								// Try new map
								if (canCreateNew) {
									try {
										destinationElement = elementsMapper.Map(element, elementTypes.From, elementTypes.To, mappingOptions);
										addMethod.Invoke(destination, new object[] { destinationElement });
										continue;
									}
									catch (MapNotFoundException) {
										canCreateNew = false;
									}
								}

								// Try merge map
								try {
									destinationElement = ObjectFactory.Create(elementTypes.To);
								}
								catch (ObjectCreationException) {
									throw new MapNotFoundException(types);
								}
								destinationElement = elementsMapper.Map(element, elementTypes.From, destinationElement, elementTypes.To, mappingOptions);
								addMethod.Invoke(destination, new object[] { destinationElement });
							}

							var result = ObjectFactory.ConvertCollectionToType(destination, types.To);

							// Should not happen
							if (result != null && !destinationType.IsAssignableFrom(result.GetType()))
								throw new InvalidOperationException($"Object of type {result.GetType().FullName ?? result.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}");

							return result;
						}
						else if (source == null) {
							var elementsMapper = mappingOptions?.GetOptions<MapperOverrideMappingOptions>()?.Mapper ?? _elementsMapper;

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
						}
						else
							throw new InvalidOperationException("Source is not an enumerable"); // Should not happen
					}
					catch (MapNotFoundException) {
						throw;
					}
					catch (Exception e) {
						throw new CollectionMappingException(e, types);
					}
				}
			}

			throw new MapNotFoundException(types);

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
	}
}
