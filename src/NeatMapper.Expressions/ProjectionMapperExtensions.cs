using System.Linq.Expressions;

namespace NeatMapper.Expressions {
	public static class ProjectionMapperExtensions {
		public static Expression<Func<TSource, TDestination>> Project<TSource, TDestination>(this IProjectionMapper mapper) {
			return (Expression<Func<TSource, TDestination>>)mapper.Project(typeof(TSource), typeof(TDestination));
		}
	}
}
