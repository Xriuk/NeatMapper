using System;

namespace NeatMapper {
	/// <summary>
	/// Configuration info for a generic used-defined map.
	/// </summary>
	/// <remarks>At least one of <see cref="From"/> or <see cref="To"/> is an open generic type.</remarks>
	internal sealed class CustomGenericMap {
		/// <summary>
		/// Source type of the map.
		/// </summary>
		/// <remarks>May be an open generic type.</remarks>
		public Type From { get; set; } = null!;

		/// <summary>
		/// Destination type of the map.
		/// </summary>
		/// <remarks>May be an open generic type.</remarks>
		public Type To { get; set; } = null!;

		/// <summary>
		/// Declaring class of the <see cref="Method"/>.
		/// </summary>
		public Type Class { get; set; } = null!;

		/// <summary>
		/// Handle of the generic method to be invoked, used with
		/// <see cref="System.Reflection.MethodBase.GetMethodFromHandle(RuntimeMethodHandle, RuntimeTypeHandle)"/>
		/// with generated concrete type during mapping.
		/// </summary>
		/// <remarks>May be instance or static.</remarks>
		public RuntimeMethodHandle Method { get; set; }
	}
}
