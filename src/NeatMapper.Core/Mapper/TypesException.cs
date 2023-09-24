using System.Reflection;

namespace NeatMapper.Core.Mapper {
	public class TypesException : Exception {
		protected TypesException(string message, Exception exception) : base(message, exception is TargetInvocationException tie ? tie.InnerException ?? exception : exception) { }
	}
}
