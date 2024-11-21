namespace NeatMapper {
	/// <summary>
	/// Delegate which allows checking if an object could be projected to another, used to add custom
	/// <see cref="ICanProject{TSource, TDestination}"/>, in conjunction with
	/// <see cref="ProjectionMapDelegate{TSource, TDestination}"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <param name="context">Projection context, which allows checking nested projections, services retrieval via DI, ....</param>
	/// <returns>
	/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be projected
	/// to an object of type <typeparamref name="TDestination"/>.
	/// </returns>
	public delegate bool CanProjectDelegate<in TSource, out TDestination>(ProjectionContext context);
}
