using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which creates a new collection (even nested) and maps elements with another
	/// <see cref="IMapper"/> by trying new map first, then merge map
	/// </summary>
	public sealed class NewCollectionMapper : CustomCollectionMapper, IMapperCanMap {
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
			IEnumerable?
#else
			IEnumerable
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
			if (TypeUtils.HasInterface(types.From, typeof(IEnumerable<>)) && types.From != typeof(string) &&
				TypeUtils.HasInterface(types.To, typeof(IEnumerable<>)) && types.To != typeof(string)) {

				var elementTypes = (
					From: TypeUtils.GetInterfaceElementType(types.From, typeof(IEnumerable<>)),
					To: TypeUtils.GetInterfaceElementType(types.To, typeof(IEnumerable<>))
				);

				// Check if collection can be created
				if (CanCreateCollection(types.To)) {
					try {
						if (source is IEnumerable sourceEnumerable) {
							var destination = CreateCollection(types.To);
							var addMethod = GetCollectionAddMethod(destination);

							// Adjust the context so that we don't pass any merge matcher along
							mappingOptions = mappingOptions?.Cast<object>().Select(o => {
								if (o is MergeCollectionsMappingOptions merge) {
									var mergeOpts = new MergeCollectionsMappingOptions(merge) {
										Matcher = null
									};
									return mergeOpts;
								}
								else
									return o;
							});

							var canCreateNew = true;

							foreach (var element in sourceEnumerable) {
								object destinationElement;

								// Try new map
								if (canCreateNew) {
									try {
										destinationElement = _elementsMapper.Map(element, elementTypes.From, elementTypes.To, mappingOptions);
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
								destinationElement = _elementsMapper.Map(element, elementTypes.From, destinationElement, elementTypes.To, mappingOptions);
								addMethod.Invoke(destination, new object[] { destinationElement });
							}

							var result = ConvertCollectionToType(destination, types.To);

							// Should not happen
							if (result != null && !destinationType.IsAssignableFrom(result.GetType()))
								throw new InvalidOperationException($"Object of type {result.GetType().FullName ?? result.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}");

							return result;
						}
						else if (source == null) {
							// Check if we can map elements
							try {
								if (_elementsMapper.CanMapNew(elementTypes.From, elementTypes.To))
									return null;
							}
							catch { }

							try {
								if (_elementsMapper.CanMapMerge(elementTypes.From, elementTypes.To))
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
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions = null) {
			// Not mapping merge
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion

		#region IMapperCanMap methods
		public bool CanMapNew(Type sourceType, Type destinationType) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (TypeUtils.HasInterface(sourceType, typeof(IEnumerable<>)) && sourceType != typeof(string) &&
				TypeUtils.HasInterface(destinationType, typeof(IEnumerable<>)) && destinationType != typeof(string) &&
				CanCreateCollection(destinationType)) {

				var elementTypes = (
					From: TypeUtils.GetInterfaceElementType(sourceType, typeof(IEnumerable<>)),
					To: TypeUtils.GetInterfaceElementType(destinationType, typeof(IEnumerable<>))
				);

				bool cannotVerifyNew = false;
				try {
					if(_elementsMapper.CanMapNew(elementTypes.From, elementTypes.To))
						return true;
				}
				catch(InvalidOperationException) {
					cannotVerifyNew = true;
				}

				if (_elementsMapper.CanMapMerge(elementTypes.From, elementTypes.To))
					return true;
				else if(cannotVerifyNew)
					throw new InvalidOperationException("Cannot verify if the mapper supports the given map");
			}

			return false;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public bool CanMapMerge(Type sourceType, Type destinationType) {
			return false;
		}
		#endregion
	}
}
