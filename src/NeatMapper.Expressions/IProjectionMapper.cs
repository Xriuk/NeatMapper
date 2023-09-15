using System.Linq.Expressions;

namespace NeatMapper.Expressions {
	public interface IProjectionMapper {
		public LambdaExpression Project(Type sourceType, Type destinationType);
	}
}
