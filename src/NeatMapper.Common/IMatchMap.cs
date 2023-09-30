namespace NeatMapper {
	/// <summary>
	/// Interface which allows matching two objects
	/// </summary>
	/// <typeparam name="TSource">Source type</typeparam>
	/// <typeparam name="TDestination">Destination type</typeparam>
	public interface IMatchMap<TSource, TDestination> {
		/// <summary>
		/// Checks if two objects are the same (usually by comparing the keys of the two)
		/// </summary>
		/// <param name="source">source object, may be null</param>
		/// <param name="destination">destination object, may be null</param>
		/// <param name="context">matching context, which allows nested matches, services retrieval via DI, ...</param>
		/// <returns>true if the two objects are the same</returns>
		public bool Match(TSource? source, TDestination? destination, MatchingContext context);
	}
}
