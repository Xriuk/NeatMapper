using System.Collections.Generic;

namespace NeatMapper.Tests.Classes {
	public class CustomCollectionWithoutParameterlessConstructor<TElement> : List<TElement> {
		public CustomCollectionWithoutParameterlessConstructor(int capacity) { }
	}
}
