using System;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which wraps another <see cref="IMapper"/> and overrides some <see cref="MappingOptions"/>.
	/// </summary>
	public sealed class NestedMapper : IMapper, IMapperCanMap, IMapperFactory {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private readonly IMapper _mapper;
		private readonly Func<MappingOptions, MappingOptions> _mappingOptionsEditor;

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif

		/// <summary>
		/// Creates a new instance of <see cref="NestedMapper"/>.
		/// </summary>
		/// <param name="mapper">Mapper to forward the actual mapping to.</param>
		/// <param name="mappingOptionsEditor">
		/// Method to invoke to alter the <see cref="MappingOptions"/> passed to the mapper,
		/// both the passed parameter and the returned value may be null.
		/// </param>
		public NestedMapper(
			IMapper mapper,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				MappingOptions?, MappingOptions?
#else
				MappingOptions, MappingOptions
#endif
			> mappingOptionsEditor) {

			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_mappingOptionsEditor = mappingOptionsEditor ?? throw new ArgumentNullException(nameof(mappingOptionsEditor));
		}


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

			return _mapper.Map(source, sourceType, destinationType, _mappingOptionsEditor.Invoke(mappingOptions));
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

			return _mapper.Map(source, sourceType, destination, destinationType, _mappingOptionsEditor.Invoke(mappingOptions));
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

			return _mapper.CanMapNew(sourceType, destinationType, _mappingOptionsEditor.Invoke(mappingOptions));
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

			return _mapper.CanMapMerge(sourceType, destinationType, _mappingOptionsEditor.Invoke(mappingOptions));
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

			return _mapper.MapNewFactory(sourceType, destinationType, _mappingOptionsEditor.Invoke(mappingOptions));
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

			return _mapper.MapMergeFactory(sourceType, destinationType, _mappingOptionsEditor.Invoke(mappingOptions));
		}
		#endregion
	}
}
