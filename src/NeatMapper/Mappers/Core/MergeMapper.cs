using System;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which maps objects by using <see cref="IMergeMap{TSource, TDestination}"/>.<br/>
	/// Supports both merge and new maps.
	/// </summary>
	public sealed class MergeMapper : CustomMapper, IMapperCanMap, IMapperFactory {
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


		private Func<object, object> CreateNewFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions, bool isRealFactory) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Forward new map to merge by creating a destination
			if (!ObjectFactory.CanCreate(destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			var mergeFactory = CreateMergeFactory(sourceType, destinationType, mappingOptions, isRealFactory);
			var destinationFactory = ObjectFactory.CreateFactory(destinationType);
			return source => {
				object destination;
				try {
					destination = destinationFactory.Invoke();
				}
				catch (ObjectCreationException e) {
					throw new MappingException(e, (sourceType, destinationType));
				}

				return mergeFactory.Invoke(source, destination);
			};
		}

		private Func<object, object, object> CreateMergeFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions, bool isRealFactory) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// DEV: replace below with TryAdd (which should not alter options if nothing changes)
			if (isRealFactory)
				mappingOptions = (mappingOptions ?? MappingOptions.Empty).ReplaceOrAdd<FactoryContext>(_ => FactoryContext.Instance);

			var map = _configuration.GetMap((sourceType, destinationType));
			var parameters = new object[] { null, null, CreateMappingContext(mappingOptions) };

			return (source, destination) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));
				TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

				parameters[0] = source;
				parameters[1] = destination;
				var result = map.Invoke(parameters);

				// Should not happen
				TypeUtils.CheckObjectType(result, destinationType);

				return result;
			};
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

			return CreateMergeFactory(sourceType, destinationType, mappingOptions, false).Invoke(source, destination);
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

			// Source type null checked in CanMapMerge
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

			return CreateMergeFactory(sourceType, destinationType, mappingOptions, true);
		}
		#endregion
	}
}
