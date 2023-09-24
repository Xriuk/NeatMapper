namespace NeatMapper.Core.Mapper {
	/// <summary>
	/// Exception thrown when no suitable collection element comparer was found for the given types
	/// </summary>
	public class CollectionElementComparerNotFound : ArgumentException {
		public CollectionElementComparerNotFound((Type From, Type To) types) :
			base($"No collection element comparer could be found for the given types: {types.From.Name} -> {types.To.Name}\n" +
			$"{types.From.FullName} -> {types.To.FullName}") { }
	}
}
