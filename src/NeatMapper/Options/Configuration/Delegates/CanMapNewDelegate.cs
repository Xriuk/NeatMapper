namespace NeatMapper {
	/// <summary>
	/// Delegate which allows checking if an object could be mapped to a new one, used to add custom
	/// <see cref="ICanMapNew{TSource, TDestination}"/>, in conjunction with
	/// <see cref="NewMapDelegate{TSource, TDestination}"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <param name="context">Mapping context, which allows checking nested mappings, services retrieval via DI, ....</param>
	/// <returns>
	/// <see langword="true"/> if an object of type <typeparamref name="TDestination"/> can be created
	/// from an object of type <typeparamref name="TSource"/>.
	/// </returns>
	public delegate bool CanMapNewDelegate<in TSource, out TDestination>(MappingContext context);
}
