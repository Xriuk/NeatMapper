using System.Reflection;

namespace NeatMapper.Common.Mapper {
	public abstract class TypesException : Exception {
		protected TypesException(string message, Exception exception) : base(message, exception is TargetInvocationException tie ? tie.InnerException ?? exception : exception) { }
	}
}
