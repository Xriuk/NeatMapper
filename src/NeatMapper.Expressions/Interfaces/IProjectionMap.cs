using System.Linq.Expressions;

namespace NeatMapper.Expressions {
	public interface IProjectionMap<TSource, TDestination> {
		Expression<Func<TSource?, TDestination?>> Project(ProjectionContext context);
	}
}
