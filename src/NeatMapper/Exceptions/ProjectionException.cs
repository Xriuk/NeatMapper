using System;

namespace NeatMapper {
	/// <summary>
	/// Exception thrown when an exception was thrown inside a projection
	/// </summary>
	public sealed class ProjectionException : TypesException {
		public ProjectionException(Exception exception, (Type From, Type To) types) :
			base($"An exception was thrown while projecting the types: {types.From.Name} -> {types.To.Name}\n" +
			$"{types.From.FullName} -> {types.To.FullName}\n" +
			$"Check the inner exception for details", exception) { }
	}
}
