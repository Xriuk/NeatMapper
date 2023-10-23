using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	public sealed class AsyncNewMapper : AsyncCustomMapper, IAsyncMapperCanMap {
		public AsyncNewMapper(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomMapsOptions?
#else
			CustomMapsOptions
#endif
			mapsOptions = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			CustomAsyncNewAdditionalMapsOptions?
#else
			CustomAsyncNewAdditionalMapsOptions
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
						return type == typeof(IAsyncNewMap<,>)
#if NET7_0_OR_GREATER
							|| type == typeof(IAsyncNewMapStatic<,>)
#endif
						;
					},
					mapsOptions ?? new CustomMapsOptions(),
					additionalMapsOptions?._maps.Values
				),
				serviceProvider) { }

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


		#region IAsyncMapper methods
		override public async Task<
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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (source != null && !sourceType.IsAssignableFrom(source.GetType()))
				throw new ArgumentException($"Object of type {source.GetType().FullName ?? source.GetType().Name} is not assignable to type {sourceType.FullName ?? sourceType.Name}", nameof(source));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			var types = (sourceType, destinationType);

			var task = (Task)_configuration.GetMap(types).Invoke(new object[] { source, CreateMappingContext(mappingOptions) });

			object result;
			try {
				result = await TaskUtils.AwaitTask<object>(task);
			}
			catch (MapNotFoundException) {
				throw;
			}
			catch (Exception e) {
				throw new MappingException(e, types);
			}

			// Should not happen
			if (result != null && !destinationType.IsAssignableFrom(result.GetType()))
				throw new InvalidOperationException($"Object of type {result.GetType().FullName ?? result.GetType().Name} is not assignable to type {destinationType.FullName ?? destinationType.Name}");

			return result;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
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
			// Not mapping merge
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion

		#region IAsyncMapperCanMap methods
		public Task<bool> CanMapAsyncNew(Type sourceType, Type destinationType, CancellationToken cancellationToken = default) {
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

		public Task<bool> CanMapAsyncMerge(Type sourceType, Type destinationType, CancellationToken cancellationToken = default) {
			return Task.FromResult(false);
		}
		#endregion
	}
}
