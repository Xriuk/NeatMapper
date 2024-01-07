using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which maps objects by using <see cref="IAsyncMergeMap{TSource, TDestination}"/>.
	/// </summary>
	public sealed class AsyncMergeMapper : AsyncCustomMapper, IAsyncMapperCanMap, IAsyncMapperFactory {
		/// <summary>
		/// Creates a new instance of <see cref="AsyncMergeMapper"/>.<br/>
		/// At least one between <paramref name="mapsOptions"/> and <paramref name="additionalMapsOptions"/>
		/// should be specified.
		/// </summary>
		/// <param name="mapsOptions">Options to retrieve user-defined maps for the mapper, null to ignore.</param>
		/// <param name="additionalMapsOptions">Additional user-defined maps for the mapper, null to ignore.</param>
		/// <param name="serviceProvider">
		/// Service provider to be passed to the maps inside <see cref="AsyncMappingContext"/>, 
		/// null to pass an empty service provider.<br/>
		/// Can be overridden during mapping with <see cref="AsyncMapperOverrideMappingOptions"/>.
		/// </param>
		public AsyncMergeMapper(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomMapsOptions?
#else
			CustomMapsOptions
#endif
			mapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomAsyncMergeAdditionalMapsOptions?
#else
			CustomAsyncMergeAdditionalMapsOptions
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
#pragma warning disable CA1068
#endif

			base(new CustomMapsConfiguration(
					(_, i) => {
						if (!i.IsGenericType)
							return false;
						var type = i.GetGenericTypeDefinition();
						return type == typeof(IAsyncMergeMap<,>)
#if NET7_0_OR_GREATER
							|| type == typeof(IAsyncMergeMapStatic<,>)
#endif
						;
					},
					(mapsOptions ?? new CustomMapsOptions()).TypesToScan,
					additionalMapsOptions?._maps.Values
				),
				serviceProvider) { }


		private Func<object, Task<object>> CreateNewFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions, CancellationToken cancellationToken, bool isRealFactory) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Forward new map to merge by creating a destination
			if (!ObjectFactory.CanCreate(destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			var mergeFactory = CreateMergeFactory(sourceType, destinationType, mappingOptions, cancellationToken, isRealFactory);
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

		private Func<object, object, Task<object>> CreateMergeFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions, CancellationToken cancellationToken, bool isRealFactory) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// DEV: replace below with TryAdd (which should not alter options if nothing changes)
			if (isRealFactory)
				mappingOptions = (mappingOptions ?? MappingOptions.Empty).ReplaceOrAdd<FactoryContext>(_ => FactoryContext.Instance);

			var types = (sourceType, destinationType);

			var map = _configuration.GetMap(types);
			var parameters = new object[] { null, null, CreateMappingContext(mappingOptions, cancellationToken) };

			return async (source, destination) => {
				TypeUtils.CheckObjectType(source, sourceType, nameof(source));
				TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

				parameters[0] = source;
				parameters[1] = destination;
				var task = (Task)map.Invoke(parameters);

				object result;
				try {
					result = await TaskUtils.AwaitTask<object>(task);
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
				TypeUtils.CheckObjectType(result, destinationType);

				return result;
			};
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#pragma warning restore CA1068
#nullable enable
#endif


		#region IAsyncMapper methods
		override public Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> MapAsync(
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
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return CreateNewFactory(sourceType, destinationType, mappingOptions, cancellationToken, false).Invoke(source);
		}

		override public Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> MapAsync(
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
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return CreateMergeFactory(sourceType, destinationType, mappingOptions, cancellationToken, false).Invoke(source, destination);
		}
		#endregion

		#region IAsyncMapperCanMap methods
		public Task<bool> CanMapAsyncNew(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			// Source type null checked in CanMapAsyncMerge
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if(!ObjectFactory.CanCreate(destinationType))
				return Task.FromResult(false);

			return CanMapAsyncMerge(sourceType, destinationType, mappingOptions, cancellationToken);
		}

		public Task<bool> CanMapAsyncMerge(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			try {
				_configuration.GetMap((sourceType, destinationType));
				return Task.FromResult(true);
			}
			catch (MapNotFoundException) {
				return Task.FromResult(false);
			}
		}
		#endregion

		#region IAsyncMapperFactory methods
		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?, Task<object?>
#else
			object, Task<object>
#endif
			> MapAsyncNewFactory(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return CreateNewFactory(sourceType, destinationType, mappingOptions, cancellationToken, true);
		}

		public Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?, object?, Task<object?>
#else
			object, object, Task<object>
#endif
			> MapAsyncMergeFactory(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {

			return CreateMergeFactory(sourceType, destinationType, mappingOptions, cancellationToken, true);
		}
		#endregion
	}
}
