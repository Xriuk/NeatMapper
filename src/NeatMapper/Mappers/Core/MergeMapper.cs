using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which maps objects by using <see cref="IMergeMap{TSource, TDestination}"/>.<br/>
	/// Supports both merge and new maps (by creating a destination object and forwarding the calls to merge map
	/// where possible).<br/>
	/// Caches <see cref="MappingContext"/> for each provided <see cref="MappingOptions"/>, so that same options
	/// will share the same context.
	/// </summary>
	public sealed class MergeMapper : CustomMapper, IMapperFactory, IMapperMaps {
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
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions.ServiceProvider"/>.
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
		override public bool CanMapNew(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			// Source type null checked in CanMapMerge
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return ObjectFactory.CanCreate(destinationType) && CanMapMerge(sourceType, destinationType, mappingOptions);
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return _configuration.TryGetDoubleMap<MappingContext>((sourceType, destinationType), out _);
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Forward new map to merge by creating a destination
			if (!ObjectFactory.CanCreate(destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			return Map(source, sourceType, ObjectFactory.Create(destinationType), destinationType, mappingOptions);
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if(!_configuration.TryGetDoubleMap<MappingContext>((sourceType, destinationType), out var map))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			var context = _contextsCache.GetOrCreate(mappingOptions);

			var result = map.Invoke(source, destination, context);

			// Should not happen
			TypeUtils.CheckObjectType(result, destinationType);

			return result;
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

			return MapMergeFactory(sourceType, destinationType, mappingOptions).MapNewFactory();
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

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if(!_configuration.TryGetDoubleMap<MappingContext>((sourceType, destinationType), out var map))
				throw new MapNotFoundException((sourceType, destinationType));

			var context = _contextsCache.GetOrCreate(mappingOptions);

			return new DefaultMergeMapFactory(
				sourceType, destinationType,
				(source, destination) => {
					TypeUtils.CheckObjectType(source, sourceType, nameof(source));
					TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

					var result = map.Invoke(source, destination, context);

					// Should not happen
					TypeUtils.CheckObjectType(result, destinationType);

					return result;
				});
		}
		#endregion

		#region IMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetNewMaps(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return GetMergeMaps().Where(m => ObjectFactory.CanCreate(m.To));
		}

		public IEnumerable<(Type From, Type To)> GetMergeMaps(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return _configuration.GetMaps();
		}
		#endregion
	}
}
