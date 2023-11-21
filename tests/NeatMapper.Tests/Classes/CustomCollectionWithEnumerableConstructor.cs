using System.Collections.Generic;

namespace NeatMapper.Tests {
	public class CustomCollectionWithEnumerableConstructor<TElement> : List<TElement> {
		public CustomCollectionWithEnumerableConstructor(IEnumerable<TElement> collection) : base(collection) { }
	}
}
