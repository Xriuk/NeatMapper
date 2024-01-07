using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which creates and caches factories from another <see cref="IAsyncMapper"/>
	/// based on mapped types, <see cref="MappingOptions"/> and <see cref="CancellationToken"/>, and uses them to perform the mappings.<br/>
	/// This allows to reuse the same factories if <see cref="MappingOptions"/> and <see cref="CancellationToken"/> do not change.
	/// </summary>
	internal sealed class AsyncCachedFactoryMapper : IAsyncMapper, IAsyncMapperCanMap, IAsyncMapperFactory {
		private readonly IAsyncMapper _mapper;
		private readonly Dictionary<(Type, Type, MappingOptions, CancellationToken), Func<object, Task<object>>> _newFactories = new Dictionary<(Type, Type, MappingOptions, CancellationToken), Func<object, Task<object>>>();
		private readonly Dictionary<(Type, Type, MappingOptions, CancellationToken), Func<object, object, Task<object>>> _mergeFactories = new Dictionary<(Type, Type, MappingOptions, CancellationToken), Func<object, object, Task<object>>>();

		public AsyncCachedFactoryMapper(IAsyncMapper mapper) {
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private Func<object, Task<object>> GetOrCreateNewFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions, CancellationToken cancellationToken) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			lock (_newFactories) {
				if (!_newFactories.TryGetValue((sourceType, destinationType, mappingOptions, cancellationToken), out var factory)) {
					try {
						factory = _mapper.MapAsyncNewFactory(sourceType, destinationType, mappingOptions, cancellationToken);
						_newFactories.Add((sourceType, destinationType, mappingOptions, cancellationToken), factory);
					}
					catch (MapNotFoundException) {
						_newFactories.Add((sourceType, destinationType, mappingOptions, cancellationToken), factory);
						throw;
					}
				}

				if (factory == null)
					throw new MapNotFoundException((sourceType, destinationType));

				return factory;
			}
		}

		private Func<object, object, Task<object>> GetOrCreateMergeFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions, CancellationToken cancellationToken) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			lock (_mergeFactories) {
				if (!_mergeFactories.TryGetValue((sourceType, destinationType, mappingOptions, cancellationToken), out var factory)) {
					try {
						factory = _mapper.MapAsyncMergeFactory(sourceType, destinationType, mappingOptions, cancellationToken);
						_mergeFactories.Add((sourceType, destinationType, mappingOptions, cancellationToken), factory);
					}
					catch (MapNotFoundException) {
						_mergeFactories.Add((sourceType, destinationType, mappingOptions, cancellationToken), factory);
						throw;
					}
				}

				if (factory == null)
					throw new MapNotFoundException((sourceType, destinationType));

				return factory;
			}
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


		#region IAsyncMapper methods
		public Task<
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

			return GetOrCreateNewFactory(sourceType, destinationType, mappingOptions, cancellationToken).Invoke(source);
		}

		public Task<
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

			return GetOrCreateMergeFactory(sourceType, destinationType, mappingOptions, cancellationToken).Invoke(source, destination);
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

			return _mapper.CanMapAsyncNew(sourceType, destinationType, mappingOptions, cancellationToken);
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

			return _mapper.CanMapAsyncMerge(sourceType, destinationType, mappingOptions, cancellationToken);
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

			return GetOrCreateNewFactory(sourceType, destinationType, mappingOptions, cancellationToken);
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

			return GetOrCreateMergeFactory(sourceType, destinationType, mappingOptions, cancellationToken);
		}
		#endregion
	}
}
