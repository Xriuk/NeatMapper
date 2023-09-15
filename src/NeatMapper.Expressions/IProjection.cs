using System.Linq.Expressions;

namespace NeatMapper.Expressions {
	public interface IProjection<TSource, TDestination> {
		public static abstract Expression<Func<TSource, TDestination>> Map(ProjectionContext context);
	}
}
