using System;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches by invoking <see cref="Object.Equals(object, object)">
	/// (and overloads).
	/// </summary>
	public sealed class ObjectEqualsMatcher : IMatcher {
		/// <summary>
		/// Singleton instance of the matcher
		/// </summary>
		public static readonly IMatcher Instance = new ObjectEqualsMatcher();


		internal ObjectEqualsMatcher() { }


		public bool CanMatch(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			// Every type implements object.Equals, but they need to be the same type to be compared
			return sourceType == destinationType;
		}

		public bool Match(
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

			return Object.Equals(source, destination);
		}
	}
}
