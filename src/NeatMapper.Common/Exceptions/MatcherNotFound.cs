using System;

namespace NeatMapper {
	/// <summary>
	/// Exception thrown when no suitable collection element comparer was found for the given types
	/// </summary>
	public class MatcherNotFound : ArgumentException {
		public MatcherNotFound((Type From, Type To) types) :
			base($"No collection element comparer could be found for the given types: {types.From.Name} -> {types.To.Name}\n" +
			$"{types.From.FullName} -> {types.To.FullName}") { }
	}
}
