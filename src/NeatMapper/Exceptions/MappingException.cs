using System;

namespace NeatMapper {
	/// <summary>
	/// Exception thrown to wrap an exception thrown inside a map.
	/// </summary>
	public sealed class MappingException : TypesException {
		public MappingException(Exception? exception, (Type From, Type To) types) :
			base($"An exception was thrown while mapping the types: {types.From.Name} -> {types.To.Name}\n" +
			$"{types.From.FullName} -> {types.To.FullName}\n" +
			$"Check the inner exception for details.", exception) { }
	}
}
