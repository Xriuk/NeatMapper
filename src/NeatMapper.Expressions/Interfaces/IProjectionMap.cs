using System.Linq.Expressions;

namespace NeatMapper.Expressions {
	public interface IProjectionMap<TSource, TDestination> {
		public static abstract Expression<Func<TSource, TDestination>> Map(ProjectionContext context);
	}
}
