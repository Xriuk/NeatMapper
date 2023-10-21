using System;

namespace NeatMapper {
	/// <summary>
	/// Exception thrown when no suitable map was found for the given types
	/// </summary>
	public class MapNotFoundException : ArgumentException {
		public MapNotFoundException((Type From, Type To) types) :
			base($"No map could be found for the given types: {types.From.Name} -> {types.To.Name}\n" +
			$"{types.From.FullName} -> {types.To.FullName}") { }
	}
}
