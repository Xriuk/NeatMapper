using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which maps enums to and from their underlying numeric types, strings
	/// and other enums. Supports only new maps.
	/// </summary>
	public sealed class EnumMapper : IMapper {
		/// <summary>
		/// Options to apply during mapping.
		/// </summary>
		private readonly EnumMapperOptions _enumMapperOptions;

		/// <summary>
		/// Creates a new instance of <see cref="CollectionMapper"/>.
		/// </summary>
		/// <param name="enumMapperOptions">
		/// Options to apply during mapping, null to use default.<br/>
		/// Can be overridden during mapping with <see cref="EnumMapperMappingOptions"/>.
		/// </param>
		public EnumMapper(EnumMapperOptions? enumMapperOptions = null) {
			_enumMapperOptions = enumMapperOptions != null ? new EnumMapperOptions(enumMapperOptions) : new EnumMapperOptions();
		}


		#region IMapper methods
		public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// One type must be an enum
			Type enumType;
			Type otherType;
			if (sourceType.IsEnum) {
				enumType = sourceType;
				otherType = destinationType;
			}
			else if (destinationType.IsEnum) {
				enumType = destinationType;
				otherType = sourceType;
			}
			else
				return false;

			if (otherType == typeof(string))
				return true;

			// If the other type is the underlying type, if we are hashing the names we must not have hash collisions
			if(otherType == Enum.GetUnderlyingType(enumType)) {
				return GetEnumToNumberMapping(enumType, mappingOptions) == EnumToNumberMapping.Value ||
					Enum.GetValues(enumType).Cast<object>().Select(v => GetHashedUnderlyingValue(v, enumType, otherType)).Distinct().Count() == 1;
			}

			// If the other type is an enum the two must have all mappings from source to destination
			if (otherType.IsEnum) {
				return sourceType.GetFields().All(f => GetEnumMatch(
					f.GetRawConstantValue()!,
					sourceType, destinationType,
					GetEnumToEnumMapping(sourceType, destinationType, mappingOptions)) != null);
			}

			return false;
		}

		public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return false;
		}

		public object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapNew(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			// One type must be an enum
			Type enumType;
			Type otherType;
			if (sourceType.IsEnum) {
				enumType = sourceType;
				otherType = destinationType;
			}
			else if (destinationType.IsEnum) {
				enumType = destinationType;
				otherType = sourceType;
			}
			else
				return false;

			if (otherType == typeof(string)) {
				if (sourceType == otherType) {
					var stringComparison = GetStringToEnumCaseInsensitive(enumType, mappingOptions) ?
						StringComparison.OrdinalIgnoreCase :
						StringComparison.Ordinal;

					return enumType.GetFields().FirstOrDefault(f => {
						var enumName = GetFieldName(f);
						if (string.IsNullOrEmpty(enumName))
							return false;
						else
							return enumName!.Equals((string)source!, stringComparison);
					})?.GetRawConstantValue();
				}
				else
					return GetEnumName(source!, sourceType);
			}
			else {
				var underlyingType = Enum.GetUnderlyingType(enumType);
				if (otherType == underlyingType) {
					if(GetEnumToNumberMapping(enumType, mappingOptions) == EnumToNumberMapping.Value) { 
						if (sourceType == otherType) {
							try {
								return Enum.Parse(enumType, source!.ToString()!);
							}
							catch (Exception e) {
								throw new MappingException(e, (sourceType, destinationType));
							}
						}
						else
							return Convert.ChangeType(source, underlyingType);
					}
					else {
						if (sourceType == otherType) {
							return Enum.GetValues(enumType).Cast<object>()
								.FirstOrDefault(v => GetHashedUnderlyingValue(v, enumType, underlyingType) == source)
								?? throw new MappingException(new InvalidOperationException("Enum name hash cannot be converted to enum value"), (sourceType, destinationType));
						}
						else 
							return GetHashedUnderlyingValue(source!, enumType, underlyingType);
					}
				}
				else {
					return GetEnumMatch(
						source!,
						sourceType, destinationType,
						GetEnumToEnumMapping(sourceType, destinationType, mappingOptions))
							?? throw new MappingException(new InvalidOperationException("Enum value cannot be converted to destination enum type"), (sourceType, destinationType));
				}
			}
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			throw new MapNotFoundException((sourceType, destinationType));
		}
		#endregion


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool GetStringToEnumCaseInsensitive(Type type, MappingOptions? mappingOptions) {
			return mappingOptions?.GetOptions<EnumMapperMappingOptions>()?.StringToEnumCaseInsensitive
				?? (_enumMapperOptions._enumMaps.TryGetValue(type, out var options) ?
					options.StringToEnumCaseInsensitive :
					null)
				?? _enumMapperOptions.StringToEnumCaseInsensitive;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private EnumToNumberMapping GetEnumToNumberMapping(Type type, MappingOptions? mappingOptions) {
			return mappingOptions?.GetOptions<EnumMapperMappingOptions>()?.EnumToNumberMapping
				?? (_enumMapperOptions._enumMaps.TryGetValue(type, out var options) ?
					options.EnumToNumberMapping :
					null)
				?? _enumMapperOptions.EnumToNumberMapping;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private EnumToEnumMapping GetEnumToEnumMapping(Type sourceType, Type destinationType, MappingOptions? mappingOptions) {
			return mappingOptions?.GetOptions<EnumMapperMappingOptions>()?.EnumToEnumMapping
				?? (_enumMapperOptions._enumToEnumMaps.TryGetValue((sourceType, destinationType), out var options) ?
					options :
					_enumMapperOptions.EnumToEnumMapping);
		}

		private static object? GetEnumMatch(object source, Type sourceType, Type destinationType, EnumToEnumMapping mapping) {
			switch (mapping) {
			case EnumToEnumMapping.Value:
				try { 
					return Enum.Parse(destinationType, Convert.ChangeType(source, Enum.GetUnderlyingType(sourceType)).ToString()!);
				}
				catch {
					return null;
				}
			case EnumToEnumMapping.NameCaseSensitive:
			case EnumToEnumMapping.NameCaseInsensitive:
				var sourceName = GetEnumName(source, sourceType);
				if(string.IsNullOrEmpty(sourceName))
					return null;

				var stringComparison = mapping == EnumToEnumMapping.NameCaseInsensitive ?
					StringComparison.OrdinalIgnoreCase :
					StringComparison.Ordinal;

				return destinationType.GetFields().FirstOrDefault(f => {
					var destinationName = GetFieldName(f);
					if (string.IsNullOrEmpty(destinationName))
						return false;
					else
						return destinationName!.Equals(sourceName, stringComparison);
				})?.GetRawConstantValue();
			default:
				return null;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string? GetEnumName(object value, Type type) {
			var fieldName = Enum.GetName(type, value);
			if (fieldName == null)
				return null;

			var sourceField = type.GetField(fieldName);
			if (sourceField == null)
				return null;

			return GetFieldName(sourceField);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string? GetFieldName(FieldInfo field) {
			if (Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute)) is EnumMemberAttribute enumMember)
				return enumMember.Value;
			else if (Attribute.GetCustomAttribute(field, typeof(DisplayAttribute)) is DisplayAttribute display)
				return display.Name;
			else
				return field.Name;
		}

		private static object GetHashedUnderlyingValue(object value, Type enumType, Type underlyingType) {
			var data = Encoding.UTF8.GetBytes(GetEnumName(value!, enumType)!);
			byte[] hash;
#if NET5_0_OR_GREATER
							hash = SHA256.HashData(data);
#else
			using (var sha256 = SHA256.Create()) {
				hash = sha256.ComputeHash(data);
			}
#endif

			if (underlyingType == typeof(ulong))
				return BitConverter.ToUInt64(hash, 0);
			else if (underlyingType == typeof(long))
				return BitConverter.ToInt64(hash, 0);

			else if (underlyingType == typeof(uint))
				return BitConverter.ToUInt32(hash, 0);
			else if (underlyingType == typeof(int))
				return BitConverter.ToInt32(hash, 0);

			else if (underlyingType == typeof(ushort))
				return BitConverter.ToUInt16(hash, 0);
			else if (underlyingType == typeof(short))
				return BitConverter.ToInt16(hash, 0);

			else if (underlyingType == typeof(byte))
				return hash[0];
			else if (underlyingType == typeof(sbyte))
				return Convert.ToSByte(hash[0]);
			else
				throw new InvalidOperationException("Unsupported enum underlying type");
		}
	}
}
