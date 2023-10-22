using System.Collections.Generic;

namespace NeatMapper.Tests {
	public class CustomCollectionWithoutParameterlessConstructor<TElement> : List<TElement> {
		public CustomCollectionWithoutParameterlessConstructor(int capacity) { }
	}
}
