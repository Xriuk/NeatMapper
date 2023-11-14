using System.Linq.Expressions;

namespace NeatMapper.Expressions {
	public delegate Expression<Func<TSource?, TDestination?>> ProjectionMapDelegate<TSource, TDestination>(ProjectionContext context);
}
