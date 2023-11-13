using System.Linq.Expressions;

namespace NeatMapper.Expressions {
	public interface IProjector {
		public LambdaExpression Project(Type sourceType, Type destinationType);
	}
}
