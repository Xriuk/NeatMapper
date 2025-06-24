using System;
using System.Collections.Generic;

namespace NeatMapper {
	/// <summary>
	/// Options applied to <see cref="EnumMapper"/>.<br/>
	/// Can be overridden during mapping with <see cref="EnumMapperMappingOptions"/>.
	/// </summary>
	public sealed class EnumMapperOptions {
		internal sealed class EnumMapperOptionsSingle {
			/// <summary>
			/// True to match strings to enums in a case insensitive way.
			/// </summary>
			/// <remarks>Defaults to <see langword="true"/>.</remarks>
			public bool? StringToEnumCaseInsensitive { get; set; }

			/// <inheritdoc cref="NeatMapper.EnumToNumberMapping"/>
			/// <remarks>Defaults to <see cref="EnumToNumberMapping.Value"/>.</remarks>
			public EnumToNumberMapping? EnumToNumberMapping { get; set; }
		}


		/// <summary>
		/// Creates a new instance of <see cref="EnumMapperOptions"/>.
		/// </summary>
		public EnumMapperOptions() {
			EnumToNumberMapping = EnumToNumberMapping.Value;
			EnumToEnumMapping = EnumToEnumMapping.Value;
			_enumMaps = [];
			_enumToEnumMaps = [];
		}
		/// <summary>
		/// Creates a new instance of <see cref="EnumMapperOptions"/> by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from.</param>
		public EnumMapperOptions(EnumMapperOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			EnumToNumberMapping = options.EnumToNumberMapping;
			EnumToEnumMapping = options.EnumToEnumMapping;
			_enumMaps = new Dictionary<Type, EnumMapperOptionsSingle>(options._enumMaps);
			_enumToEnumMaps = new Dictionary<(Type From, Type To), EnumToEnumMapping>(options._enumToEnumMaps);
		}

		internal readonly Dictionary<Type, EnumMapperOptionsSingle> _enumMaps;
		internal readonly Dictionary<(Type From, Type To), EnumToEnumMapping> _enumToEnumMaps;


		/// <summary>
		/// True to match strings to enums in a case insensitive way.
		/// </summary>
		/// <remarks>Defaults to <see langword="true"/>.</remarks>
		public bool StringToEnumCaseInsensitive { get; set; }

		/// <inheritdoc cref="NeatMapper.EnumToNumberMapping"/>
		/// <remarks>Defaults to <see cref="EnumToNumberMapping.Value"/>.</remarks>
		public EnumToNumberMapping EnumToNumberMapping { get; set; }

		/// <inheritdoc cref="NeatMapper.EnumToEnumMapping"/>
		/// <remarks>Defaults to <see cref="EnumToEnumMapping.Value"/>.</remarks>
		public EnumToEnumMapping EnumToEnumMapping { get; set; }

		/// <summary>
		/// Overrides the default <see cref="EnumToNumberMapping"/> for an enum type.
		/// </summary>
		/// <typeparam name="TEnum">Enum type.</typeparam>
		/// <param name="enumToNumberMapping">
		/// <inheritdoc cref="NeatMapper.EnumToNumberMapping" path="/summary"/>
		/// </param>
		public void OverrideEnumMap<TEnum>(EnumToNumberMapping enumToNumberMapping) where TEnum : Enum {
			OverrideEnumMap<TEnum>(null, enumToNumberMapping);
		}
		/// <summary>
		/// Overrides the default <see cref="StringToEnumCaseInsensitive"/> for an enum type.
		/// </summary>
		/// <typeparam name="TEnum">Enum type.</typeparam>
		/// <param name="stringToEnumCaseInsensitive">
		/// <inheritdoc cref="StringToEnumCaseInsensitive" path="/summary"/>
		/// </param>
		public void OverrideEnumMap<TEnum>(bool stringToEnumCaseInsensitive) where TEnum : Enum {
			OverrideEnumMap<TEnum>(stringToEnumCaseInsensitive, null);
		}
		/// <summary>
		/// Overrides the default <see cref="StringToEnumCaseInsensitive"/> and
		/// <see cref="EnumToNumberMapping"/> for an enum type.
		/// </summary>
		/// <typeparam name="TEnum">Enum type.</typeparam>
		/// <param name="stringToEnumCaseInsensitive">
		/// <inheritdoc cref="StringToEnumCaseInsensitive" path="/summary"/>
		/// </param>
		/// <param name="enumToNumberMapping">
		/// <inheritdoc cref="NeatMapper.EnumToNumberMapping" path="/summary"/>
		/// </param>
		public void OverrideEnumMap<TEnum>(bool stringToEnumCaseInsensitive, EnumToNumberMapping enumToNumberMapping) where TEnum : Enum {
			OverrideEnumMap<TEnum>((bool?)stringToEnumCaseInsensitive, (EnumToNumberMapping?)enumToNumberMapping);
		}
		private void OverrideEnumMap<TEnum>(bool? stringToEnumCaseInsensitive, EnumToNumberMapping? enumToNumberMapping) where TEnum : Enum {
			if(!_enumMaps.TryGetValue(typeof(TEnum), out var map)) {
				map = new EnumMapperOptionsSingle();
				_enumMaps[typeof(TEnum)] = map;
			}

			if(stringToEnumCaseInsensitive != null)
				map.StringToEnumCaseInsensitive = stringToEnumCaseInsensitive.Value;
			if(enumToNumberMapping != null)
				map.EnumToNumberMapping = enumToNumberMapping.Value;
		}

		/// <summary>
		/// Overrides the default <see cref="EnumToEnumMapping"/> for two enum types.
		/// </summary>
		/// <typeparam name="TSource">Source enum type.</typeparam>
		/// <typeparam name="TDestination">Destination enum type.</typeparam>
		/// <param name="mapping"><inheritdoc cref="NeatMapper.EnumToEnumMapping" path="/summary"/></param>
		/// <param name="reverse">
		/// If true will use the same <paramref name="mapping"/> for the inverted types.
		/// </param>
		public void OverrideEnumToEnumMap<TSource, TDestination>(EnumToEnumMapping mapping, bool reverse = false) where TSource : Enum where TDestination : Enum {
			_enumToEnumMaps[(typeof(TSource), typeof(TDestination))] = mapping;
			if (reverse)
				_enumToEnumMaps[(typeof(TDestination), typeof(TSource))] = mapping;
		}
	}
}
