﻿using System;

namespace NeatMapper {
	/// <summary>
	/// Exception thrown when an exception was thrown inside a IMatchMap or IMatchMapStatic
	/// </summary>
	public sealed class MatcherException : TypesException {
		public MatcherException(Exception exception, (Type From, Type To) types) :
			base($"An exception was thrown while comparing the types: {types.From.Name} -> {types.To.Name}\n" +
			$"{types.From.FullName} -> {types.To.FullName}\n" +
			$"Check the inner exception for details", exception) { }
	}
}
