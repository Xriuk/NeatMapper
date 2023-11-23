using System;
#if !NET47_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace NeatMapper {
	/// <summary>
	/// Exception thrown when no suitable map was found for the given types
	/// </summary>
	public sealed class MapNotFoundException : Exception {
		public MapNotFoundException((Type From, Type To) types) :
			base($"No map could be found for the given types: {types.From.Name} -> {types.To.Name}\n" +
			$"{types.From.FullName} -> {types.To.FullName}") {

			From = types.From;
			To = types.To;
		}

		public Type From { get; }

		public Type To { get; }


#if !NET47_OR_GREATER
		[DoesNotReturn] 
#endif
		public static void Throw<TSource, TDestination>() {
			throw new MapNotFoundException((typeof(TSource), typeof(TDestination)));
		}
	}
}
