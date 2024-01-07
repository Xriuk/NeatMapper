using System;
using System.Collections.Generic;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which creates and caches factories from another <see cref="IMapper"/>
	/// based on mapped types and <see cref="MappingOptions"/>, and uses them to perform the mappings.<br/>
	/// This allows to reuse the same factories if <see cref="MappingOptions"/> do not change.
	/// </summary>
	internal sealed class CachedFactoryMapper : IMapper, IMapperCanMap, IMapperFactory {
		private readonly IMapper _mapper;
		private readonly Dictionary<(Type, Type, MappingOptions), Func<object, object>> _newFactories = new Dictionary<(Type, Type, MappingOptions), Func<object, object>>();
		private readonly Dictionary<(Type, Type, MappingOptions), Func<object, object, object>> _mergeFactories = new Dictionary<(Type, Type, MappingOptions), Func<object, object, object>>();

		public CachedFactoryMapper(IMapper mapper) {
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private Func<object, object> GetOrCreateNewFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			lock (_newFactories) {
				if (!_newFactories.TryGetValue((sourceType, destinationType, mappingOptions), out var factory)) {
					try { 
						factory = _mapper.MapNewFactory(sourceType, destinationType, mappingOptions);
						_newFactories.Add((sourceType, destinationType, mappingOptions), factory);
					}
					catch (MapNotFoundException) {
						_newFactories.Add((sourceType, destinationType, mappingOptions), factory);
						throw;
					}
				}

				if(factory == null)
					throw new MapNotFoundException((sourceType, destinationType));

				return factory;
			}
		}

		private Func<object, object, object> GetOrCreateMergeFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			lock (_mergeFactories) {
				if (!_mergeFactories.TryGetValue((sourceType, destinationType, mappingOptions), out var factory)) {
					try {
						factory = _mapper.MapMergeFactory(sourceType, destinationType, mappingOptions);
						_mergeFactories.Add((sourceType, destinationType, mappingOptions), factory);
					}
					catch (MapNotFoundException) {
						_mergeFactories.Add((sourceType, destinationType, mappingOptions), factory);
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

		#region IMapper methods
		public
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

			return GetOrCreateNewFactory(sourceType, destinationType, mappingOptions).Invoke(source);
		}

		public
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

			return GetOrCreateMergeFactory(sourceType, destinationType, mappingOptions).Invoke(source, destination);
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

			return _mapper.CanMapNew(sourceType, destinationType, mappingOptions);
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

			return _mapper.CanMapMerge(sourceType, destinationType, mappingOptions);
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

			return GetOrCreateNewFactory(sourceType, destinationType, mappingOptions);
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

			return GetOrCreateMergeFactory(sourceType, destinationType, mappingOptions);
		}
		#endregion
	}
}
