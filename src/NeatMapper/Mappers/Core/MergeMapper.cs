using System;
using System.Collections;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which maps objects by using <see cref="IMergeMap{TSource, TDestination}"/>
	/// </summary>
	public sealed class MergeMapper : CustomMapper, IMapperCanMap {
		/// <summary>
		/// Creates a new instance of <see cref="MergeMapper"/>.<br/>
		/// At least one between <paramref name="mapsOptions"/> and <paramref name="additionalMapsOptions"/>
		/// should be specified.
		/// </summary>
		/// <param name="mapsOptions">Options to retrieve user-defined maps for the mapper, null to ignore.</param>
		/// <param name="additionalMapsOptions">Additional user-defined maps for the mapper, null to ignore.</param>
		/// <param name="serviceProvider">
		/// Service provider to be passed to the maps inside <see cref="MappingContext"/>, 
		/// null to pass an empty service provider.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions"/>.
		/// </param>
		public MergeMapper(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomMapsOptions?
#else
			CustomMapsOptions
#endif
			mapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomMergeAdditionalMapsOptions?
#else
			CustomMergeAdditionalMapsOptions
#endif
			additionalMapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) :

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			base(new CustomMapsConfiguration(
					(_, i) => {
						if (!i.IsGenericType)
							return false;
						var type = i.GetGenericTypeDefinition();
						return type == typeof(IMergeMap<,>)
#if NET7_0_OR_GREATER
							|| type == typeof(IMergeMapStatic<,>)
#endif
						;
					},
					(mapsOptions ?? new CustomMapsOptions()).TypesToScan,
					additionalMapsOptions?._maps.Values
				),
				serviceProvider) {}

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

			// Forward new map to merge by creating a destination
			object destination;
			try {
				destination = ObjectFactory.Create(destinationType);
			}
			catch (ObjectCreationException) {
				throw new MapNotFoundException((sourceType, destinationType));
			}

			return Map(source, sourceType, destination, destinationType, mappingOptions);
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source != null && !sourceType.IsAssignableFrom(source.GetType()))
				throw new ArgumentException($"Object of type {source.GetType().FullName ?? source.GetType().Name} is not assignable to type {sourceType.FullName ?? sourceType.Name}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));
			if (destination != null && !destinationType.IsAssignableFrom(destination.GetType()))
				throw new ArgumentException($"Object of type {destination.GetType().FullName ?? destination.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}", nameof(destination));

			var result = _configuration.GetMap((sourceType, destinationType)).Invoke(new object[] { source, destination, CreateMappingContext(mappingOptions) });

			// Should not happen
			if (result != null && !destinationType.IsAssignableFrom(result.GetType()))
				throw new InvalidOperationException($"Object of type {result.GetType().FullName ?? result.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}");

			return result;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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

			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return ObjectFactory.CanCreate(destinationType) && CanMapMerge(sourceType, destinationType, mappingOptions);
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			try {
				_configuration.GetMap((sourceType, destinationType));
				return true;
			}
			catch (MapNotFoundException) {
				return false;
			}
		}
		#endregion
	}
}
