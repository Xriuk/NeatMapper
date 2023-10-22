using System;
using System.Collections;
namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which just returns false for every match
	/// </summary>
	public sealed class EmptyMatcher : IMatcher {
		public static readonly IMatcher Instance = new EmptyMatcher();


		internal EmptyMatcher() { }


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

			return false;
		}
	}
}
