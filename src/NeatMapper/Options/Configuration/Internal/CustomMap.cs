using System;
using System.Reflection;

namespace NeatMapper {
	/// <summary>
	/// Configuration info for a specific used-defined map.
	/// </summary>
	internal class CustomMap {
		/// <summary>
		/// Source type of the map.
		/// </summary>
		public Type From { get; set; } = null!;

		/// <summary>
		/// Destination type of the map.
		/// </summary>
		public Type To { get; set; } = null!;

		/// <summary>
		/// Instance on which to invoke the non-static <see cref="Method"/>, should be provided if cannot be automatically created
		/// like for classes with no parameterless constructors or special types like delegates.
		/// </summary>
		/// <remarks>May be null, in which case an instance should be created automatically by the service if possible.</remarks>
		public object? Instance { get; set; }

		/// <summary>
		/// Mapping method to invoke.
		/// </summary>
		/// <remarks>May be instance or static.</remarks>
		public MethodInfo Method { get; set; } = null!;
	}
}
