using System.Reflection;

namespace NeatMapper.Core.Configuration {
	public interface IMapperConfiguration {
		/// <summary>
		/// <see cref="IMap{TSource, TDestination}.Map(TSource, MappingContext)"/>
		/// </summary>
		public IDictionary<(Type From, Type To), MethodInfo> Maps { get; }

		/// <summary>
		/// <see cref="IMergeMap{TSource, TDestination}.Map(TSource, TDestination, MappingContext)"/>
		/// </summary>
		public IDictionary<(Type From, Type To), MethodInfo> MergeMaps { get; }
	}
}
