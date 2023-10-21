using System;
using System.Reflection;

namespace NeatMapper {
	public abstract class TypesException : Exception {
		internal TypesException(string message, Exception exception) :
			base(message, exception is TargetInvocationException tie ? tie.InnerException ?? exception : exception) { }
	}
}
