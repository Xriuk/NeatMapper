using System;

namespace NeatMapper {
	/// <summary>
	/// Exception thrown to wrap an exception thrown inside a match.
	/// </summary>
	public sealed class MatcherException : TypesException {
		public MatcherException(Exception exception, (Type From, Type To) types) :
			base($"An exception was thrown while comparing the types: {types.From.Name} -> {types.To.Name}\n" +
			$"{types.From.FullName} -> {types.To.FullName}\n" +
			$"Check the inner exception for details", exception) { }
	}
}
