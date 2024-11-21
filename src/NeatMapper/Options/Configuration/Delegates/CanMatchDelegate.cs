namespace NeatMapper {
	/// <summary>
	/// Delegate which allows checking if an object could be matched with another one, used to add custom
	/// <see cref="ICanMatch{TSource, TDestination}"/>, in conjunction with
	/// <see cref="MatchMapDelegate{TSource, TDestination}"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <param name="context">Matching context, which allows checking nested matchings, services retrieval via DI, ....</param>
	/// <returns>
	/// <see langword="true"/> if an object of type <typeparamref name="TSource"/> can be matched
	/// with an object of type <typeparamref name="TDestination"/>.
	/// </returns>
	public delegate bool CanMatchDelegate<in TSource, in TDestination>(MatchingContext context);
}
