using System.Linq.Expressions;

namespace NeatMapper.Expressions {
	public static class ProjectorExtensions {
		public static Expression<Func<TSource, TDestination>> Project<TSource, TDestination>(this IProjector mapper) {
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			return (Expression<Func<TSource, TDestination>>)mapper.Project(typeof(TSource), typeof(TDestination));
		}
	}
}
