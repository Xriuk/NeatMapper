namespace NeatMapper.Expressions {
	/// <summary>
	/// Interface used to perform nested projections, it is not implemented so it will always throw when invoked,
	/// but it will be used to replace the expressions with the correct map
	/// </summary>
	public interface INestedProjectionMapper {
		public TDestination Project<TSource, TDestination>(TSource source);

		public TDestination Project<TDestination>(object source);
	}
}
