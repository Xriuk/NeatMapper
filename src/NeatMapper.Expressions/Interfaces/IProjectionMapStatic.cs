using System.Linq.Expressions;

namespace NeatMapper.Expressions {
	public interface IProjectionMapStatic<TSource, TDestination> {
		public static abstract Expression<Func<TSource?, TDestination?>> Project(ProjectionContext context);
	}
}
