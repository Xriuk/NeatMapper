using System;

namespace NeatMapper {
	[Obsolete("MatcherNotFound is no longer used and will be removed in future versions.")]
	public sealed class MatcherNotFound : Exception {
		public MatcherNotFound((Type From, Type To) types) :
			base($"No collection element comparer could be found for the given types: {types.From.Name} -> {types.To.Name}\n" +
			$"{types.From.FullName} -> {types.To.FullName}") { }
	}
}
